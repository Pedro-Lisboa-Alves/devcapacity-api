using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using DevCapacityApi.Models;
using DevCapacityApi.Repositories;

namespace DevCapacityApi.Messaging;

public class DefaultEngineerAssignmentProcessor : IEngineerAssignmentProcessor
{
    private readonly ILogger<DefaultEngineerAssignmentProcessor> _logger;
    private readonly ITasksRepository _tasksRepo;
    private readonly IEngineerRepository _engineerRepo;
    private readonly IEngineerCalendarRepository _engineerCalendarRepo;
    private readonly IEngineerAssignmentRepository _assignmentRepo;

    public DefaultEngineerAssignmentProcessor(
        ILogger<DefaultEngineerAssignmentProcessor> logger,
        ITasksRepository tasksRepo,
        IEngineerRepository engineerRepo,
        IEngineerCalendarRepository engineerCalendarRepo,
        IEngineerAssignmentRepository assignmentRepo)
    {
        _logger = logger;
        _tasksRepo = tasksRepo;
        _engineerRepo = engineerRepo;
        _engineerCalendarRepo = engineerCalendarRepo;
        _assignmentRepo = assignmentRepo;
    }

    public Task ProcessAssignmentAsync(EngineerAssignment assignment, string operation, CancellationToken cancellationToken = default)
    {
        if (assignment == null)
        {
            var msg = "Received null assignment in processor.";
            _logger.LogWarning(msg);
            Console.WriteLine(msg);
            return Task.CompletedTask;
        }

        try
        {
            switch ((operation ?? string.Empty).ToLowerInvariant())
            {
                case "created":
                    //HandleCreatedAssignment(assignment);
                    HandleCreatedAssignmentWithRebalance(assignment);
                    break;
                case "deleted":
                    HandleDeletedAssignment(assignment);
                    break;
                default:
                    _logger.LogInformation("Operation {Op} not handled. Ignoring.", operation);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessAssignmentAsync for AssignmentId={AssignmentId}", assignment.AssignmentId);
            Console.WriteLine($"Error processing assignment: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    private void HandleCreatedAssignment(EngineerAssignment assignment)
    {
        var task = _tasksRepo.GetById(assignment.TaskId);
        var engineer = _engineerRepo.GetById(assignment.EngineerId);

        if (task == null)
        {
            _logger.LogWarning("Task not found for TaskId={TaskId}", assignment.TaskId);
            Console.WriteLine($"Task not found for TaskId={assignment.TaskId}");
            return;
        }

        if (engineer == null)
        {
            _logger.LogWarning("Engineer not found for EngineerId={EngineerId}", assignment.EngineerId);
            Console.WriteLine($"Engineer not found for EngineerId={assignment.EngineerId}");
            return;
        }

        Console.WriteLine($"EngineerId: {engineer.Id}; Name: {engineer.Name}");

        var toAssign = task.UnassignedPDs;
        if (toAssign <= 0)
        {
            Console.WriteLine($"No unassigned PDs for TaskId={task.TaskId}");
            return;
        }

        var calendar = _engineerCalendarRepo.GetByEngineerId(engineer.Id);
        if (calendar == null || (calendar.CalendarDays == null && calendar.CalendarDays == null) )
        {
            Console.WriteLine($"No calendar/days for EngineerId={engineer.Id}");
            return;
        }

        // normalize collection reference
        var days = (calendar.CalendarDays as IList<EngineerCalendarDay>) ?? (calendar.CalendarDays as IList<EngineerCalendarDay>) ?? calendar.CalendarDays?.ToList();
        if (days == null || !days.Any())
        {
            Console.WriteLine($"No calendar days for EngineerId={engineer.Id}");
            return;
        }

        // clear AssignmentId on non-assigned days to ensure consistency (in memory only)
        foreach (var d in days)
        {
            if (d.Type != EngineerCalendarDayType.Assigned && d.AssignmentId != null)
                d.AssignmentId = null;
        }

        var availableDays = days
            .Where(d => d.Type == EngineerCalendarDayType.Available && d.Date.Date >= task.StartDate.Date)
            .OrderBy(d => d.Date)
            .ToList();

        if (!availableDays.Any())
        {
            Console.WriteLine($"No available days from {task.StartDate:yyyy-MM-dd} for EngineerId={engineer.Id}");
            return;
        }

        // prefer persisted assignment entity if available
        var persistedAssignment = _assignmentRepo.GetById(assignment.AssignmentId)
                              ?? _assignmentRepo.GetByEngineerId(assignment.EngineerId)
                                                 .FirstOrDefault(a => a.TaskId == assignment.TaskId);

        var assignedCount = 0;
        DateTime? lastAssignedDate = null;

        // collect only modified days to persist individually (prevents full replace that deletes days)
        var modifiedDays = new List<EngineerCalendarDay>();

        foreach (var day in availableDays)
        {
            if (toAssign <= 0) break;

            day.Type = EngineerCalendarDayType.Assigned;
            day.AssignmentId = persistedAssignment?.AssignmentId ?? assignment.AssignmentId;
            toAssign--;
            assignedCount++;
            lastAssignedDate = day.Date.Date;

            modifiedDays.Add(day);
        }

        if (assignedCount == 0)
        {
            Console.WriteLine($"No days could be assigned for EngineerId={engineer.Id} TaskId={task.TaskId}");
            return;
        }

        // persist only modified days (fix for bug: do not remove other days)
        var anyCalendarUpdateSuccess = false;
        foreach (var md in modifiedDays)
        {
            var ok = _engineerCalendarRepo.UpdateDay(md);
            anyCalendarUpdateSuccess = anyCalendarUpdateSuccess || ok;
        }

        Console.WriteLine($"Assigned {assignedCount} day(s) to EngineerId={engineer.Id} for TaskId={task.TaskId}. Calendar update success: {anyCalendarUpdateSuccess}");

        // extend task end date if needed
        if (lastAssignedDate.HasValue && lastAssignedDate.Value.Date > task.EndDate.Date)
        {
            var oldEnd = task.EndDate;
            task.EndDate = lastAssignedDate.Value.Date;
            var taskUpdated = _tasksRepo.Update(task);
            Console.WriteLine($"TaskId={task.TaskId} EndDate updated from {oldEnd:yyyy-MM-dd} to {task.EndDate:yyyy-MM-dd}. Update: {taskUpdated}");
        }

        // update assignment CapacityShare and dates if persisted
        if (persistedAssignment != null)
        {
            persistedAssignment.CapacityShare = assignedCount;
            persistedAssignment.StartDate = persistedAssignment.StartDate == default ? task.StartDate : persistedAssignment.StartDate;
            persistedAssignment.EndDate = lastAssignedDate.HasValue && lastAssignedDate.Value.Date > persistedAssignment.EndDate.Date
                ? lastAssignedDate.Value.Date
                : persistedAssignment.EndDate;

            var assignUpdated = _assignmentRepo.Update(persistedAssignment);
            Console.WriteLine($"EngineerAssignmentId={persistedAssignment.AssignmentId} CapacityShare updated to {persistedAssignment.CapacityShare}. Update: {assignUpdated}");
        }
        else
        {
            Console.WriteLine($"Warning: persisted assignment not found for AssignmentId={assignment.AssignmentId}; CapacityShare not updated.");
        }
    }

    private void HandleDeletedAssignment(EngineerAssignment assignment)
    {
        var calendar = _engineerCalendarRepo.GetByEngineerId(assignment.EngineerId);
        if (calendar == null)
        {
            Console.WriteLine($"No calendar found for EngineerId={assignment.EngineerId} when deleting assignment {assignment.AssignmentId}");
            return;
        }

        // unify collection reference (support Vacations or CalendarDays)
        var days = (calendar.CalendarDays as IEnumerable<EngineerCalendarDay>) ?? (calendar.CalendarDays as IEnumerable<EngineerCalendarDay>) ?? Enumerable.Empty<EngineerCalendarDay>();

        var daysToClear = days.Where(x => x.AssignmentId == assignment.AssignmentId).ToList();
        if (!daysToClear.Any())
        {
            Console.WriteLine($"No calendar days found with AssignmentId={assignment.AssignmentId} for EngineerId={assignment.EngineerId}");
            return;
        }

        var anyUpdated = false;
        foreach (var d in daysToClear)
        {
            d.AssignmentId = null;
            d.Type = EngineerCalendarDayType.Available;

            // persist only this day (does not remove other days)
            var ok = _engineerCalendarRepo.UpdateDay(d);
            anyUpdated = anyUpdated || ok;
        }

        Console.WriteLine($"Cleared assignment references in calendar for EngineerId={assignment.EngineerId}. Any day updated: {anyUpdated}");
    }

    // New: rebalance all assignments for the task using the received assignment as trigger.
    // Strategy:
    // 1) load all assignments for the task
    // 2) clear any calendar days currently assigned to these assignments (set Available + AssignmentId=null)
    // 3) collect all available days (Type == Available) from task.StartDate onward across involved engineers
    // 4) iterate days in chronological order and assign them to the assignment that belongs to that day.engineer
    //    (if engineer has multiple assignments, assign to the first one found)
    // 5) persist only modified days and update CapacityShare on each affected assignment
    private void HandleCreatedAssignmentWithRebalance(EngineerAssignment triggerAssignment)
    {
        var task = _tasksRepo.GetById(triggerAssignment.TaskId);
        if (task == null)
        {
            _logger.LogWarning("Rebalance: Task not found for TaskId={TaskId}", triggerAssignment.TaskId);
            Console.WriteLine($"Rebalance: Task not found for TaskId={triggerAssignment.TaskId}");
            return;
        }

        var allAssignments = _assignmentRepo.GetByTaskId(task.TaskId)?.ToList();
        if (allAssignments == null || !allAssignments.Any())
        {
            _logger.LogInformation("Rebalance: No assignments found for TaskId={TaskId}", task.TaskId);
            return;
        }

        // group assignments by engineer
        var assignmentsByEngineer = allAssignments
            .GroupBy(a => a.EngineerId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var assignmentIds = new HashSet<int>(allAssignments.Select(a => a.AssignmentId));

        // 1) Clear existing calendar day links for this task's assignments across all involved engineers
        var clearedDays = new List<EngineerCalendarDay>();
        foreach (var engId in assignmentsByEngineer.Keys)
        {
            var calendar = _engineerCalendarRepo.GetByEngineerId(engId);
            if (calendar == null) continue;

            var daysColl = (calendar.CalendarDays as IEnumerable<EngineerCalendarDay>) ?? (calendar.CalendarDays as IEnumerable<EngineerCalendarDay>) ?? Enumerable.Empty<EngineerCalendarDay>();

            var toClear = daysColl.Where(dd => dd.AssignmentId.HasValue && assignmentIds.Contains(dd.AssignmentId.Value)).ToList();
            foreach (var d in toClear)
            {
                d.AssignmentId = null;
                d.Type = EngineerCalendarDayType.Available;
                clearedDays.Add(d);
            }
        }

        // persist cleared days individually
        foreach (var d in clearedDays)
        {
            _engineerCalendarRepo.UpdateDay(d);
        }

        // 2) collect all available days from task.StartDate onward across involved engineers
        var candidateDays = new List<(DateTime Date, int EngineerId, EngineerCalendarDay Day)>();
        foreach (var engId in assignmentsByEngineer.Keys)
        {
            var calendar = _engineerCalendarRepo.GetByEngineerId(engId);
            if (calendar == null) continue;

            var daysColl = (calendar.CalendarDays as IEnumerable<EngineerCalendarDay>) ?? (calendar.CalendarDays as IEnumerable<EngineerCalendarDay>) ?? Enumerable.Empty<EngineerCalendarDay>();

            var avail = daysColl
                .Where(dd => dd.Type == EngineerCalendarDayType.Available && dd.Date.Date >= task.StartDate.Date)
                .Select(dd => (Date: dd.Date.Date, EngineerId: engId, Day: dd));

            candidateDays.AddRange(avail);
        }

        if (!candidateDays.Any())
        {
            _logger.LogInformation("Rebalance: No available days from {Start} for TaskId={TaskId}", task.StartDate.Date, task.TaskId);
            Console.WriteLine($"Rebalance: No available days from {task.StartDate:yyyy-MM-dd} for TaskId={task.TaskId}");
            return;
        }

        // sort by date so earliest days are used first
        candidateDays = candidateDays.OrderBy(t => t.Date).ToList();

        // remaining PDs to allocate = total PDs of the task
        var remaining = task.PDs;

        // counters
        var assignedCountsByAssignment = allAssignments.ToDictionary(a => a.AssignmentId, a => 0);
        var assignedCountsByEngineer = assignmentsByEngineer.Keys.ToDictionary(eid => eid, eid => 0);

        var modifiedDays = new List<EngineerCalendarDay>();

        // process days by date groups to allow multiple assignments on same date across different engineers
        var groupedByDate = candidateDays.GroupBy(x => x.Date).OrderBy(g => g.Key);
        foreach (var dateGroup in groupedByDate)
        {
            if (remaining <= 0) break;

            // list of available engineers for this date
            var entries = dateGroup.Select(x => (EngineerId: x.EngineerId, Day: x.Day)).ToList();

            // while there are free PDs and available engineers on this date
            while (remaining > 0 && entries.Any())
            {
                // choose engineer with smallest assigned count so far among available ones (balance)
                var chosenEngineer = entries
                    .Select(e => e.EngineerId)
                    .Distinct()
                    .OrderBy(eid => assignedCountsByEngineer.ContainsKey(eid) ? assignedCountsByEngineer[eid] : 0)
                    .ThenBy(eid => eid)
                    .FirstOrDefault();

                if (chosenEngineer == 0 && !assignedCountsByEngineer.ContainsKey(0)) break; // safety

                // pick an available entry for that engineer for this date
                var entryIndex = entries.FindIndex(e => e.EngineerId == chosenEngineer);
                if (entryIndex < 0) break;

                var chosenEntry = entries[entryIndex];
                // pick target assignment for this engineer: the assignment with least assigned so far
                var engAssignments = assignmentsByEngineer[chosenEngineer];
                var targetAssignment = engAssignments
                    .OrderBy(a => assignedCountsByAssignment.ContainsKey(a.AssignmentId) ? assignedCountsByAssignment[a.AssignmentId] : 0)
                    .First();

                // assign the day
                chosenEntry.Day.Type = EngineerCalendarDayType.Assigned;
                chosenEntry.Day.AssignmentId = targetAssignment.AssignmentId;

                // update counters
                assignedCountsByAssignment[targetAssignment.AssignmentId] = assignedCountsByAssignment[targetAssignment.AssignmentId] + 1;
                assignedCountsByEngineer[chosenEngineer] = assignedCountsByEngineer[chosenEngineer] + 1;
                remaining--;

                modifiedDays.Add(chosenEntry.Day);

                // remove this entry so we don't assign same day twice
                entries.RemoveAt(entryIndex);
            }
        }

        // persist modified days
        foreach (var d in modifiedDays)
        {
            _engineerCalendarRepo.UpdateDay(d);
        }

        var lastAssignedDate = modifiedDays.Any() ? modifiedDays.Max(d => d.Date) : (DateTime?)null;

        // update assignments CapacityShare and dates
        foreach (var a in allAssignments)
        {
            assignedCountsByAssignment.TryGetValue(a.AssignmentId, out var count);

            // update persisted assignment if changed
            var needUpdate = a.CapacityShare != count;
            if (!needUpdate && lastAssignedDate.HasValue)
            {
                var currentEnd = a.EndDate == default ? DateTime.MinValue : a.EndDate.Date;
                if (lastAssignedDate.Value.Date > currentEnd) needUpdate = true;
            }

            if (needUpdate)
            {
                a.CapacityShare = count;
                a.StartDate = a.StartDate == default ? task.StartDate : a.StartDate;
                a.EndDate = lastAssignedDate.HasValue && lastAssignedDate.Value.Date > (a.EndDate == default ? DateTime.MinValue : a.EndDate.Date)
                    ? lastAssignedDate.Value.Date
                    : a.EndDate;

                _assignmentRepo.Update(a);
            }
        }

        // update task end date if extended
        if (lastAssignedDate.HasValue && lastAssignedDate.Value.Date > task.EndDate.Date)
        {
            var oldEnd = task.EndDate;
            task.EndDate = lastAssignedDate.Value.Date;
            _tasksRepo.Update(task);
            Console.WriteLine($"Rebalance: TaskId={task.TaskId} EndDate updated from {oldEnd:yyyy-MM-dd} to {task.EndDate:yyyy-MM-dd}");
        }

        _logger.LogInformation("Rebalance completed for TaskId={TaskId}. Remaining PDs unallocated: {Remaining}", task.TaskId, remaining);
        Console.WriteLine($"Rebalance completed for TaskId={task.TaskId}. Remaining PDs unallocated: {remaining}");
    }
}
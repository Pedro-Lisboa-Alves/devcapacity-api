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
                    HandleCreatedAssignment(assignment);
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
}
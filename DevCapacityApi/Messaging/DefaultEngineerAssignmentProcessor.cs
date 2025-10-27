using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

    public DefaultEngineerAssignmentProcessor(ILogger<DefaultEngineerAssignmentProcessor> logger,
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
            _logger.LogWarning("Received null assignment in processor.");
            Console.WriteLine("Received null assignment in processor.");
            return Task.CompletedTask;
        }

        try
        {
            var task = _tasksRepo.GetById(assignment.TaskId);
            var engineer = _engineerRepo.GetById(assignment.EngineerId);

            if (task == null)
            {
                var msg = $"Task not found for TaskId={assignment.TaskId}";
                _logger.LogWarning(msg);
                Console.WriteLine(msg);
                return Task.CompletedTask;
            }

            if (engineer == null)
            {
                var msg = $"Engineer not found for EngineerId={assignment.EngineerId}";
                _logger.LogWarning(msg);
                Console.WriteLine(msg);
                return Task.CompletedTask;
            }

            // log engineer name
            var engLog = $"EngineerId: {engineer.Id}; Name: {engineer.Name}";
            _logger.LogInformation(engLog);
            Console.WriteLine(engLog);

            // compute how many PDs need assignment
            var toAssign = task.UnassignedPDs;
            if (toAssign <= 0)
            {
                var msg = $"No unassigned PDs for TaskId={task.TaskId}";
                _logger.LogInformation(msg);
                Console.WriteLine(msg);
                return Task.CompletedTask;
            }

            // get engineer calendar
            var calendar = _engineerCalendarRepo.GetByEngineerId(engineer.Id);
            if (calendar == null || (calendar.CalendarDays == null && calendar.CalendarDays == null))
            {
                var msg = $"No calendar found for EngineerId={engineer.Id}";
                _logger.LogWarning(msg);
                Console.WriteLine(msg);
                return Task.CompletedTask;
            }

            // unify days collection name (support both Vacations or CalendarDays)
            var days = (calendar.CalendarDays as IList<EngineerCalendarDay>) ??
                       (calendar.CalendarDays as IList<EngineerCalendarDay>) ??
                       calendar.CalendarDays?.ToList();

            if (days == null)
            {
                var msg = $"No calendar days for EngineerId={engineer.Id}";
                _logger.LogWarning(msg);
                Console.WriteLine(msg);
                return Task.CompletedTask;
            }

            // find available days from task.StartDate forward
            var availableDays = days
                .Where(d => d.Type == EngineerCalendarDayType.Available && d.Date.Date >= task.StartDate.Date)
                .OrderBy(d => d.Date)
                .ToList();

            if (!availableDays.Any())
            {
                var msg = $"No available days from {task.StartDate:yyyy-MM-dd} for EngineerId={engineer.Id}";
                _logger.LogInformation(msg);
                Console.WriteLine(msg);
                return Task.CompletedTask;
            }

            var assignedCount = 0;
            DateTime? lastAssignedDate = null;

            foreach (var day in availableDays)
            {
                if (toAssign <= 0) break;

                day.Type = EngineerCalendarDayType.Assigned;
                toAssign--;
                assignedCount++;
                lastAssignedDate = day.Date.Date;
            }

            if (assignedCount > 0)
            {
                // persist calendar changes
                var calendarUpdated = _engineerCalendarRepo.Update(calendar);
                var msg = $"Assigned {assignedCount} day(s) to EngineerId={engineer.Id} for TaskId={task.TaskId}. Calendar update success: {calendarUpdated}";
                _logger.LogInformation(msg);
                Console.WriteLine(msg);

                // update task end date if extended
                if (lastAssignedDate.HasValue && lastAssignedDate.Value.Date > task.EndDate.Date)
                {
                    var oldEnd = task.EndDate;
                    task.EndDate = lastAssignedDate.Value.Date;
                    var taskUpdated = _tasksRepo.Update(task);
                    var updateMsg = $"TaskId={task.TaskId} EndDate updated from {oldEnd:yyyy-MM-dd} to {task.EndDate:yyyy-MM-dd}. Update success: {taskUpdated}";
                    _logger.LogInformation(updateMsg);
                    Console.WriteLine(updateMsg);
                }

                // update the EngineerAssignment CapacityShare to the number of PDs allocated
                try
                {
                    // try to load the persisted assignment entity
                    var persisted = _assignmentRepo.GetById(assignment.AssignmentId) ?? _assignmentRepo
                        .GetByEngineerId(assignment.EngineerId)
                        .FirstOrDefault(a => a.TaskId == assignment.TaskId);

                    if (persisted != null)
                    {
                        persisted.CapacityShare = assignedCount;
                        // also update StartDate/EndDate if desired (optional)
                        persisted.StartDate = persisted.StartDate == default ? task.StartDate : persisted.StartDate;
                        persisted.EndDate = lastAssignedDate.HasValue && lastAssignedDate.Value.Date > persisted.EndDate.Date ? lastAssignedDate.Value.Date : persisted.EndDate;

                        var assignUpdated = _assignmentRepo.Update(persisted);
                        var assignMsg = $"EngineerAssignmentId={persisted.AssignmentId} CapacityShare updated to {persisted.CapacityShare}. Update success: {assignUpdated}";
                        _logger.LogInformation(assignMsg);
                        Console.WriteLine(assignMsg);
                    }
                    else
                    {
                        var warn = $"Could not find persisted EngineerAssignment for AssignmentId={assignment.AssignmentId} to update CapacityShare.";
                        _logger.LogWarning(warn);
                        Console.WriteLine(warn);
                    }
                }
                catch (Exception exAssign)
                {
                    _logger.LogError(exAssign, "Error updating EngineerAssignment CapacityShare for AssignmentId {AssignmentId}", assignment.AssignmentId);
                    Console.WriteLine($"Error updating assignment CapacityShare: {exAssign.Message}");
                }
            }
            else
            {
                var msg = $"No days were assigned for EngineerId={engineer.Id} TaskId={task.TaskId}";
                _logger.LogInformation(msg);
                Console.WriteLine(msg);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing assignment for TaskId {TaskId} and EngineerId {EngineerId}", assignment?.TaskId, assignment?.EngineerId);
            Console.WriteLine($"Error processing assignment: {ex.Message}");
        }

        return Task.CompletedTask;
    }
}
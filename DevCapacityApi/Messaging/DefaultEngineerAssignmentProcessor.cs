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

            Console.WriteLine($"EngineerId: {engineer.Id}; Name: {engineer.Name}");

            var toAssign = task.UnassignedPDs;
            if (toAssign <= 0)
            {
                var msg = $"No unassigned PDs for TaskId={task.TaskId}";
                _logger.LogInformation(msg);
                Console.WriteLine(msg);
                return Task.CompletedTask;
            }

            var calendar = _engineerCalendarRepo.GetByEngineerId(engineer.Id);
            if (calendar == null || calendar.CalendarDays == null)
            {
                var msg = $"No calendar found for EngineerId={engineer.Id}";
                _logger.LogWarning(msg);
                Console.WriteLine(msg);
                return Task.CompletedTask;
            }

            // Ensure AssignmentId is null for days that are not Assigned
            foreach (var d in calendar.CalendarDays)
            {
                if (d.Type != EngineerCalendarDayType.Assigned && d.AssignmentId != null)
                    d.AssignmentId = null;
            }

            var availableDays = calendar.CalendarDays
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

            // try to find persisted assignment (prefer persisted record)
            var persistedAssignment = _assignmentRepo.GetById(assignment.AssignmentId)
                                      ?? _assignmentRepo.GetByEngineerId(assignment.EngineerId)
                                                         .FirstOrDefault(a => a.TaskId == assignment.TaskId);

            // assign days up to toAssign, mark Type and AssignmentId
            foreach (var day in availableDays)
            {
                if (toAssign <= 0) break;

                day.Type = EngineerCalendarDayType.Assigned;
                // only set AssignmentId when Type == Assigned
                day.AssignmentId = persistedAssignment?.AssignmentId ?? assignment.AssignmentId;
                toAssign--;
                assignedCount++;
                lastAssignedDate = day.Date.Date;
            }

            if (assignedCount > 0)
            {
                // persist calendar changes (repo must persist AssignmentId)
                var calendarUpdated = _engineerCalendarRepo.Update(calendar);
                Console.WriteLine($"Assigned {assignedCount} day(s) to EngineerId={engineer.Id} for TaskId={task.TaskId}. Calendar update success: {calendarUpdated}");

                // extend task end date if needed
                if (lastAssignedDate.HasValue && lastAssignedDate.Value.Date > task.EndDate.Date)
                {
                    var oldEnd = task.EndDate;
                    task.EndDate = lastAssignedDate.Value.Date;
                    var taskUpdated = _tasksRepo.Update(task);
                    Console.WriteLine($"TaskId={task.TaskId} EndDate updated from {oldEnd:yyyy-MM-dd} to {task.EndDate:yyyy-MM-dd}. Update success: {taskUpdated}");
                }

                // update the persisted EngineerAssignment CapacityShare and dates
                if (persistedAssignment != null)
                {
                    persistedAssignment.CapacityShare = assignedCount;
                    persistedAssignment.StartDate = persistedAssignment.StartDate == default ? task.StartDate : persistedAssignment.StartDate;
                    persistedAssignment.EndDate = lastAssignedDate.HasValue && lastAssignedDate.Value.Date > persistedAssignment.EndDate.Date
                        ? lastAssignedDate.Value.Date
                        : persistedAssignment.EndDate;

                    var assignUpdated = _assignmentRepo.Update(persistedAssignment);
                    Console.WriteLine($"EngineerAssignmentId={persistedAssignment.AssignmentId} CapacityShare updated to {persistedAssignment.CapacityShare}. Update success: {assignUpdated}");
                }
                else
                {
                    Console.WriteLine($"Warning: no persisted assignment found for AssignmentId={assignment.AssignmentId}; CapacityShare not updated.");
                }
            }
            else
            {
                Console.WriteLine($"No days were assigned for EngineerId={engineer.Id} TaskId={task.TaskId}");
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
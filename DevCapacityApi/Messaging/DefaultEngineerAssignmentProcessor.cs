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

    public DefaultEngineerAssignmentProcessor(ILogger<DefaultEngineerAssignmentProcessor> logger,
                                             ITasksRepository tasksRepo,
                                             IEngineerRepository engineerRepo,
                                             IEngineerCalendarRepository engineerCalendarRepo)
    {
        _logger = logger;
        _tasksRepo = tasksRepo;
        _engineerRepo = engineerRepo;
        _engineerCalendarRepo = engineerCalendarRepo;
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
            if (calendar == null || calendar.CalendarDays == null)
            {
                var msg = $"No calendar found for EngineerId={engineer.Id}";
                _logger.LogWarning(msg);
                Console.WriteLine(msg);
                return Task.CompletedTask;
            }

            // find available days from task.StartDate forward
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
                // persist changes
                var updated = _engineerCalendarRepo.Update(calendar);
                var msg = $"Assigned {assignedCount} day(s) to EngineerId={engineer.Id} for TaskId={task.TaskId}. Calendar update success: {updated}";
                _logger.LogInformation(msg);
                Console.WriteLine(msg);

                // if last assigned date extends beyond task.EndDate, update task.EndDate
                if (lastAssignedDate.HasValue && lastAssignedDate.Value.Date > task.EndDate.Date)
                {
                    var oldEnd = task.EndDate;
                    task.EndDate = lastAssignedDate.Value.Date;
                    var taskUpdated = _tasksRepo.Update(task);
                    var updateMsg = $"TaskId={task.TaskId} EndDate updated from {oldEnd:yyyy-MM-dd} to {task.EndDate:yyyy-MM-dd}. Update success: {taskUpdated}";
                    _logger.LogInformation(updateMsg);
                    Console.WriteLine(updateMsg);
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
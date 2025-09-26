using System;
using System.Collections.Generic;
using System.Linq;
using DevCapacityApi.DTOs;
using DevCapacityApi.Models;
using DevCapacityApi.Repositories;

namespace DevCapacityApi.Services;

public class TasksService : ITasksService
{
    private readonly ITasksRepository _repo;
    public TasksService(ITasksRepository repo) => _repo = repo;

    public TasksDto Create(CreateUpdateTasksDto dto)
    {
        if (_repo.GetByName(dto.Name) is not null)
            throw new InvalidOperationException("Task name must be unique.");

        var t = new Tasks
        {
            Name = dto.Name,
            Initiative = dto.Initiative,
            Status = dto.Status,
            PDs = dto.PDs,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };

        var created = _repo.Add(t);
        return ToDto(created);
    }

    public IEnumerable<TasksDto> GetAll() => _repo.GetAll().Select(ToDto);

    public TasksDto? GetById(int id)
    {
        var t = _repo.GetById(id);
        return t == null ? null : ToDto(t);
    }

    public bool Update(int id, CreateUpdateTasksDto dto)
    {
        var existing = _repo.GetById(id);
        if (existing == null) return false;

        var byName = _repo.GetByName(dto.Name);
        if (byName != null && byName.TaskId != id)
            throw new InvalidOperationException("Task name duplicate.");

        existing.Name = dto.Name;
        existing.Initiative = dto.Initiative;
        existing.Status = dto.Status;
        existing.PDs = dto.PDs;
        existing.StartDate = dto.StartDate;
        existing.EndDate = dto.EndDate;

        return _repo.Update(existing);
    }

    public bool Delete(int id) => _repo.Delete(id);

    private static TasksDto ToDto(Tasks t) =>
        new TasksDto
        {
            TaskId = t.TaskId,
            Name = t.Name,
            Initiative = t.Initiative,
            Status = t.Status,
            PDs = t.PDs,
            StartDate = t.StartDate,
            EndDate = t.EndDate,
            AssignmentIds = t.Assignments?.Select(a => a.AssignmentId) ?? Enumerable.Empty<int>()
        };
}
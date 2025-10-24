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
        var entity = new Tasks
        {
            Name = dto.Name,
            Initiative = dto.Initiative,
            Status = dto.Status,
            PDs = dto.PDs,
            MaxResources = dto.MaxResources,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };

        var created = _repo.Add(entity);
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

        existing.Name = dto.Name;
        existing.Initiative = dto.Initiative;
        existing.Status = dto.Status;
        existing.PDs = dto.PDs;
        existing.MaxResources = dto.MaxResources;
        existing.StartDate = dto.StartDate;
        existing.EndDate = dto.EndDate;

        return _repo.Update(existing);
    }

    public bool Delete(int id)
    {
        var existing = _repo.GetById(id);
        if (existing == null) return false;
        return _repo.Delete(id);
    }

    private static TasksDto ToDto(Tasks t) =>
        new TasksDto
        {
            TaskId = t.TaskId,
            Name = t.Name,
            Initiative = t.Initiative,
            Status = t.Status,
            PDs = t.PDs,
            MaxResources = t.MaxResources,
            StartDate = t.StartDate,
            EndDate = t.EndDate,
            AssignmentIds = t.Assignments?.Select(a => a.AssignmentId) ?? Enumerable.Empty<int>(),
            UnassignedPDs = t.UnassignedPDs
        };
}
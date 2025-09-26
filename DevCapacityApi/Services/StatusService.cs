using System;
using System.Collections.Generic;
using System.Linq;
using DevCapacityApi.DTOs;
using DevCapacityApi.Models;
using DevCapacityApi.Repositories;

namespace DevCapacityApi.Services;

public class StatusService : IStatusService
{
    private readonly IStatusRepository _repo;
    public StatusService(IStatusRepository repo) => _repo = repo;

    public StatusDto Create(CreateUpdateStatusDto dto)
    {
        if (_repo.GetByName(dto.Name) is not null)
            throw new InvalidOperationException("Status name must be unique.");

        var s = new Status { Name = dto.Name };
        var created = _repo.Add(s);
        return ToDto(created);
    }

    public IEnumerable<StatusDto> GetAll() => _repo.GetAll().Select(ToDto);

    public StatusDto? GetById(int id)
    {
        var s = _repo.GetById(id);
        return s == null ? null : ToDto(s);
    }

    public bool Update(int id, CreateUpdateStatusDto dto)
    {
        var existing = _repo.GetById(id);
        if (existing == null) return false;

        var byName = _repo.GetByName(dto.Name);
        if (byName != null && byName.StatusId != id)
            throw new InvalidOperationException("Status name duplicate.");

        existing.Name = dto.Name;
        return _repo.Update(existing);
    }

    public bool Delete(int id) => _repo.Delete(id);

    private static StatusDto ToDto(Status s) =>
        new StatusDto { StatusId = s.StatusId, Name = s.Name };
}
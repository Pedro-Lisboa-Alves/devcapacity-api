using System;
using System.Collections.Generic;
using System.Linq;
using DevCapacityApi.DTOs;
using DevCapacityApi.Models;
using DevCapacityApi.Repositories;

namespace DevCapacityApi.Services;

public class InitiativesService : IInitiativesService
{
    private readonly IInitiativesRepository _repo;
    public InitiativesService(IInitiativesRepository repo) => _repo = repo;

    public InitiativesDto Create(CreateUpdateInitiativesDto dto)
    {
        if (_repo.GetByName(dto.Name) is not null)
            throw new InvalidOperationException("Initiative name must be unique.");

        if (dto.ParentInitiative.HasValue && _repo.GetById(dto.ParentInitiative.Value) == null)
            throw new InvalidOperationException("Parent initiative does not exist.");

        var i = new Initiatives
        {
            Name = dto.Name,
            ParentInitiative = dto.ParentInitiative,
            Status = dto.Status,
            PDs = dto.PDs,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };

        var created = _repo.Add(i);
        return ToDto(created);
    }

    public IEnumerable<InitiativesDto> GetAll() => _repo.GetAll().Select(ToDto);

    public InitiativesDto? GetById(int id)
    {
        var i = _repo.GetById(id);
        return i == null ? null : ToDto(i);
    }

    public bool Update(int id, CreateUpdateInitiativesDto dto)
    {
        var existing = _repo.GetById(id);
        if (existing == null) return false;

        var byName = _repo.GetByName(dto.Name);
        if (byName != null && byName.InitiativeId != id)
            throw new InvalidOperationException("Initiative name duplicate.");

        if (dto.ParentInitiative.HasValue && dto.ParentInitiative.Value == id)
            throw new InvalidOperationException("Initiative cannot be parent of itself.");

        if (dto.ParentInitiative.HasValue && _repo.GetById(dto.ParentInitiative.Value) == null)
            throw new InvalidOperationException("Parent initiative does not exist.");

        existing.Name = dto.Name;
        existing.ParentInitiative = dto.ParentInitiative;
        existing.Status = dto.Status;
        existing.PDs = dto.PDs;
        existing.StartDate = dto.StartDate;
        existing.EndDate = dto.EndDate;

        return _repo.Update(existing);
    }

    public bool Delete(int id) => _repo.Delete(id);

    private static InitiativesDto ToDto(Initiatives i) =>
        new InitiativesDto
        {
            InitiativeId = i.InitiativeId,
            Name = i.Name,
            ParentInitiative = i.ParentInitiative,
            Status = i.Status,
            PDs = i.PDs,
            StartDate = i.StartDate,
            EndDate = i.EndDate,
            TaskIds = i.Tasks?.Select(t => t.TaskId) ?? Enumerable.Empty<int>()
        };
}
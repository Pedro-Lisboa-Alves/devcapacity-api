using System;
using System.Collections.Generic;
using System.Linq;
using DevCapacityApi.DTOs;
using DevCapacityApi.Models;
using DevCapacityApi.Repositories;

namespace DevCapacityApi.Services;

public class EngineerService : IEngineerService
{
    private readonly IEngineerRepository _repo;

    public EngineerService(IEngineerRepository repo)
    {
        _repo = repo;
    }

    public IEnumerable<EngineerDto> GetAll() =>
        _repo.GetAll().Select(MapToDto);

    public EngineerDto? GetById(int id)
    {
        var e = _repo.GetById(id);
        return e is null ? null : MapToDto(e);
    }

    public EngineerDto Create(EngineerDto dto)
    {
        // valida duplicado (case-insensitive)
        if (!string.IsNullOrWhiteSpace(dto.Name) && _repo.GetByName(dto.Name) is not null)
            throw new InvalidOperationException("Engineer name already exists.");

        var entity = new Engineer
        {
            Name = dto.Name!,
            Role = dto.Role,
            DailyCapacity = dto.DailyCapacity,
            TeamId = dto.TeamId
        };

        var created = _repo.Add(entity);
        return MapToDto(created);
    }

    public bool Update(int id, EngineerDto dto)
    {
        var existing = _repo.GetById(id);
        if (existing is null) return false;

        // se mudou o name, verificar duplicado
        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            var other = _repo.GetByName(dto.Name);
            if (other is not null && other.Id != id)
                throw new InvalidOperationException("Engineer name already exists.");
        }

        // atualizar a entidade existente (inclui TeamId)
        existing.Name = dto.Name ?? existing.Name;
        existing.Role = dto.Role;
        existing.DailyCapacity = dto.DailyCapacity;
        existing.TeamId = dto.TeamId;

        _repo.Update(existing);
        return true;
    }

    public bool Delete(int id)
    {
        var existing = _repo.GetById(id);
        if (existing is null) return false;
        _repo.Delete(id);
        return true;
    }

    private static EngineerDto MapToDto(Engineer e) =>
        new EngineerDto
        {
            Id = e.Id,
            Name = e.Name,
            Role = e.Role,
            DailyCapacity = e.DailyCapacity,
            TeamId = e.TeamId
        };

    private static Engineer MapToEntity(EngineerDto d) =>
        new Engineer
        {
            Id = d.Id,
            Name = d.Name!,
            Role = d.Role,
            DailyCapacity = d.DailyCapacity,
            TeamId = d.TeamId
        };
}
using System;
using System.Collections.Generic;
using System.Linq;
using DevCapacityApi.DTOs;
using DevCapacityApi.Models;
using DevCapacityApi.Repositories;

namespace DevCapacityApi.Services;

public class TeamService : ITeamService
{
    private readonly ITeamRepository _repo;
    public TeamService(ITeamRepository repo) => _repo = repo;

    public TeamDto Create(CreateUpdateTeamDto dto)
    {
        if (_repo.GetByName(dto.Name) != null)
            throw new InvalidOperationException("Team name must be unique.");

        if (dto.ParentTeam.HasValue && _repo.GetById(dto.ParentTeam.Value) == null)
            throw new InvalidOperationException("Parent team does not exist.");

        var team = new Team { Name = dto.Name, ParentTeam = dto.ParentTeam };
        var created = _repo.Add(team);
        return ToDto(created);
    }

    public IEnumerable<TeamDto> GetAll() =>
        _repo.GetAll().Select(ToDto);

    public TeamDto? GetById(int id)
    {
        var t = _repo.GetById(id);
        return t == null ? null : ToDto(t);
    }

    public bool Update(int id, CreateUpdateTeamDto dto)
    {
        var existing = _repo.GetById(id);
        if (existing == null) return false;

        var byName = _repo.GetByName(dto.Name);
        if (byName != null && byName.TeamId != id)
            throw new InvalidOperationException("Team name duplicate.");

        if (dto.ParentTeam.HasValue && dto.ParentTeam.Value == id)
            throw new InvalidOperationException("Team cannot be parent of itself.");

        if (dto.ParentTeam.HasValue && _repo.GetById(dto.ParentTeam.Value) == null)
            throw new InvalidOperationException("Parent team does not exist.");

        existing.Name = dto.Name;
        existing.ParentTeam = dto.ParentTeam;
        return _repo.Update(existing);
    }

    public bool Delete(int id) => _repo.Delete(id);

    private static TeamDto ToDto(Team t) =>
        new TeamDto
        {
            TeamId = t.TeamId,
            Name = t.Name,
            ParentTeam = t.ParentTeam,
            EngineerIds = t.Engineers?.Select(e => e.Id) ?? Enumerable.Empty<int>()
        };
}
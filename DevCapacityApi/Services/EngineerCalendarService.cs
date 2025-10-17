using System;
using System.Collections.Generic;
using System.Linq;
using DevCapacityApi.DTOs;
using DevCapacityApi.Models;
using DevCapacityApi.Repositories;

namespace DevCapacityApi.Services;

public class EngineerCalendarService : IEngineerCalendarService
{
    private readonly IEngineerCalendarRepository _repo;
    private readonly IEngineerRepository _engineerRepo;

    public EngineerCalendarService(IEngineerCalendarRepository repo, IEngineerRepository engineerRepo)
    {
        _repo = repo;
        _engineerRepo = engineerRepo;
    }

    public EngineerCalendarDto Create(CreateUpdateEngineerCalendarDto dto)
    {
        // validate engineer exists
        var eng = _engineerRepo.GetById(dto.EngineerId);
        if (eng is null) throw new InvalidOperationException("Engineer not found.");

        var entity = new EngineerCalendar
        {
            EngineerId = dto.EngineerId,
            Vacations = dto.Vacations.Select(d => new EngineerCalendarDay { Date = d.Date }).ToList()
        };

        var created = _repo.Add(entity);
        return MapToDto(created);
    }

    public IEnumerable<EngineerCalendarDto> GetAll() => _repo.GetAll().Select(MapToDto);

    public EngineerCalendarDto? GetById(int id)
    {
        var c = _repo.GetById(id);
        return c == null ? null : MapToDto(c);
    }

    public EngineerCalendarDto? GetByEngineerId(int engineerId)
    {
        var c = _repo.GetByEngineerId(engineerId);
        return c == null ? null : MapToDto(c);
    }

    public bool Update(int id, CreateUpdateEngineerCalendarDto dto)
    {
        var existing = _repo.GetById(id);
        if (existing == null) return false;

        // validate engineer exists
        var eng = _engineerRepo.GetById(dto.EngineerId);
        if (eng is null) throw new InvalidOperationException("Engineer not found.");

        existing.EngineerId = dto.EngineerId;
        existing.Vacations = dto.Vacations.Select(d => new EngineerCalendarDay { EngineerCalendarId = id, Date = d.Date }).ToList();

        return _repo.Update(existing);
    }

    public bool Delete(int id) => _repo.Delete(id);

    public bool IsVacation(int calendarId, DateTime date)
    {
        var c = _repo.GetById(calendarId) ?? throw new InvalidOperationException("EngineerCalendar not found.");
        return c.IsVacation(date);
    }

    private static EngineerCalendarDto MapToDto(EngineerCalendar c) =>
        new EngineerCalendarDto
        {
            EngineerCalendarId = c.EngineerCalendarId,
            EngineerId = c.EngineerId,
            Vacations = c.Vacations?.Select(v => v.Date) ?? Enumerable.Empty<DateTime>()
        };
}
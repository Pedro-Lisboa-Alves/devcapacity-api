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

    public EngineerCalendarService(IEngineerCalendarRepository repo) => _repo = repo;

    public EngineerCalendarDto Create(CreateUpdateEngineerCalendarDto dto)
    {
        var entity = new EngineerCalendar
        {
            EngineerId = dto.EngineerId,
            CalendarDays = dto.Days
                .Select(d => new EngineerCalendarDay
                {
                    Date = d.Date.Date,
                    Type = ParseType(d.Type)
                })
                .ToList()
        };

        var created = _repo.Add(entity);
        return ToDto(created);
    }

    public IEnumerable<EngineerCalendarDto> GetAll() => _repo.GetAll().Select(ToDto);

    public EngineerCalendarDto? GetById(int id)
    {
        var c = _repo.GetById(id);
        return c == null ? null : ToDto(c);
    }

    public EngineerCalendarDto? GetByEngineerId(int engineerId)
    {
        var c = _repo.GetByEngineerId(engineerId);
        return c == null ? null : ToDto(c);
    }

    public bool Update(int id, CreateUpdateEngineerCalendarDto dto)
    {
        var existing = _repo.GetById(id);
        if (existing == null) return false;

        existing.EngineerId = dto.EngineerId;
        // replace days
        existing.CalendarDays = dto.Days
            .Select(d => new EngineerCalendarDay
            {
                EngineerCalendarId = id,
                Date = d.Date.Date,
                Type = ParseType(d.Type)
            })
            .ToList();

        return _repo.Update(existing);
    }

    public bool Delete(int id) => _repo.Delete(id);

    public bool IsVacation(int calendarId, DateTime date)
    {
        var c = _repo.GetById(calendarId) ?? throw new InvalidOperationException("EngineerCalendar not found.");
        return c.IsVacation(date);
    }

    private static EngineerCalendarDto ToDto(EngineerCalendar c) =>
        new EngineerCalendarDto
        {
            EngineerCalendarId = c.EngineerCalendarId,
            EngineerId = c.EngineerId,
            Days = c.CalendarDays?.Select(v => new EngineerCalendarDayDto
            {
                Id = v.Id,
                Date = v.Date,
                Type = v.Type.ToString(),
                AssignmentId = v.AssignmentId
            }) ?? Enumerable.Empty<EngineerCalendarDayDto>()
        };

    private static EngineerCalendarDayType ParseType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type)) return EngineerCalendarDayType.Available;
        return Enum.TryParse<EngineerCalendarDayType>(type, true, out var t) ? t : EngineerCalendarDayType.Available;
    }
}
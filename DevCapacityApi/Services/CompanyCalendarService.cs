using System;
using System.Collections.Generic;
using System.Linq;
using DevCapacityApi.DTOs;
using DevCapacityApi.Models;
using DevCapacityApi.Repositories;

namespace DevCapacityApi.Services;

public class CompanyCalendarService : ICompanyCalendarService
{
    private readonly ICompanyCalendarRepository _repo;

    public CompanyCalendarService(ICompanyCalendarRepository repo) => _repo = repo;

    public CompanyCalendarDto Create(CreateUpdateCompanyCalendarDto dto)
    {
        var entity = new CompanyCalendar
        {
            NonWorkingDays = dto.NonWorkingDays
                .Select(d => new CompanyCalendarNonWorkingDay { Day = d })
                .ToList()
        };

        var created = _repo.Add(entity);
        return ToDto(created);
    }

    public IEnumerable<CompanyCalendarDto> GetAll() => _repo.GetAll().Select(ToDto);

    public CompanyCalendarDto? GetById(int id)
    {
        var c = _repo.GetById(id);
        return c == null ? null : ToDto(c);
    }

    public bool Update(int id, CreateUpdateCompanyCalendarDto dto)
    {
        var existing = _repo.GetById(id);
        if (existing == null) return false;

        existing.NonWorkingDays = dto.NonWorkingDays.Select(d => new CompanyCalendarNonWorkingDay { CompanyCalendarId = id, Day = d }).ToList();

        return _repo.Update(existing);
    }

    public bool Delete(int id) => _repo.Delete(id);

    public bool IsCompanyWorkingDay(int calendarId, DateTime date)
    {
        var c = _repo.GetById(calendarId);
        if (c == null) throw new InvalidOperationException("CompanyCalendar not found.");
        return c.IsCompanyWorkingDay(date);
    }

    private static CompanyCalendarDto ToDto(CompanyCalendar c) =>
        new CompanyCalendarDto
        {
            CompanyCalendarId = c.CompanyCalendarId,
            NonWorkingDays = c.NonWorkingDays?.Select(n => n.Day) ?? Enumerable.Empty<DayOfWeek>()
        };
}
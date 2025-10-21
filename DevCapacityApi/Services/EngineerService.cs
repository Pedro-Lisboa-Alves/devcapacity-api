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
    private readonly IEngineerCalendarService _calendarService;

    public EngineerService(IEngineerRepository repo, IEngineerCalendarService calendarService)
    {
        _repo = repo;
        _calendarService = calendarService;
    }

    public IEnumerable<EngineerDto> GetAll()
    {
        var items = _repo.GetAll();
        return items.Select(MapToDto);
    }

    public EngineerDto? GetById(int id)
    {
        var e = _repo.GetById(id);
        return e == null ? null : MapToDto(e);
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

        // if calendar present in DTO, create it
        if (dto.EngineerCalendar is not null)
        {
            var createCalDto = new CreateUpdateEngineerCalendarDto
            {
                EngineerId = created.Id,
                Days = dto.EngineerCalendar.Days?.Select(d => new CreateUpdateEngineerCalendarDayDto
                {
                    Date = d.Date.Date,
                    Type = d.Type
                }) ?? Array.Empty<CreateUpdateEngineerCalendarDayDto>()
            };

            try
            {
                _calendarService.Create(createCalDto);
            }
            catch
            {
                // swallow or log depending on policy; do not block engineer creation
            }
        }

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

        // if calendar payload provided, create or update calendar accordingly
        if (dto.EngineerCalendar is not null)
        {
            try
            {
                var existingCal = _calendarService.GetByEngineerId(existing.Id);
                var calDto = new CreateUpdateEngineerCalendarDto
                {
                    EngineerId = existing.Id,
                    Days = dto.EngineerCalendar.Days?.Select(d => new CreateUpdateEngineerCalendarDayDto
                    {
                        Date = d.Date.Date,
                        Type = d.Type
                    }) ?? Array.Empty<CreateUpdateEngineerCalendarDayDto>()
                };

                if (existingCal is null)
                {
                    _calendarService.Create(calDto);
                }
                else
                {
                    _calendarService.Update(existingCal.EngineerCalendarId, calDto);
                }
            }
            catch
            {
                // swallow or log depending on policy
            }
        }

        return true;
    }

    public bool Delete(int id)
    {
        var existing = _repo.GetById(id);
        if (existing is null) return false;
        _repo.Delete(id);
        return true;
    }

    private EngineerDto MapToDto(Engineer e)
    {
        var dto = new EngineerDto
        {
            EngineerId = e.Id,
            Name = e.Name,
            Role = e.Role,
            DailyCapacity = e.DailyCapacity,
            TeamId = e.TeamId
        };

        // attach calendar if exists
        var cal = _calendarService.GetByEngineerId(e.Id);
        if (cal != null) dto.EngineerCalendar = cal;

        return dto;
    }
}
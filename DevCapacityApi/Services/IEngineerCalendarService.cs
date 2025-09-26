using System;
using System.Collections.Generic;
using DevCapacityApi.DTOs;

namespace DevCapacityApi.Services;

public interface IEngineerCalendarService
{
    EngineerCalendarDto Create(CreateUpdateEngineerCalendarDto dto);
    IEnumerable<EngineerCalendarDto> GetAll();
    EngineerCalendarDto? GetById(int id);
    EngineerCalendarDto? GetByEngineerId(int engineerId);
    bool Update(int id, CreateUpdateEngineerCalendarDto dto);
    bool Delete(int id);

    // helper
    bool IsVacation(int calendarId, DateTime date);
}
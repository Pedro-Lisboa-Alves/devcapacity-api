using System;
using System.Collections.Generic;
using DevCapacityApi.DTOs;

namespace DevCapacityApi.Services;

public interface ICompanyCalendarService
{
    CompanyCalendarDto Create(CreateUpdateCompanyCalendarDto dto);
    IEnumerable<CompanyCalendarDto> GetAll();
    CompanyCalendarDto? GetById(int id);
    bool Update(int id, CreateUpdateCompanyCalendarDto dto);
    bool Delete(int id);

    // método utilitário exposto pelo serviço
    bool IsCompanyWorkingDay(int calendarId, DateTime date);
}
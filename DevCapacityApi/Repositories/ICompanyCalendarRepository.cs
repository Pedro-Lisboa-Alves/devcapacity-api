using System.Collections.Generic;
using DevCapacityApi.Models;

namespace DevCapacityApi.Repositories;

public interface ICompanyCalendarRepository
{
    IEnumerable<CompanyCalendar> GetAll();
    CompanyCalendar? GetById(int id);
    CompanyCalendar Add(CompanyCalendar c);
    bool Update(CompanyCalendar c);
    bool Delete(int id);
}
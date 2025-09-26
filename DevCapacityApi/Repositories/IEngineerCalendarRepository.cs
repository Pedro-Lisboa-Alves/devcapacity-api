using System.Collections.Generic;
using DevCapacityApi.Models;

namespace DevCapacityApi.Repositories;

public interface IEngineerCalendarRepository
{
    IEnumerable<EngineerCalendar> GetAll();
    EngineerCalendar? GetById(int id);
    EngineerCalendar? GetByEngineerId(int engineerId);
    EngineerCalendar Add(EngineerCalendar c);
    bool Update(EngineerCalendar c);
    bool Delete(int id);
}
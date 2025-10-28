using System.Collections.Generic;
using DevCapacityApi.Models;

namespace DevCapacityApi.Repositories;

public interface IEngineerCalendarRepository
{
    EngineerCalendar? GetByEngineerId(int engineerId);
    EngineerCalendar? GetById(int id);
    IEnumerable<EngineerCalendar> GetAll();
    EngineerCalendar Add(EngineerCalendar c);
    bool Update(EngineerCalendar c);
    bool Delete(int id);

    // new: update a single calendar day (do not remove other days)
    bool UpdateDay(EngineerCalendarDay day);
}
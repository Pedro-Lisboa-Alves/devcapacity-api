using System.Collections.Generic;
using System.Linq;
using DevCapacityApi.Data;
using DevCapacityApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DevCapacityApi.Repositories;

public class EngineerCalendarRepository : IEngineerCalendarRepository
{
    private readonly AppDbContext _db;
    public EngineerCalendarRepository(AppDbContext db) => _db = db;

    public IEnumerable<EngineerCalendar> GetAll() =>
        _db.Set<EngineerCalendar>().Include(c => c.CalendarDays).AsNoTracking().ToList();

    public EngineerCalendar? GetById(int id) =>
        _db.Set<EngineerCalendar>().Include(c => c.CalendarDays).FirstOrDefault(c => c.EngineerCalendarId == id);

    public EngineerCalendar? GetByEngineerId(int engineerId) =>
        _db.Set<EngineerCalendar>().Include(c => c.CalendarDays).FirstOrDefault(c => c.EngineerId == engineerId);

    public EngineerCalendar Add(EngineerCalendar c)
    {
        _db.Set<EngineerCalendar>().Add(c);
        _db.SaveChanges();
        return c;
    }

    public bool Update(EngineerCalendar c)
    {
        var existing = _db.Set<EngineerCalendar>().Include(x => x.CalendarDays).FirstOrDefault(x => x.EngineerCalendarId == c.EngineerCalendarId);
        if (existing == null) return false;

        // replace vacation/day entries
        _db.Set<EngineerCalendarDay>().RemoveRange(existing.CalendarDays);
        _db.SaveChanges();

        // attach new days
        existing.CalendarDays = c.CalendarDays;
        _db.Update(existing);
        _db.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var existing = _db.Set<EngineerCalendar>().Find(id);
        if (existing == null) return false;
        _db.Set<EngineerCalendar>().Remove(existing);
        _db.SaveChanges();
        return true;
    }
}
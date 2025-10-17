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
        _db.Set<EngineerCalendar>().Include(c => c.Vacations).AsNoTracking().ToList();

    public EngineerCalendar? GetById(int id) =>
        _db.Set<EngineerCalendar>().Include(c => c.Vacations).FirstOrDefault(c => c.EngineerCalendarId == id);

    public EngineerCalendar? GetByEngineerId(int engineerId) =>
        _db.Set<EngineerCalendar>().Include(c => c.Vacations).FirstOrDefault(c => c.EngineerId == engineerId);

    public EngineerCalendar Add(EngineerCalendar c)
    {
        _db.Set<EngineerCalendar>().Add(c);
        _db.SaveChanges();
        return c;
    }

    public bool Update(EngineerCalendar c)
    {
        var existing = _db.Set<EngineerCalendar>().Include(x => x.Vacations).FirstOrDefault(x => x.EngineerCalendarId == c.EngineerCalendarId);
        if (existing == null) return false;

        existing.EngineerId = c.EngineerId;

        // replace vacations: remove old then add new
        _db.Set<EngineerCalendarDay>().RemoveRange(existing.Vacations);
        existing.Vacations = c.Vacations;
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
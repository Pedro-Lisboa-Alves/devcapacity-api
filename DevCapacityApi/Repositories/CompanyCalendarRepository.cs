using System.Collections.Generic;
using System.Linq;
using DevCapacityApi.Data;
using DevCapacityApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DevCapacityApi.Repositories;

public class CompanyCalendarRepository : ICompanyCalendarRepository
{
    private readonly AppDbContext _db;
    public CompanyCalendarRepository(AppDbContext db) => _db = db;

    public IEnumerable<CompanyCalendar> GetAll() =>
        _db.Set<CompanyCalendar>()
           .Include(c => c.NonWorkingDays)
           .AsNoTracking()
           .ToList();

    public CompanyCalendar? GetById(int id) =>
        _db.Set<CompanyCalendar>()
           .Include(c => c.NonWorkingDays)
           .FirstOrDefault(c => c.CompanyCalendarId == id);

    public CompanyCalendar Add(CompanyCalendar c)
    {
        _db.Set<CompanyCalendar>().Add(c);
        _db.SaveChanges();
        return c;
    }

    public bool Update(CompanyCalendar c)
    {
        var existing = _db.Set<CompanyCalendar>().Include(x => x.NonWorkingDays).FirstOrDefault(x => x.CompanyCalendarId == c.CompanyCalendarId);
        if (existing == null) return false;

        // replace NonWorkingDays collection
        _db.Set<CompanyCalendarNonWorkingDay>().RemoveRange(existing.NonWorkingDays);

        existing.NonWorkingDays = c.NonWorkingDays;
        _db.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var existing = _db.Set<CompanyCalendar>().Find(id);
        if (existing == null) return false;
        _db.Set<CompanyCalendar>().Remove(existing);
        _db.SaveChanges();
        return true;
    }
}
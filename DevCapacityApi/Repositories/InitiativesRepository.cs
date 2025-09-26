using System.Collections.Generic;
using System.Linq;
using DevCapacityApi.Data;
using DevCapacityApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DevCapacityApi.Repositories;

public class InitiativesRepository : IInitiativesRepository
{
    private readonly AppDbContext _db;
    public InitiativesRepository(AppDbContext db) => _db = db;

    public IEnumerable<Initiatives> GetAll() =>
        _db.Set<Initiatives>().Include(i => i.Tasks).AsNoTracking().ToList();

    public Initiatives? GetById(int id) =>
        _db.Set<Initiatives>().Include(i => i.Tasks).FirstOrDefault(i => i.InitiativeId == id);

    public Initiatives? GetByName(string name) =>
        _db.Set<Initiatives>().FirstOrDefault(i => i.Name == name);

    public Initiatives Add(Initiatives initiative)
    {
        _db.Set<Initiatives>().Add(initiative);
        _db.SaveChanges();
        return initiative;
    }

    public bool Update(Initiatives initiative)
    {
        var existing = _db.Set<Initiatives>().Find(initiative.InitiativeId);
        if (existing == null) return false;
        existing.Name = initiative.Name;
        existing.ParentInitiative = initiative.ParentInitiative;
        existing.Status = initiative.Status;
        existing.PDs = initiative.PDs;
        existing.StartDate = initiative.StartDate;
        existing.EndDate = initiative.EndDate;
        _db.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var existing = _db.Set<Initiatives>().Find(id);
        if (existing == null) return false;
        _db.Set<Initiatives>().Remove(existing);
        _db.SaveChanges();
        return true;
    }
}
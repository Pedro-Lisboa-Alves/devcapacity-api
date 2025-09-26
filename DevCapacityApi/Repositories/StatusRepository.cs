using System.Collections.Generic;
using System.Linq;
using DevCapacityApi.Data;
using DevCapacityApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DevCapacityApi.Repositories;

public class StatusRepository : IStatusRepository
{
    private readonly AppDbContext _db;
    public StatusRepository(AppDbContext db) => _db = db;

    public IEnumerable<Status> GetAll() =>
        _db.Set<Status>().AsNoTracking().ToList();

    public Status? GetById(int id) =>
        _db.Set<Status>().Find(id);

    public Status? GetByName(string name) =>
        _db.Set<Status>().FirstOrDefault(s => s.Name == name);

    public Status Add(Status status)
    {
        _db.Set<Status>().Add(status);
        _db.SaveChanges();
        return status;
    }

    public bool Update(Status status)
    {
        var existing = _db.Set<Status>().Find(status.StatusId);
        if (existing == null) return false;
        existing.Name = status.Name;
        _db.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var existing = _db.Set<Status>().Find(id);
        if (existing == null) return false;
        _db.Set<Status>().Remove(existing);
        _db.SaveChanges();
        return true;
    }
}
using System.Collections.Generic;
using System.Linq;
using DevCapacityApi.Data;
using DevCapacityApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DevCapacityApi.Repositories;

public class TasksRepository : ITasksRepository
{
    private readonly AppDbContext _db;
    public TasksRepository(AppDbContext db) => _db = db;

    public IEnumerable<Tasks> GetAll() =>
        _db.Set<Tasks>().Include(t => t.Assignments).AsNoTracking().ToList();

    public Tasks? GetById(int id) =>
        _db.Set<Tasks>().Include(t => t.Assignments).FirstOrDefault(t => t.TaskId == id);

    public Tasks Add(Tasks t)
    {
        _db.Set<Tasks>().Add(t);
        _db.SaveChanges();
        return t;
    }

    public bool Update(Tasks t)
    {
        var existing = _db.Set<Tasks>().Find(t.TaskId);
        if (existing == null) return false;

        existing.Name = t.Name;
        existing.Initiative = t.Initiative;
        existing.Status = t.Status;
        existing.PDs = t.PDs;
        existing.MaxResources = t.MaxResources;
        existing.StartDate = t.StartDate;
        existing.EndDate = t.EndDate;

        _db.Update(existing);
        _db.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var e = _db.Set<Tasks>().Find(id);
        if (e == null) return false;
        _db.Set<Tasks>().Remove(e);
        _db.SaveChanges();
        return true;
    }

    public Tasks? GetByName(string name) =>
        _db.Set<Tasks>().FirstOrDefault(t => t.Name == name);
}
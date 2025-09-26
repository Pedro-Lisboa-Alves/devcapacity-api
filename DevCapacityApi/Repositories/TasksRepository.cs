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

    public Tasks? GetByName(string name) =>
        _db.Set<Tasks>().FirstOrDefault(t => t.Name == name);

    public Tasks Add(Tasks task)
    {
        _db.Set<Tasks>().Add(task);
        _db.SaveChanges();
        return task;
    }

    public bool Update(Tasks task)
    {
        var existing = _db.Set<Tasks>().Find(task.TaskId);
        if (existing == null) return false;
        existing.Name = task.Name;
        existing.Initiative = task.Initiative;
        existing.Status = task.Status;
        existing.PDs = task.PDs;
        existing.StartDate = task.StartDate;
        existing.EndDate = task.EndDate;
        _db.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var existing = _db.Set<Tasks>().Find(id);
        if (existing == null) return false;
        _db.Set<Tasks>().Remove(existing);
        _db.SaveChanges();
        return true;
    }
}
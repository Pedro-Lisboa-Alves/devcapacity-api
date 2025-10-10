using System.Collections.Generic;
using System.Linq;
using DevCapacityApi.Data;
using DevCapacityApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DevCapacityApi.Repositories;

public class EngineerAssignmentRepository : IEngineerAssignmentRepository
{
    private readonly AppDbContext _db;
    public EngineerAssignmentRepository(AppDbContext db) => _db = db;

    public IEnumerable<EngineerAssignment> GetAll() =>
        _db.Set<EngineerAssignment>().Include(a => a.Engineer).AsNoTracking().ToList();

    public EngineerAssignment? GetById(int id) =>
        _db.Set<EngineerAssignment>().Include(a => a.Engineer).FirstOrDefault(a => a.AssignmentId == id);

    public IEnumerable<EngineerAssignment> GetByEngineerId(int engineerId) =>
        _db.Set<EngineerAssignment>().Where(a => a.EngineerId == engineerId).AsNoTracking().ToList();

    public IEnumerable<EngineerAssignment> GetByTaskId(int taskId) =>
        _db.Set<EngineerAssignment>().Where(a => a.TaskId == taskId).AsNoTracking().ToList();

    public EngineerAssignment Add(EngineerAssignment a)
    {
        _db.Set<EngineerAssignment>().Add(a);
        _db.SaveChanges();
        return a;
    }

    public bool Update(EngineerAssignment a)
    {
        var existing = _db.Set<EngineerAssignment>().Find(a.AssignmentId);
        if (existing == null) return false;

        existing.EngineerId = a.EngineerId;
        existing.TaskId = a.TaskId;
        existing.CapacityShare = a.CapacityShare;
        existing.StartDate = a.StartDate;
        existing.EndDate = a.EndDate;

        _db.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var existing = _db.Set<EngineerAssignment>().Find(id);
        if (existing == null) return false;
        _db.Set<EngineerAssignment>().Remove(existing);
        _db.SaveChanges();
        return true;
    }
}
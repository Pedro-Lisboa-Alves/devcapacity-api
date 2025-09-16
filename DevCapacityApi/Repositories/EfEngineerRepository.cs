using DevCapacityApi.Models;
using DevCapacityApi.Data;
using Microsoft.EntityFrameworkCore;

namespace DevCapacityApi.Repositories;

public class EfEngineerRepository : IEngineerRepository
{
    private readonly AppDbContext _ctx;

    public EfEngineerRepository(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public IEnumerable<Engineer> GetAll()
    {
        return _ctx.Engineers.AsNoTracking().ToList();
    }

    public Engineer? GetById(int id)
    {
        return _ctx.Engineers.AsNoTracking().FirstOrDefault(e => e.Id == id);
    }

    public Engineer? GetByName(string name) // <-- implementação
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var lowered = name.Trim().ToLower();
        return _ctx.Engineers.AsNoTracking()
                   .FirstOrDefault(e => e.Name != null && e.Name.ToLower() == lowered);
    }

    public Engineer Add(Engineer engineer)
    {
        var entry = _ctx.Engineers.Add(engineer);
        _ctx.SaveChanges();
        return entry.Entity;
    }

    public void Update(Engineer engineer)
    {
        _ctx.Engineers.Update(engineer);
        _ctx.SaveChanges();
    }

    public void Delete(int id)
    {
        var e = _ctx.Engineers.Find(id);
        if (e is null) return;
        _ctx.Engineers.Remove(e);
        _ctx.SaveChanges();
    }
}
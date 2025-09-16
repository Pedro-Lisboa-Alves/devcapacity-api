using System.Collections.Generic;
using System.Linq;
using DevCapacityApi.Data;
using DevCapacityApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DevCapacityApi.Repositories;

public class TeamRepository : ITeamRepository
{
    private readonly AppDbContext _db;
    public TeamRepository(AppDbContext db) => _db = db;

    public IEnumerable<Team> GetAll() =>
        _db.Teams.Include(t => t.Children).Include(t => t.Engineers).AsNoTracking().ToList();

    public Team? GetById(int id) =>
        _db.Teams.Include(t => t.Children).Include(t => t.Engineers).FirstOrDefault(t => t.TeamId == id);

    public Team? GetByName(string name) =>
        _db.Teams.FirstOrDefault(t => t.Name == name);

    public Team Add(Team team)
    {
        _db.Teams.Add(team);
        _db.SaveChanges();
        return team;
    }

    public bool Update(Team team)
    {
        var existing = _db.Teams.Find(team.TeamId);
        if (existing == null) return false;
        existing.Name = team.Name;
        existing.ParentTeam = team.ParentTeam;
        _db.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var t = _db.Teams.Find(id);
        if (t == null) return false;
        _db.Teams.Remove(t);
        _db.SaveChanges();
        return true;
    }
}
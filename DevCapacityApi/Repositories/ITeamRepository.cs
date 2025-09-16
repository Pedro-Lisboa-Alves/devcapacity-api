using System.Collections.Generic;
using DevCapacityApi.Models;

namespace DevCapacityApi.Repositories;

public interface ITeamRepository
{
    IEnumerable<Team> GetAll();
    Team? GetById(int id);
    Team? GetByName(string name);
    Team Add(Team team);
    bool Update(Team team);
    bool Delete(int id);
}
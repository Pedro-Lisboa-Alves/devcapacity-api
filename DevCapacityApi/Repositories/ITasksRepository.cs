using System.Collections.Generic;
using DevCapacityApi.Models;

namespace DevCapacityApi.Repositories;

public interface ITasksRepository
{
    IEnumerable<Tasks> GetAll();
    Tasks? GetById(int id);
    Tasks? GetByName(string name);
    Tasks Add(Tasks task);
    bool Update(Tasks task);
    bool Delete(int id);
}
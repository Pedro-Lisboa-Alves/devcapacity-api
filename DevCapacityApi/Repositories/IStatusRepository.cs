using System.Collections.Generic;
using DevCapacityApi.Models;

namespace DevCapacityApi.Repositories;

public interface IStatusRepository
{
    IEnumerable<Status> GetAll();
    Status? GetById(int id);
    Status? GetByName(string name);
    Status Add(Status status);
    bool Update(Status status);
    bool Delete(int id);
}
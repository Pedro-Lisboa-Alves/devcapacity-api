using System.Collections.Generic;
using DevCapacityApi.Models;

namespace DevCapacityApi.Repositories;

public interface IInitiativesRepository
{
    IEnumerable<Initiatives> GetAll();
    Initiatives? GetById(int id);
    Initiatives? GetByName(string name);
    Initiatives Add(Initiatives initiative);
    bool Update(Initiatives initiative);
    bool Delete(int id);
}
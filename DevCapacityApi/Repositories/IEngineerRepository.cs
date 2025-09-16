using DevCapacityApi.Models;

namespace DevCapacityApi.Repositories;

public interface IEngineerRepository
{
    IEnumerable<Engineer> GetAll();
    Engineer? GetById(int id);
    Engineer? GetByName(string name); // <-- nova linha
    Engineer Add(Engineer engineer);
    void Update(Engineer engineer);
    void Delete(int id);
}
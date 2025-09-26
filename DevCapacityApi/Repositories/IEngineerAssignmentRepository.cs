using System.Collections.Generic;
using DevCapacityApi.Models;

namespace DevCapacityApi.Repositories;

public interface IEngineerAssignmentRepository
{
    IEnumerable<EngineerAssignment> GetAll();
    EngineerAssignment? GetById(int id);
    IEnumerable<EngineerAssignment> GetByEngineerId(int engineerId);
    EngineerAssignment Add(EngineerAssignment a);
    bool Update(EngineerAssignment a);
    bool Delete(int id);
}
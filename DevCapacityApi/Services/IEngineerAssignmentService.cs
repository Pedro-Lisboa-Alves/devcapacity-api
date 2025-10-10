using System.Collections.Generic;
using DevCapacityApi.DTOs;

namespace DevCapacityApi.Services;

public interface IEngineerAssignmentService
{
    EngineerAssignmentDto Create(CreateUpdateEngineerAssignmentDto dto);
    IEnumerable<EngineerAssignmentDto> GetAll();
    EngineerAssignmentDto? GetById(int id);
    IEnumerable<EngineerAssignmentDto> GetByEngineerId(int engineerId);
    bool Update(int id, CreateUpdateEngineerAssignmentDto dto);
    bool Delete(int id);
    IEnumerable<EngineerAssignmentDto> GetByTaskId(int taskId);
}
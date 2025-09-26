using System.Collections.Generic;
using DevCapacityApi.DTOs;

namespace DevCapacityApi.Services;

public interface ITasksService
{
    TasksDto Create(CreateUpdateTasksDto dto);
    IEnumerable<TasksDto> GetAll();
    TasksDto? GetById(int id);
    bool Update(int id, CreateUpdateTasksDto dto);
    bool Delete(int id);
}
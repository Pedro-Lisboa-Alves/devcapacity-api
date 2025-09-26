using System.Collections.Generic;
using DevCapacityApi.DTOs;

namespace DevCapacityApi.Services;

public interface IStatusService
{
    StatusDto Create(CreateUpdateStatusDto dto);
    IEnumerable<StatusDto> GetAll();
    StatusDto? GetById(int id);
    bool Update(int id, CreateUpdateStatusDto dto);
    bool Delete(int id);
}
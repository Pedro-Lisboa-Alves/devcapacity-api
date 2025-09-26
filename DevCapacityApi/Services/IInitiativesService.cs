using System.Collections.Generic;
using DevCapacityApi.DTOs;

namespace DevCapacityApi.Services;

public interface IInitiativesService
{
    InitiativesDto Create(CreateUpdateInitiativesDto dto);
    IEnumerable<InitiativesDto> GetAll();
    InitiativesDto? GetById(int id);
    bool Update(int id, CreateUpdateInitiativesDto dto);
    bool Delete(int id);
}
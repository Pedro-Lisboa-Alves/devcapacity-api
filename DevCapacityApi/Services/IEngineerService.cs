using DevCapacityApi.DTOs;

namespace DevCapacityApi.Services;

public interface IEngineerService
{
    IEnumerable<EngineerDto> GetAll();
    EngineerDto? GetById(int id);
    EngineerDto Create(EngineerDto dto);
    bool Update(int id, EngineerDto dto);
    bool Delete(int id);
}
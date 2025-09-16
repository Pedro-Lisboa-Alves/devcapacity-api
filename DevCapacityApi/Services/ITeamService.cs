using System.Collections.Generic;
using DevCapacityApi.DTOs;

namespace DevCapacityApi.Services;

public interface ITeamService
{
    TeamDto Create(CreateUpdateTeamDto dto);
    IEnumerable<TeamDto> GetAll();
    TeamDto? GetById(int id);
    bool Update(int id, CreateUpdateTeamDto dto);
    bool Delete(int id);
}
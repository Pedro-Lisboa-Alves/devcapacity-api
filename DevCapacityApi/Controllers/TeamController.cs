using Microsoft.AspNetCore.Mvc;
using DevCapacityApi.DTOs;
using DevCapacityApi.Services;

namespace DevCapacityApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamController : ControllerBase
{
    private readonly ITeamService _svc;
    public TeamController(ITeamService svc) => _svc = svc;

    [HttpGet]
    public IActionResult GetAll() => Ok(_svc.GetAll());

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var t = _svc.GetById(id);
        if (t == null) return NotFound();
        return Ok(t);
    }

    [HttpPost]
    public IActionResult Create(CreateUpdateTeamDto dto)
    {
        var created = _svc.Create(dto);
        return CreatedAtAction(nameof(Get), new { id = created.TeamId }, created);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, CreateUpdateTeamDto dto)
    {
        var ok = _svc.Update(id, dto);
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var ok = _svc.Delete(id);
        if (!ok) return NotFound();
        return NoContent();
    }
}
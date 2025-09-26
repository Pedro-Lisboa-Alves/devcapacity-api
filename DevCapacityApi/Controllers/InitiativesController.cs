using Microsoft.AspNetCore.Mvc;
using DevCapacityApi.DTOs;
using DevCapacityApi.Services;

namespace DevCapacityApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InitiativesController : ControllerBase
{
    private readonly IInitiativesService _svc;
    public InitiativesController(IInitiativesService svc) => _svc = svc;

    [HttpGet]
    public IActionResult GetAll() => Ok(_svc.GetAll());

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var i = _svc.GetById(id);
        if (i == null) return NotFound();
        return Ok(i);
    }

    [HttpPost]
    public IActionResult Create(CreateUpdateInitiativesDto dto)
    {
        var created = _svc.Create(dto);
        return CreatedAtAction(nameof(Get), new { id = created.InitiativeId }, created);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, CreateUpdateInitiativesDto dto)
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
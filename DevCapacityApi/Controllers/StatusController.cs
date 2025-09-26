using Microsoft.AspNetCore.Mvc;
using DevCapacityApi.DTOs;
using DevCapacityApi.Services;

namespace DevCapacityApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatusController : ControllerBase
{
    private readonly IStatusService _svc;
    public StatusController(IStatusService svc) => _svc = svc;

    [HttpGet]
    public IActionResult GetAll() => Ok(_svc.GetAll());

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var s = _svc.GetById(id);
        if (s == null) return NotFound();
        return Ok(s);
    }

    [HttpPost]
    public IActionResult Create(CreateUpdateStatusDto dto)
    {
        var created = _svc.Create(dto);
        return CreatedAtAction(nameof(Get), new { id = created.StatusId }, created);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, CreateUpdateStatusDto dto)
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
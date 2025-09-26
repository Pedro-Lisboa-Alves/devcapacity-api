using Microsoft.AspNetCore.Mvc;
using DevCapacityApi.DTOs;
using DevCapacityApi.Services;

namespace DevCapacityApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EngineerAssignmentController : ControllerBase
{
    private readonly IEngineerAssignmentService _svc;
    public EngineerAssignmentController(IEngineerAssignmentService svc) => _svc = svc;

    [HttpGet]
    public IActionResult GetAll() => Ok(_svc.GetAll());

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var a = _svc.GetById(id);
        if (a == null) return NotFound();
        return Ok(a);
    }

    [HttpGet("engineer/{engineerId:int}")]
    public IActionResult GetByEngineer(int engineerId) => Ok(_svc.GetByEngineerId(engineerId));

    [HttpPost]
    public IActionResult Create(CreateUpdateEngineerAssignmentDto dto)
    {
        var created = _svc.Create(dto);
        return CreatedAtAction(nameof(Get), new { id = created.AssignmentId }, created);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, CreateUpdateEngineerAssignmentDto dto)
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
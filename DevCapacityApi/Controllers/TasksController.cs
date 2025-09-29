using Microsoft.AspNetCore.Mvc;
using DevCapacityApi.DTOs;
using DevCapacityApi.Services;

namespace DevCapacityApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITasksService _svc;
    public TasksController(ITasksService svc) => _svc = svc;

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
    public IActionResult Create(CreateUpdateTasksDto dto)
    {
        var created = _svc.Create(dto);
        return CreatedAtAction(nameof(Get), new { id = created.TaskId }, created);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, CreateUpdateTasksDto dto)
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
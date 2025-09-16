using Microsoft.AspNetCore.Mvc;
using DevCapacityApi.Services;
using DevCapacityApi.DTOs;

namespace DevCapacityApi.Controllers;

[ApiController]
[Route("[controller]")]
public class EngineersController : ControllerBase
{
    private readonly IEngineerService _service;

    public EngineersController(IEngineerService service)
    {
        _service = service;
    }

    [HttpGet]
    public ActionResult<IEnumerable<EngineerDto>> Get() => Ok(_service.GetAll());

    [HttpGet("{id:int}")]
    public ActionResult<EngineerDto> Get(int id)
    {
        var e = _service.GetById(id);
        return e is null ? NotFound() : Ok(e);
    }

    [HttpPost]
    public ActionResult<EngineerDto> Post([FromBody] EngineerDto dto)
    {
        try
        {
            var created = _service.Create(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public IActionResult Put(int id, [FromBody] EngineerDto dto)
    {
        try
        {
            if (!_service.Update(id, dto)) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        if (!_service.Delete(id)) return NotFound();
        return NoContent();
    }
}
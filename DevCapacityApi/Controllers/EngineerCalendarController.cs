using Microsoft.AspNetCore.Mvc;
using DevCapacityApi.DTOs;
using DevCapacityApi.Services;

namespace DevCapacityApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EngineerCalendarController : ControllerBase
{
    private readonly IEngineerCalendarService _svc;
    public EngineerCalendarController(IEngineerCalendarService svc) => _svc = svc;

    [HttpGet]
    public IActionResult GetAll() => Ok(_svc.GetAll());

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var c = _svc.GetById(id);
        if (c == null) return NotFound();
        return Ok(c);
    }

    [HttpGet("engineer/{engineerId:int}")]
    public IActionResult GetByEngineer(int engineerId)
    {
        var c = _svc.GetByEngineerId(engineerId);
        if (c == null) return NotFound();
        return Ok(c);
    }

    [HttpPost]
    public IActionResult Create(CreateUpdateEngineerCalendarDto dto)
    {
        var created = _svc.Create(dto);
        return CreatedAtAction(nameof(Get), new { id = created.EngineerCalendarId }, created);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, CreateUpdateEngineerCalendarDto dto)
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

    [HttpGet("{id:int}/isvacation")]
    public IActionResult IsVacation(int id, [FromQuery] DateTime date)
    {
        try
        {
            var result = _svc.IsVacation(id, date);
            return Ok(new { date = date.Date, isVacation = result });
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using DevCapacityApi.DTOs;
using DevCapacityApi.Services;
using DevCapacityApi.Models;

namespace DevCapacityApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EngineerCalendarController : ControllerBase
{
    private readonly IEngineerCalendarService _calendarService;
    public EngineerCalendarController(IEngineerCalendarService calendarService) => _calendarService = calendarService;

    [HttpGet("engineer/{engineerId:int}")]
    public IActionResult GetByEngineer(int engineerId)
    {
        var c = _calendarService.GetByEngineerId(engineerId);
        if (c == null) return NotFound();
        return Ok(c);
    }

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var c = _calendarService.GetById(id);
        if (c == null) return NotFound();
        return Ok(c);
    }

    [HttpPost]
    public IActionResult Create(CreateUpdateEngineerCalendarDto dto)
    {
        var created = _calendarService.Create(dto);
        return CreatedAtAction(nameof(Get), new { id = created.EngineerCalendarId }, created);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, CreateUpdateEngineerCalendarDto dto)
    {
        var ok = _calendarService.Update(id, dto);
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var ok = _calendarService.Delete(id);
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpGet("{id:int}/isvacation")]
    public IActionResult IsVacation(int id, [FromQuery] DateTime date)
    {
        try
        {
            var result = _calendarService.IsVacation(id, date);
            return Ok(new { date = date.Date, isVacation = result });
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}
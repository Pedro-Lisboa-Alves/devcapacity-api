using Microsoft.AspNetCore.Mvc;
using DevCapacityApi.DTOs;
using DevCapacityApi.Services;

namespace DevCapacityApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyCalendarController : ControllerBase
{
    private readonly ICompanyCalendarService _svc;
    public CompanyCalendarController(ICompanyCalendarService svc) => _svc = svc;

    [HttpGet]
    public IActionResult GetAll() => Ok(_svc.GetAll());

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var c = _svc.GetById(id);
        if (c == null) return NotFound();
        return Ok(c);
    }

    [HttpPost]
    public IActionResult Create(CreateUpdateCompanyCalendarDto dto)
    {
        var created = _svc.Create(dto);
        return CreatedAtAction(nameof(Get), new { id = created.CompanyCalendarId }, created);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, CreateUpdateCompanyCalendarDto dto)
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

    // check if given date is working day
    [HttpGet("{id:int}/isworking")]
    public IActionResult IsWorking(int id, [FromQuery] DateTime date)
    {
        try
        {
            var result = _svc.IsCompanyWorkingDay(id, date);
            return Ok(new { date = date.Date, isWorking = result });
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevCapacityApi.DTOs;
using DevCapacityApi.Models;
using DevCapacityApi.Repositories;
using DevCapacityApi.Messaging;
using Avro.Generic;
using Confluent.SchemaRegistry.Serdes;

namespace DevCapacityApi.Services;

public class EngineerAssignmentService : IEngineerAssignmentService
{
    private readonly IEngineerAssignmentRepository _repo;
    private readonly IEngineerRepository _engineerRepo;
    private readonly IKafkaAssignmentProducer _producer;
    private readonly IEngineerCalendarRepository _engineerCalendarRepo;
    private readonly ICompanyCalendarRepository _companyCalendarRepo;

    public EngineerAssignmentService(
        IEngineerAssignmentRepository repo,
        IEngineerRepository engineerRepo,
        IKafkaAssignmentProducer producer,
        IEngineerCalendarRepository engineerCalendarRepo,
        ICompanyCalendarRepository companyCalendarRepo)
    {
        _repo = repo;
        _engineerRepo = engineerRepo;
        _producer = producer;
        _engineerCalendarRepo = engineerCalendarRepo;
        _companyCalendarRepo = companyCalendarRepo;
    }

    public EngineerAssignmentDto Create(CreateUpdateEngineerAssignmentDto dto)
    {
        // validate engineer exists
        var eng = _engineerRepo.GetById(dto.EngineerId);
        if (eng is null) throw new InvalidOperationException("Engineer not found.");

        // ensure engineer calendar days exist for next 3 years
        try
        {
            EnsureEngineerCalendarDays(dto.EngineerId);
        }
        catch (Exception ex)
        {
            // non-fatal: log/ignore or rethrow depending on policy
            // here we swallow to not block assignment creation; in production prefer logging
        }

        var entity = new EngineerAssignment
        {
            EngineerId = dto.EngineerId,
            TaskId = dto.TaskId,
            CapacityShare = dto.CapacityShare,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };

        var created = _repo.Add(entity);

        // enviar evento Avro para Kafka (fire-and-forget via Task)
        _ = _producer.ProduceAssignmentAsync(created, "created");

        return MapToDto(created);
    }

    public IEnumerable<EngineerAssignmentDto> GetAll() => _repo.GetAll().Select(MapToDto);

    public EngineerAssignmentDto? GetById(int id)
    {
        var a = _repo.GetById(id);
        return a == null ? null : MapToDto(a);
    }

    public IEnumerable<EngineerAssignmentDto> GetByEngineerId(int engineerId) =>
        _repo.GetByEngineerId(engineerId).Select(MapToDto);

    // novo: obter por TaskId
    public IEnumerable<EngineerAssignmentDto> GetByTaskId(int taskId) =>
        _repo.GetByTaskId(taskId).Select(MapToDto);

    public bool Update(int id, CreateUpdateEngineerAssignmentDto dto)
    {
        var existing = _repo.GetById(id);
        if (existing == null) return false;

        // validate engineer exists
        var eng = _engineerRepo.GetById(dto.EngineerId);
        if (eng is null) throw new InvalidOperationException("Engineer not found.");

        // ensure calendar days exist when assignment's engineer may change / be updated
        try
        {
            EnsureEngineerCalendarDays(dto.EngineerId);
        }
        catch
        {
            // swallow as above
        }

        existing.EngineerId = dto.EngineerId;
        existing.TaskId = dto.TaskId;
        existing.CapacityShare = dto.CapacityShare;
        existing.StartDate = dto.StartDate;
        existing.EndDate = dto.EndDate;

        return _repo.Update(existing);
    }

    public bool Delete(int id)
    {
        // load entity to include data in event
        var existing = _repo.GetById(id);
        if (existing == null) return false;

        var ok = _repo.Delete(id);
        if (ok)
        {
            _ = _producer.ProduceAssignmentAsync(existing, "deleted");
        }
        return ok;
    }

    private static EngineerAssignmentDto MapToDto(EngineerAssignment a) =>
        new EngineerAssignmentDto
        {
            AssignmentId = a.AssignmentId,
            EngineerId = a.EngineerId,
            TaskId = a.TaskId,
            CapacityShare = a.CapacityShare,
            StartDate = a.StartDate,
            EndDate = a.EndDate
        };

    // --- new helper to generate calendar days for an engineer for 3 years ---
    private void EnsureEngineerCalendarDays(int engineerId)
    {
        // get or create calendar for engineer
        var calendar = _engineerCalendarRepo.GetByEngineerId(engineerId);
        var now = DateTime.Today;
        var end = now.AddYears(3);

        if (calendar == null)
        {
            calendar = new EngineerCalendar
            {
                EngineerId = engineerId,
                CalendarDays = new List<EngineerCalendarDay>()
            };
            calendar = _engineerCalendarRepo.Add(calendar);
        }
        else
        {
            // ensure collection is loaded
            calendar.CalendarDays ??= new List<EngineerCalendarDay>();
        }

        // build set of existing dates for quick lookup
        var existingDates = new HashSet<DateTime>(calendar.CalendarDays.Select(d => d.Date.Date));

        // get company calendar (if any). pick first available or null.
        CompanyCalendar? companyCal = null;
        try
        {
            companyCal = _companyCalendarRepo.GetAll().FirstOrDefault();
        }
        catch
        {
            companyCal = null;
        }

        var toAdd = new List<EngineerCalendarDay>();

        for (var date = now; date < end; date = date.AddDays(1))
        {
            if (existingDates.Contains(date)) continue;

            // determine type: Available if company says working day; otherwise Vacations
            var type = EngineerCalendarDayType.Weekends;
            try
            {
                if (companyCal != null)
                {
                    type = companyCal.IsCompanyWorkingDay(date) ? EngineerCalendarDayType.Available : EngineerCalendarDayType.Weekends;
                }
                else
                {
                    // fallback: Weekdays Available, weekends Vacations
                    type = (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                        ? EngineerCalendarDayType.Weekends
                        : EngineerCalendarDayType.Available;
                }
            }
            catch
            {
                type = (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                        ? EngineerCalendarDayType.Weekends
                        : EngineerCalendarDayType.Available;
            }

            toAdd.Add(new EngineerCalendarDay
            {
                EngineerCalendarId = calendar.EngineerCalendarId,
                Date = date,
                Type = type
            });
        }

        if (toAdd.Count > 0)
        {
            // append and persist
            foreach (var d in toAdd) calendar.CalendarDays.Add(d);
            _engineerCalendarRepo.Update(calendar);
        }
    }
}
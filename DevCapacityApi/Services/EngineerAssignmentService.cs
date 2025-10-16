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

    public EngineerAssignmentService(IEngineerAssignmentRepository repo, IEngineerRepository engineerRepo, IKafkaAssignmentProducer producer)
    {
        _repo = repo;
        _engineerRepo = engineerRepo;
        _producer = producer;
    }

    public EngineerAssignmentDto Create(CreateUpdateEngineerAssignmentDto dto)
    {
        // validate engineer exists
        var eng = _engineerRepo.GetById(dto.EngineerId);
        if (eng is null) throw new InvalidOperationException("Engineer not found.");

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
}
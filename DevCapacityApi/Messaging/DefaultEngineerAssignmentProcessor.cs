using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DevCapacityApi.Models;

namespace DevCapacityApi.Messaging;

public class DefaultEngineerAssignmentProcessor : IEngineerAssignmentProcessor
{
    private readonly ILogger<DefaultEngineerAssignmentProcessor> _logger;

    public DefaultEngineerAssignmentProcessor(ILogger<DefaultEngineerAssignmentProcessor> logger)
    {
        _logger = logger;
    }

    public Task ProcessAssignmentAsync(EngineerAssignment assignment, string operation, CancellationToken cancellationToken = default)
    {
        // TODO: substituir por lógica real (resolver escopo e chamar repositório/serviço)
        _logger.LogInformation("Processing assignment event: Operation={Operation} AssignmentId={AssignmentId} EngineerId={EngineerId} TaskId={TaskId}",
            operation, assignment.AssignmentId, assignment.EngineerId, assignment.TaskId);

        // placeholder for future async processing
        return Task.CompletedTask;
    }
}
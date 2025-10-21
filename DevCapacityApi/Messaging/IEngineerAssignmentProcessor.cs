using System.Threading;
using System.Threading.Tasks;
using DevCapacityApi.Models;

namespace DevCapacityApi.Messaging;

public interface IEngineerAssignmentProcessor
{
    /// <summary>
    /// Process an incoming EngineerAssignment event (operation = created|deleted|etc).
    /// Implement business logic here.
    /// </summary>
    Task ProcessAssignmentAsync(EngineerAssignment assignment, string operation, CancellationToken cancellationToken = default);
}
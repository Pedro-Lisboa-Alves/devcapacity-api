using System;
using System.Threading;
using System.Threading.Tasks;
using Avro.Generic;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using DevCapacityApi.Models;
using Confluent.Kafka.SyncOverAsync;

namespace DevCapacityApi.Messaging;

public class KafkaAssignmentConsumer : BackgroundService
{
    private readonly ConsumerConfig _consumerConfig;
    private readonly SchemaRegistryConfig _schemaRegistryConfig;
    private readonly string _topic;
    private readonly ILogger<KafkaAssignmentConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    private Task? _consumerTask;

    public KafkaAssignmentConsumer(
        ConsumerConfig consumerConfig,
        SchemaRegistryConfig schemaRegistryConfig,
        IServiceScopeFactory scopeFactory,
        ILogger<KafkaAssignmentConsumer> logger,
        string? topic = null)
    {
        _consumerConfig = consumerConfig ?? throw new ArgumentNullException(nameof(consumerConfig));
        _schemaRegistryConfig = schemaRegistryConfig ?? throw new ArgumentNullException(nameof(schemaRegistryConfig));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _topic = topic ?? "engineer-assignment-events";
    }

    // NÃO bloquear o arranque do host
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Arranca o consumidor num OS-thread dedicado (não usa o ThreadPool do ASP.NET)
        _consumerTask = Task.Factory.StartNew(
            () => RunConsumer(stoppingToken),
            stoppingToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);

        return Task.CompletedTask; // devolve já → Kestrel/Swagger sobem
    }

    private void RunConsumer(CancellationToken stoppingToken)
    {
        try
        {
            using var schemaRegistry = new CachedSchemaRegistryClient(_schemaRegistryConfig);
            var avroDeserializer = new AvroDeserializer<GenericRecord>(schemaRegistry);

            using var consumer = new ConsumerBuilder<Null, GenericRecord>(_consumerConfig)
                .SetValueDeserializer(avroDeserializer.AsSyncOverAsync())
                .SetErrorHandler((_, e) => _logger.LogError("Kafka error: {Reason}", e.Reason))
                .SetLogHandler((_, m) => _logger.LogDebug("Kafka log: {Facility} {Message}", m.Facility, m.Message))
                .Build();

            consumer.Subscribe(_topic);
            _logger.LogInformation("Kafka consumer started on dedicated thread. Topic={Topic}", _topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<Null, GenericRecord>? cr = null;
                try
                {
                    // Respeita cancelamento → não fica “preso” no shutdown
                    cr = consumer.Consume(stoppingToken);
                    if (cr?.Message?.Value == null) continue;

                    var rec = cr.Message.Value;

                    var assignment = new EngineerAssignment
                    {
                        AssignmentId = Convert.ToInt32(rec["assignmentId"]),
                        EngineerId = Convert.ToInt32(rec["engineerId"]),
                        TaskId = Convert.ToInt32(rec["taskId"]),
                        CapacityShare = Convert.ToInt32(rec["capacityShare"]),
                        StartDate = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(rec["startDateMs"])).DateTime,
                        EndDate = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(rec["endDateMs"])).DateTime
                    };
                    var operation = rec["operation"]?.ToString() ?? string.Empty;

                    using var scope = _scopeFactory.CreateScope();
                    var processor = scope.ServiceProvider.GetRequiredService<IEngineerAssignmentProcessor>();
                    // Como estamos num thread dedicado, podemos bloquear aqui sem afetar o host
                    processor.ProcessAssignmentAsync(assignment, operation, stoppingToken).GetAwaiter().GetResult();

                    try { consumer.Commit(cr); } catch (Exception ex) { _logger.LogWarning(ex, "Commit failed"); }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                catch (ConsumeException cex)
                {
                    _logger.LogError(cex, "Consume exception: {Reason}", cex.Error?.Reason);
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message at offset {Offset}", cr?.Offset);
                }
            }

            try { consumer.Close(); } catch { }
            _logger.LogInformation("Kafka consumer stopped.");
        }
        catch (Exception ex)
        {
            // Qualquer falha aqui NÃO derruba o host/web
            _logger.LogCritical(ex, "Kafka consumer crashed during startup/run.");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_consumerTask != null)
        {
            try { await _consumerTask.ConfigureAwait(false); } catch { /* ignore */ }
        }
        await base.StopAsync(cancellationToken);
    }
}

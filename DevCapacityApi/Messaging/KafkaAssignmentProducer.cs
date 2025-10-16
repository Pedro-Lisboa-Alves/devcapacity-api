using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Avro;
using Avro.Generic;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using DevCapacityApi.Models;

namespace DevCapacityApi.Messaging;

public interface IKafkaAssignmentProducer
{
    Task ProduceAssignmentAsync(EngineerAssignment assignment, string operation);
}

public class KafkaAssignmentProducer : IKafkaAssignmentProducer, IDisposable
{
    private readonly IProducer<Null, GenericRecord> _producer;
    private readonly ISchemaRegistryClient _schemaRegistry;
    private readonly string _topic;
    private readonly Avro.Schema _avroSchema;

    // avro schema includes operation and timestamps as long (epoch ms)
    private const string SchemaString = @"{
      ""type"": ""record"",
      ""name"": ""EngineerAssignmentEvent"",
      ""namespace"": ""devcapacity"",
      ""fields"": [
        { ""name"": ""assignmentId"", ""type"": ""int"" },
        { ""name"": ""engineerId"", ""type"": ""int"" },
        { ""name"": ""taskId"", ""type"": ""int"" },
        { ""name"": ""capacityShare"", ""type"": ""int"" },
        { ""name"": ""startDateMs"", ""type"": ""long"" },
        { ""name"": ""endDateMs"", ""type"": ""long"" },
        { ""name"": ""operation"", ""type"": ""string"" }
      ]
    }";

    public KafkaAssignmentProducer(ProducerConfig producerConfig, SchemaRegistryConfig registryConfig, string topic)
    {
        _topic = topic ?? throw new ArgumentNullException(nameof(topic));
        _schemaRegistry = new CachedSchemaRegistryClient(registryConfig);
        _avroSchema = Avro.Schema.Parse(SchemaString);
        producerConfig.Debug = "broker,protocol";

        var avroSerializer = new AvroSerializer<GenericRecord>(_schemaRegistry, new AvroSerializerConfig { AutoRegisterSchemas = true });

        var builder = new ProducerBuilder<Null, GenericRecord>(producerConfig);
        builder.SetValueSerializer(avroSerializer);
        _producer = builder.Build();
    }

    public async Task ProduceAssignmentAsync(EngineerAssignment assignment, string operation)
    {
        if (assignment == null) throw new ArgumentNullException(nameof(assignment));
        var record = new GenericRecord((RecordSchema)_avroSchema);
        record.Add("assignmentId", assignment.AssignmentId);
        record.Add("engineerId", assignment.EngineerId);
        record.Add("taskId", assignment.TaskId);
        record.Add("capacityShare", assignment.CapacityShare);
        record.Add("startDateMs", assignment.StartDate == default ? 0L : new DateTimeOffset(assignment.StartDate).ToUnixTimeMilliseconds());
        record.Add("endDateMs", assignment.EndDate == default ? 0L : new DateTimeOffset(assignment.EndDate).ToUnixTimeMilliseconds());
        record.Add("operation", operation ?? "unknown");

        var msg = new Message<Null, GenericRecord> { Value = record };

        try
        {
            var delivery = await _producer.ProduceAsync(_topic, msg).ConfigureAwait(false);
            Console.WriteLine($"OK -> {delivery.TopicPartitionOffset}");
        }
        catch (ProduceException<Null, GenericRecord> ex)
        {
            Console.WriteLine($"[ProduceException] Code={ex.Error.Code} IsError={ex.Error.IsError} " +
                            $"IsLocalError={ex.Error.IsLocalError} Reason={ex.Error.Reason}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
            throw;
        }
    }

    public void Dispose()
    {
        try { _producer.Flush(TimeSpan.FromSeconds(5)); } catch { }
        _producer?.Dispose();
        _schemaRegistry?.Dispose();
    }
}
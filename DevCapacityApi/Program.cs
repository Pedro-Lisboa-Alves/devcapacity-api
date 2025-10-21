using Microsoft.EntityFrameworkCore;
using DevCapacityApi.Repositories;
using DevCapacityApi.Services;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using DevCapacityApi.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// register DbContext (SQLite)
//builder.Services.AddDbContext<DevCapacityApi.Data.AppDbContext>(options =>
    //options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<DevCapacityApi.Data.AppDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlite(cs);
});

// DI registrations

// replace in-memory engineer repo with EF implementation
builder.Services.AddScoped<DevCapacityApi.Repositories.IEngineerRepository, DevCapacityApi.Repositories.EfEngineerRepository>();
builder.Services.AddScoped<DevCapacityApi.Services.IEngineerService, DevCapacityApi.Services.EngineerService>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IStatusRepository, StatusRepository>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<DevCapacityApi.Repositories.IEngineerAssignmentRepository, DevCapacityApi.Repositories.EngineerAssignmentRepository>();
builder.Services.AddScoped<DevCapacityApi.Services.IEngineerAssignmentService, DevCapacityApi.Services.EngineerAssignmentService>();
builder.Services.AddScoped<ITasksRepository, TasksRepository>();
builder.Services.AddScoped<ITasksService, TasksService>();
builder.Services.AddScoped<IInitiativesRepository, InitiativesRepository>();
builder.Services.AddScoped<IInitiativesService, InitiativesService>();
builder.Services.AddScoped<ICompanyCalendarRepository, CompanyCalendarRepository>();
builder.Services.AddScoped<ICompanyCalendarService, CompanyCalendarService>();
builder.Services.AddScoped<IEngineerCalendarRepository, EngineerCalendarRepository>();
builder.Services.AddScoped<IEngineerCalendarService, EngineerCalendarService>();

// Kafka / Schema Registry configuration and producer registration
builder.Services.AddSingleton(sp =>
{
    return new ProducerConfig
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092"
    };
});

// MANTER apenas UM registo de SchemaRegistryConfig
builder.Services.AddSingleton(sp => new SchemaRegistryConfig
{
    Url = builder.Configuration["SchemaRegistry:Url"] ?? "http://localhost:8081"
});

var assignmentTopic = builder.Configuration["Kafka:AssignmentTopic"] ?? "engineer-assignment-events";

builder.Services.AddSingleton<IKafkaAssignmentProducer>(sp =>
{
    var prodCfg = sp.GetRequiredService<ProducerConfig>();
    var regCfg = sp.GetRequiredService<SchemaRegistryConfig>();
    return new KafkaAssignmentProducer(prodCfg, regCfg, assignmentTopic);
});

// Kafka consumer config
builder.Services.AddSingleton(sp => new ConsumerConfig
{
    BootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
    GroupId = builder.Configuration["Kafka:AssignmentConsumerGroup"] ?? "assignment-consumer-group",
    AutoOffsetReset = AutoOffsetReset.Earliest,
    EnableAutoCommit = false
});

// register processor implementation
builder.Services.AddScoped<IEngineerAssignmentProcessor, DefaultEngineerAssignmentProcessor>();


// register background consumer
builder.Services.AddSingleton<IHostedService>(sp =>
   new KafkaAssignmentConsumer(
        sp.GetRequiredService<ConsumerConfig>(),
        sp.GetRequiredService<SchemaRegistryConfig>(),
        sp.GetRequiredService<IServiceScopeFactory>(),
        sp.GetRequiredService<ILogger<KafkaAssignmentConsumer>>(),
        assignmentTopic // <- vem do appsettings
    )
);

var app = builder.Build();

// aplicar migrations pendentes automaticamente
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<DevCapacityApi.Data.AppDbContext>();
        db.Database.Migrate();

        var conn = db.Database.GetDbConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "PRAGMA journal_mode=WAL;";
        var mode = cmd.ExecuteScalar()?.ToString();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("SQLite journal_mode set to {Mode}", mode);
        
    }
    catch (Exception ex)
    {
        // log and continue (ou rethrow se preferires)
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao aplicar migrations automaticamente.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

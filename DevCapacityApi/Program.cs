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
builder.Services.AddDbContext<DevCapacityApi.Data.AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// Kafka / Schema Registry configuration and producer registration
builder.Services.AddSingleton(sp =>
{
    // read from configuration; defaults provided for local testing
    return new ProducerConfig
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092"
    };
});

builder.Services.AddSingleton(sp =>
{
    return new SchemaRegistryConfig
    {
        Url = builder.Configuration["SchemaRegistry:Url"] ?? "http://localhost:8081"
    };
});

var assignmentTopic = builder.Configuration["Kafka:AssignmentTopic"] ?? "engineer-assignment-events";

builder.Services.AddSingleton<IKafkaAssignmentProducer>(sp =>
{
    var prodCfg = sp.GetRequiredService<ProducerConfig>();
    var regCfg = sp.GetRequiredService<SchemaRegistryConfig>();
    return new KafkaAssignmentProducer(prodCfg, regCfg, assignmentTopic);
});

var app = builder.Build();

// aplicar migrations pendentes automaticamente
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DevCapacityApi.Data.AppDbContext>();
    db.Database.Migrate();
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

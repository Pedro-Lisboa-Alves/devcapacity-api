using Microsoft.EntityFrameworkCore;

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

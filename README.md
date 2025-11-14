# devcapacity-api

A RESTful API backend for DevCapacity. Handles business logic, data processing, and integration with external systems to power the frontend application.

## Technical summary

This project is an ASP.NET Core REST API that provides task/assignment management and integrates with Kafka for assignment events.

### Supported runtime
- .NET SDK / Runtime: .NET 8 (net8.0)
  - Project and Dockerfile target .NET 8 / ASP.NET Core 8.

### Frameworks & libraries
- ASP.NET Core (Web API)
- Entity Framework Core (EF Core) — SQLite provider for persistence
- Confluent.Kafka — Kafka client
- Confluent.SchemaRegistry — Schema Registry integration (when used)
- Swashbuckle / Swagger — API documentation (if enabled)
- Microsoft.Extensions.* — logging, dependency injection, configuration


### Infrastructure & deployment
- Docker multi-stage image (see DevCapacityApi/Dockerfile)
- Docker Compose for local stacks and integration with Kafka
  - Named Docker volume recommended for SQLite DB persistence (configured as `appdata` in compose)
- Kafka broker + Schema Registry run in separate containers (see kafka compose)

## Configuration

Important configuration keys (can be provided via appsettings.json or environment variables):

- Connection string (SQLite)
  - ConnectionStrings:DefaultConnection or env var `ConnectionStrings__DefaultConnection`
  - Example: `Data Source=/data/devcapacity.db;Mode=ReadWriteCreate;Cache=Shared`

- Kafka
  - `Kafka:Enabled` (env `KAFKA__ENABLED`) — enable Kafka producer/consumer
  - `Kafka:BootstrapServers` (env `KAFKA__BOOTSTRAPSERVERS`) — broker bootstrap (use service hostname on Docker network, e.g. `kafka:9093`)
  - `Kafka:AssignmentTopic` (env `KAFKA__ASSIGNMENTTOPIC`) — topic name
  - `Kafka:AssignmentConsumerGroup` (env `KAFKA__ASSIGNMENTCONSUMERGROUP`) — consumer group

- ASP.NET Core
  - `ASPNETCORE_ENVIRONMENT` (Development / Production)
  - `ASPNETCORE_URLS` (e.g. `http://+:5152`)
  - `ASPNETCORE_HTTPS_PORT` (if HTTPS redirection required)

## Running locally (development)

Prerequisites:
- .NET 8 SDK installed
- Docker & Docker Compose (for full stack with Kafka)

Run with .NET directly:
1. Restore/build:
   - dotnet restore
   - dotnet build
2. Apply EF migrations (ensure ConnectionStrings configured):
   - dotnet tool install --global dotnet-ef --version 8.*
   - dotnet ef database update --project DevCapacityApi
3. Run:
   - dotnet run --project DevCapacityApi

Run with Docker Compose (recommended for integration tests with Kafka):
1. Build & start:
   - docker compose up -d --build
2. Verify logs:
   - docker logs -f <container-id>

Notes:
- When running in Docker, ensure Kafka bootstrap server points to the broker hostname on the same Docker network (for example `kafka:9093`). The broker must advertise an address reachable by containers (see `KAFKA_ADVERTISED_LISTENERS` in the Kafka compose).

## Migrations
- Migrations are defined under `DevCapacityApi/Migrations`.
- To add a migration locally:
  - dotnet ef migrations add <Name> --project DevCapacityApi
- To apply:
  - dotnet ef database update --project DevCapacityApi


## Contributing
- Follow project coding standards.
- Add unit tests under Services/Repositories as appropriate.
- Run migrations and ensure DB model compatibility before pushing.

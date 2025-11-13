# ===== Runtime base =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5152

ENV ASPNETCORE_URLS=http://+:5152

# create data dir and ensure permissions for the runtime user 'app'
USER root
RUN mkdir -p /data && chown -R app:root /data && chmod 770 /data

# switch to non-root user for security
USER app

# ===== Build =====
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src

COPY ["DevCapacityApi/DevCapacityApi.csproj", "DevCapacityApi/"]
RUN dotnet restore "DevCapacityApi/DevCapacityApi.csproj"

COPY . .
WORKDIR /src/DevCapacityApi
RUN dotnet publish "DevCapacityApi.csproj" -c ${configuration} -o /app/publish /p:UseAppHost=false

# ===== Final =====
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DevCapacityApi.dll"]

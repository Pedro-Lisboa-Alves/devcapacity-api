FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5152

ENV ASPNETCORE_URLS=http://+:5152

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["DevCapacityApi/DevCapacityApi.csproj", "DevCapacityApi/"]
RUN dotnet restore "DevCapacityApi/DevCapacityApi.csproj"
COPY . .
WORKDIR "/src/DevCapacityApi"
RUN dotnet build "DevCapacityApi.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "DevCapacityApi.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DevCapacityApi.dll"]

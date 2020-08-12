#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Export/WB.Services.Export.Host/WB.Services.Export.Host.csproj", "Export/WB.Services.Export.Host/"]
COPY ["../Core/Infrastructure/WB.Infrastructure.AspNetCore/WB.Infrastructure.AspNetCore.csproj", "../Core/Infrastructure/WB.Infrastructure.AspNetCore/"]
COPY ["Core/WB.Services.Scheduler/WB.Services.Scheduler.csproj", "Core/WB.Services.Scheduler/"]
COPY ["Core/WB.Services.Infrastructure/WB.Services.Infrastructure.csproj", "Core/WB.Services.Infrastructure/"]
COPY ["Export/WB.Services.Export/WB.Services.Export.csproj", "Export/WB.Services.Export/"]
RUN dotnet restore "Export/WB.Services.Export.Host/WB.Services.Export.Host.csproj"
COPY . .
WORKDIR "/src/Export/WB.Services.Export.Host"
RUN dotnet build "WB.Services.Export.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WB.Services.Export.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WB.Services.Export.Host.dll"]
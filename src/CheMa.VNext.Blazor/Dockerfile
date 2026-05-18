FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["CheMa.VNext.sln", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["NuGet.Config", "."]

COPY ["src/CheMa.VNext.ServiceDefaults/CheMa.VNext.ServiceDefaults.csproj", "src/CheMa.VNext.ServiceDefaults/"]
COPY ["src/CheMa.VNext.Infrastructure/CheMa.VNext.Infrastructure.csproj", "src/CheMa.VNext.Infrastructure/"]
COPY ["src/CheMa.VNext.HttpApi.Host/CheMa.VNext.HttpApi.Host.csproj", "src/CheMa.VNext.HttpApi.Host/"]
COPY ["src/CheMa.VNext.HttpApi.Client/CheMa.VNext.HttpApi.Client.csproj", "src/CheMa.VNext.HttpApi.Client/"]
COPY ["src/CheMa.VNext.HttpApi/CheMa.VNext.HttpApi.csproj", "src/CheMa.VNext.HttpApi/"]
COPY ["src/CheMa.VNext.EntityFrameworkCore/CheMa.VNext.EntityFrameworkCore.csproj", "src/CheMa.VNext.EntityFrameworkCore/"]
COPY ["src/CheMa.VNext.Gateway/CheMa.VNext.Gateway.csproj", "src/CheMa.VNext.Gateway/"]
COPY ["src/CheMa.VNext.Domain.Shared/CheMa.VNext.Domain.Shared.csproj", "src/CheMa.VNext.Domain.Shared/"]
COPY ["src/CheMa.VNext.Domain/CheMa.VNext.Domain.csproj", "src/CheMa.VNext.Domain/"]
COPY ["src/CheMa.VNext.DbMigrator/CheMa.VNext.DbMigrator.csproj", "src/CheMa.VNext.DbMigrator/"]
COPY ["src/CheMa.VNext.Blazor.Client/CheMa.VNext.Blazor.Client.csproj", "src/CheMa.VNext.Blazor.Client/"]
COPY ["src/CheMa.VNext.Blazor/CheMa.VNext.Blazor.csproj", "src/CheMa.VNext.Blazor/"]
COPY ["src/CheMa.VNext.BackgroundWorker/CheMa.VNext.BackgroundWorker.csproj", "src/CheMa.VNext.BackgroundWorker/"]
COPY ["src/CheMa.VNext.BackgroundJobs/CheMa.VNext.BackgroundJobs.csproj", "src/CheMa.VNext.BackgroundJobs/"]
COPY ["src/CheMa.VNext.Application.Contracts/CheMa.VNext.Application.Contracts.csproj", "src/CheMa.VNext.Application.Contracts/"]
COPY ["src/CheMa.VNext.Application/CheMa.VNext.Application.csproj", "src/CheMa.VNext.Application/"]
COPY ["src/CheMa.VNext.AppHost/CheMa.VNext.AppHost.csproj", "src/CheMa.VNext.AppHost/"]

RUN dotnet restore "src/CheMa.VNext.Blazor/CheMa.VNext.Blazor.csproj"

COPY . .

RUN dotnet publish "src/CheMa.VNext.Blazor/CheMa.VNext.Blazor.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
COPY --from=build --chown=app:app /app/publish .
USER app
ENTRYPOINT ["dotnet", "CheMa.VNext.Blazor.dll"]

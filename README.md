# Cosmos data indexer

Ths repository contains a Cosmos block and transaction indexer.

## Getting started

This code is written in dotnet 6 which is available cross platform.

Ensure that you have the latest dotnet 6 SDK installed and Docker (+docker compose), then getting started is as simple as:

```bash
docker compose up -d
```

And then run the API

```bash
dotnet run --project src/ServiceHost/ServiceHost.csproj
```

The default launch-settings _will not_ start downloading data, as this is both CPU and network heavy.
If you want to download data, edit the file `src/ServiceHost/Properties/launchSettings.json` and add 
`BACKGROUND_WORKER` to the `SAPIENT_SERVICE_ROLES_ENABLED` config variable.

## Code Structure

The solution structure is based on the [Clean Architecture pattern](https://github.com/ardalis/cleanarchitecture)
which is also described
as [Common web application architectures](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#clean-architecture)
by Microsoft.

- **terra dotnet**
    - To be separate repository down the road with everything related to parsing Terra transactions / messages

- **Kernel types**
    - Entities (business model classes that are persisted)
    - Aggregates (groups of entities)
    - Interfaces
    - Domain Services
    - Specifications
    - Custom Exceptions and Guard Clauses
    - Domain Events / Contracts
    - Extension methods

- **Infrastructure types**
    - Database migrations and setup / configuration
    - Data access implementation types (Repositories)
    - Infrastructure-specific services (for example, FileLogger or SmtpNotifier)
    - MassTransit Consumers
    - Terra data fetchers
    - API integrations

- **ServiceHost types**
    - GraphQL resolvers/mutations and types
    - MassTransit configuration
    - Startup
    - Configuration parsing and setup

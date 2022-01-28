# Pylon Board API / Backend

This repository contains the API/backend and data synchronization code for Pylon Board

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
If you want to download data, edit the file `src/ServiceHost/Properties/launchSettings.json` and add `BACKGROUND_WORKER` to the `PYLONBOARD_SERVICE_ROLES_ENABLED` config variable.


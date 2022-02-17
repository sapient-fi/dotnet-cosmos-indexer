FROM mcr.microsoft.com/dotnet/sdk:6.0-focal as builder

# copy the somewhat static csproj file to
# enable caching of the restore
COPY ./src/TerraDotnet/TerraDotnet.csproj ./TerraDotnet/TerraDotnet.csproj
COPY ./src/Kernel/Kernel.csproj ./Kernel/Kernel.csproj
COPY ./src/Infrastructure/Infrastructure.csproj ./Infrastructure/Infrastructure.csproj
COPY ./src/ServiceHost/ServiceHost.csproj ./ServiceHost/ServiceHost.csproj


# RUN dotnet restore
RUN dotnet restore ./ServiceHost/ServiceHost.csproj

# copy it all 
COPY ./src/TerraDotnet ./TerraDotnet
COPY ./src/Kernel ./Kernel
COPY ./src/Infrastructure ./Infrastructure
COPY ./src/ServiceHost ./ServiceHost

# Build and publish
RUN dotnet publish --no-restore -c release -o /app/published ./ServiceHost/ServiceHost.csproj

##
# And now build the production image
##
FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal
COPY --from=builder /app/published /app

# Install the agent
RUN apt-get update && apt-get install -y wget ca-certificates gnupg \
&& echo 'deb http://apt.newrelic.com/debian/ newrelic non-free' | tee /etc/apt/sources.list.d/newrelic.list \
&& wget https://download.newrelic.com/548C16BF.gpg \
&& apt-key add 548C16BF.gpg \
&& apt-get update \
&& apt-get install -y newrelic-netcore20-agent \
&& rm -rf /var/lib/apt/lists/*

# Enable the agent
ENV CORECLR_ENABLE_PROFILING=1 \
CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
CORECLR_NEWRELIC_HOME=/usr/local/newrelic-netcore20-agent \
CORECLR_PROFILER_PATH=/usr/local/newrelic-netcore20-agent/libNewRelicProfiler.so \
NEW_RELIC_LICENSE_KEY=eu01xx1ac5c939b087f58a69276b7688d91dNRAL \
NEW_RELIC_APP_NAME=Pylonboard-backend

WORKDIR /app
EXPOSE 5000/tcp

CMD ["/app/Pylonboard.ServiceHost"]

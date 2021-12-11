FROM mcr.microsoft.com/dotnet/sdk:6.0-focal as builder

# copy the somewhat static csproj file to
# enable caching of the restore
COPY ./src/Kernel/Kernel.csproj ./Kernel/Kernel.csproj
COPY ./src/Contracts/Contracts.csproj ./Contracts/Contracts.csproj
COPY ./src/ServiceHost/ServiceHost.csproj ./ServiceHost/ServiceHost.csproj


# RUN dotnet restore
RUN dotnet restore ./ServiceHost/ServiceHost.csproj

# copy it all 
COPY ./src/Kernel ./Kernel
COPY ./src/Contracts ./Contracts
COPY ./src/ServiceHost ./ServiceHost

# Build and publish
RUN dotnet publish --no-restore -c release -o /app/published ./ServiceHost/ServiceHost.csproj

##
# And now build the production image
##
FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal
COPY --from=builder /app/published /app

WORKDIR /app
EXPOSE 5000/tcp

CMD ["/app/Pylonboard.ServiceHost"]

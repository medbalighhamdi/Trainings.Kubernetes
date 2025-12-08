# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy entire backend folder (all projects)
COPY . .

# Grant execution rights to shell script
RUN chmod +x /src/Scripts/wait-for.sh

# Go to API project folder
WORKDIR /src/Api

# Restore, build, and publish API project
RUN dotnet restore
RUN dotnet publish -c Release -o /app --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install nc command which is necessary for redis readiness ping commands
RUN apt-get update \
    && apt-get install -y netcat-openbsd \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app .

# Copy wait script for redis integration
# And grant it the execution rights
COPY --from=build /src/Scripts/wait-for.sh /app/wait-for.sh
RUN chmod +x /app/wait-for.sh

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

# It is important to do not set entry point in order to be able to set entry point on docker compose level
# From now on, run individual services using docker compose run <service-name> command
ENTRYPOINT []
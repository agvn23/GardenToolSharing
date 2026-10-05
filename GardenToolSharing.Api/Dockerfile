# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy just the csproj first so `dotnet restore` is cached unless dependencies actually change
COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app --no-restore

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GardenToolSharing.Api.dll"]

# Optional hardening for later: run as a non-root user. Left out for now because the SQLite
# volume mounted at /data is owned by root by default, and a non-root user needs that fixed
# first (e.g. an entrypoint script that chowns /data, or a `user:` override in Compose).
# RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
# USER appuser

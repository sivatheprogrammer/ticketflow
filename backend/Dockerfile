# =============================================================================
# Stage 1: Build
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy source and publish
COPY src/ src/
RUN dotnet publish src/TicketFlow.Api/TicketFlow.Api.csproj \
    -c Release \
    -o /app/publish

# =============================================================================
# Stage 2: Runtime
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Run as the non-root 'app' user that ships built into the .NET runtime image
USER app

COPY --from=build /app/publish .

# ASP.NET Core will listen on this port inside the container
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TicketFlow.Api.dll"]

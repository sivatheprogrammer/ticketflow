# ADR-013: Azure Service Bus Local Emulator for Development

## Status
Accepted

## Date
2026-06-14

## Context
Phase 5 introduces async messaging between services using Azure Service Bus.
During local development, connecting to a real Azure Service Bus instance
would incur costs and require internet connectivity.

A local development strategy is needed that:
- Mirrors production behavior as closely as possible
- Has zero cost during development
- Requires minimal setup
- Allows seamless transition to real Azure Service Bus in production

## Decision
Use the **Azure Service Bus Emulator** 
(`mcr.microsoft.com/azure-messaging/servicebus-emulator`) running as a
Docker container for local development.

Production deployments (Phase 6) will use real Azure Service Bus with
connection strings injected via environment variables or Azure App Config.

## Consequences

### Positive
- Zero cost during local development
- Works offline
- Same application code works locally and in Azure — only connection
  string changes
- Docker already in use (Redis emulator pattern established in Phase 3)
- No Azure resources to provision or tear down during development

### Negative
- Requires Docker running locally
- Minor behavioral differences from production Service Bus (edge cases)
- Team members need Docker installed
- Requires SQL Edge as a dependency (adds complexity to docker-compose)

### Neutral
- Connection string switching handled via `appsettings.json` locally
  and environment variables in Azure
- Same pattern used for Redis (local Docker → Azure Redis Cache)

## Alternatives Considered
- **Real Azure Service Bus:** Costs money, requires internet, overkill for dev
- **RabbitMQ:** Popular but not Azure-native; migration to Service Bus
  would require code changes
- **In-memory queue:** Fast but doesn't test real messaging behavior
- **Azure Storage Queues:** Simpler but fewer features than Service Bus

## Known Gotchas (Lessons Learned)

### 1. Namespace name is fixed
The emulator only supports the namespace name `sbemulatorns` (exactly 12
characters). Any other name causes the emulator to crash with:

### 2. SQL Edge dependency required
The emulator requires `mcr.microsoft.com/azure-sql-edge` as a backing
store. Must use `docker-compose` with both containers — running the
emulator standalone will crash immediately.

### 3. Logging config must not be null
The `config.json` must include a `Logging` section:
```json
"Logging": {
  "Type": "console"
}
```
Omitting this causes a `NullReferenceException` on startup.

### 4. Transport type must be AmqpWebSockets
The connection string must use `AmqpWebSockets` transport:

And the `ServiceBusClient` must be configured with:
```csharp
var clientOptions = new ServiceBusClientOptions
{
    TransportType = ServiceBusTransportType.AmqpWebSockets
};
```

### 5. Correct config.json structure
```json
{
  "UserConfig": {
    "Namespaces": [
      {
        "Name": "sbemulatorns",
        "Queues": [
          { "Name": "booking-created" },
          { "Name": "booking-confirmed" }
        ]
      }
    ],
    "Logging": {
      "Type": "console"
    }
  }
}
```
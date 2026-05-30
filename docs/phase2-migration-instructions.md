# Phase 2 — EF Core Migration Instructions

## What Changed in the Database

The `Customer` entity got a new `ExternalId` column:
- Type: `nvarchar(128)` (nullable)
- Unique filtered index: `WHERE ExternalId IS NOT NULL`

## Run the Migration

In your terminal from the `backend` folder:

```bash
dotnet ef migrations add Phase2_AddCustomerExternalId `
  --project src\TicketFlow.Infrastructure `
  --startup-project src\TicketFlow.Api `
  --output-dir Persistence\Migrations

dotnet ef database update `
  --project src\TicketFlow.Infrastructure `
  --startup-project src\TicketFlow.Api
```

## What the Migration Will Create

```sql
ALTER TABLE [Customers] ADD [ExternalId] nvarchar(128) NULL;

CREATE UNIQUE INDEX [IX_Customers_ExternalId] 
  ON [Customers] ([ExternalId]) 
  WHERE [ExternalId] IS NOT NULL;
```

The filtered index (`WHERE ExternalId IS NOT NULL`) is important —
it allows multiple NULL values (Phase 1 seed customers) while
enforcing uniqueness on real values (authenticated users).

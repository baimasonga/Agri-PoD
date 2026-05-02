# Local Runbook

## Current Local Runtime

Development uses SQLite so the app can run on machines without SQL Server LocalDB.

- API: `http://localhost:5068`
- API docs: `http://localhost:5068/swagger`
- Portal: `http://localhost:5253`
- Local database file: `agri-pod-system/agri-pod-dev.db`

## Demo Login Accounts

Use password `demo` for all seeded role demos.

- `admin@agripod.local` - SystemAdministrator
- `manager@agripod.local` - ProjectManager
- `procurement@agripod.local` - ProcurementOfficer
- `warehouse@agripod.local` - WarehouseManager
- `coordinator@agripod.local` - DistrictCoordinator
- `field@agripod.local` - FieldOfficer
- `driver@agripod.local` - Driver
- `me@agripod.local` - MonitoringEvaluationOfficer
- `audit@agripod.local` - Auditor

The API enforces JWT bearer authentication and role restrictions on workflow controllers. The portal login page stores the JWT for the current Blazor Server circuit and attaches it to API calls.

The API seeds demo data on startup:

- Users and roles
- Districts
- Farmers
- Inventory items
- Warehouses and stock lots
- Supplier and purchase order
- Campaign and farmer allocations
- Vehicles, dispatch, GPS pings
- Delivery, exception, audit records

## Verification Commands

```powershell
dotnet build .\AgriPod.sln
dotnet test .\backend\AgriPod.Tests\AgriPod.Tests.csproj
cd .\mobile\agri_pod_mobile
flutter analyze
flutter test
```

## Production Database

For Azure SQL or SQL Server Express district nodes, set:

```json
{
  "Database": {
    "Provider": "SqlServer"
  },
  "ConnectionStrings": {
    "AgriPod": "..."
  }
}
```

Use EF migrations or the SQL snapshot in `database/migrations` to provision production databases. Store Twilio and JWT secrets outside source control.

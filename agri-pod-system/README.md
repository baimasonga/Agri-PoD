# Agri-PoD Inventory and Distribution System

Production-oriented monorepo for agriculture inventory and proof of delivery in Sierra Leone.

## Stack

- ASP.NET Core Web API with controller endpoints and OpenAPI.
- Blazor web portal for national and district operations.
- Flutter mobile app with local-first SQLite writes and later sync.
- Azure SQL for the online database.
- SQL Server Express for district/offline nodes.
- Twilio Verify for OTP.
- GPS, barcode/QR, facial biometric reference capture, and vehicle tracking contracts.

## Workflow Modules

- Authentication, JWT issuing, users, roles, districts, and operating scope.
- Farmer registration, GPS/photo evidence, QR/barcode identity, and district approval.
- Procurement, supplier management, purchase orders, stock receiving, batches, expiry, and package traceability.
- Campaign planning, eligible farmer allocation, warehouse stock assignment, vehicles, drivers, field officers, and GPS boundaries.
- Vehicle dispatch, manifest barcode validation, GPS tracking, and distribution-session start.
- Proof of Delivery with farmer barcode, package barcode, Twilio OTP evidence, face reference, GPS, vehicle proximity, and quantity validation.
- Offline mobile and district SQL Express sync with conflict/exception handling.
- Stock reconciliation, reports, exception handling, and audit logs.

## Quick Start

```powershell
dotnet restore .\AgriPod.sln
dotnet build .\AgriPod.sln
dotnet run --project .\backend\AgriPod.Api\AgriPod.Api.csproj
dotnet run --project .\web\AgriPod.Blazor\AgriPod.Blazor\AgriPod.Blazor.csproj
cd .\mobile\agri_pod_mobile
flutter analyze
```

See `docs/deployment/local-runbook.md` for local runtime URLs, seeded data, and production database notes.

Local development uses SQLite by default via `backend/AgriPod.Api/appsettings.Development.json`. Production should use Azure SQL or SQL Server Express by setting `Database:Provider` to `SqlServer`.

Configure secrets outside source control:

- `ConnectionStrings:AgriPod`
- `TwilioVerify:AccountSid`
- `TwilioVerify:AuthToken`
- `TwilioVerify:VerifyServiceSid`

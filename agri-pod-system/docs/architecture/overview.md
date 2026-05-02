# Architecture Overview

Agri-PoD is split into clean layers:

- `AgriPod.Domain`: entities and business invariants.
- `AgriPod.Application`: use cases and service interfaces.
- `AgriPod.Infrastructure`: EF Core, SQL Server or SQLite configuration, Twilio Verify.
- `AgriPod.Api`: controller-based Web API and OpenAPI.
- `AgriPod.Blazor`: operations portal.
- `agri_pod_mobile`: offline-first Flutter field capture.

## Offline First

The mobile app writes proof-of-delivery data into SQLite before attempting network sync. District SQL Server Express nodes follow the same mutation-envelope pattern, making field devices and district servers resilient to poor connectivity.

## Proof of Delivery

Proof data is represented as events: OTP verified, barcode scanned, face reference captured, GPS recorded, delivered, or exception. Biometric media should be stored in encrypted object storage, while the database stores references and audit metadata.

## Workflow Coverage

The codebase now models the full operating chain:

1. JWT login and role-based user records.
2. Farmer registration with GPS, photo reference, value chain, QR/barcode token, and district review.
3. Procurement, supplier registration, purchase orders, stock receiving, batches, expiry, and package traceability.
4. Campaign planning, farmer allocation, vehicles, drivers, field officers, and site GPS boundaries.
5. Vehicle dispatch, manifest barcode validation, GPS tracking, and session start.
6. Proof of Delivery validation using farmer barcode, package barcode, OTP evidence, face reference, GPS, vehicle proximity, and quantity rules.
7. Offline sync envelopes from mobile and district SQL Express nodes.
8. Exceptions, reconciliation hooks, reporting, and audit logs.

# SQL Server Express District Sync

District nodes run SQL Server Express and relay changes to the central API through `POST /api/v1/sync`.

## Rules

- Every local mutation must have a globally unique `ClientMutationId`.
- Local writes are accepted first, then replayed to the API when connectivity returns.
- The API returns accepted mutation IDs and a monotonically increasing `ServerVersion`.
- Conflicts should be resolved by business priority: delivered proof events win over draft delivery edits, while stock adjustments require supervisor review.
- District nodes should retain a local audit log even after central acceptance.
- PoD sync must include farmer barcode, package barcode, OTP result, face capture reference, GPS point, vehicle registration, and delivered quantity.
- The server rejects or flags duplicate package delivery, over-allocation, missing OTP, missing face evidence, missing GPS, and missing vehicle proximity.

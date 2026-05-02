# Azure Deployment Notes

## API and Portal

- Deploy `AgriPod.Api` and `AgriPod.Blazor` as separate App Services or container apps.
- Use managed identity where possible.
- Store Twilio Verify secrets in Key Vault.
- Set `ConnectionStrings:AgriPod` to Azure SQL.

## District Node

- Install SQL Server Express.
- Configure a district relay service to submit mutation envelopes to the central API.
- Use local encrypted backups and retry queues.

## Mobile

- Configure Android and iOS permissions for camera, location, and network access.
- Set the API base URL through environment-specific Flutter configuration.

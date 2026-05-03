using AgriPod.Blazor.Components;
using AgriPod.Blazor.Security;
using AgriPod.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<PortalAuthState>();
builder.Services.AddTransient<AuthHeaderHandler>();
builder.Services.AddHttpClient("AgriPodApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5068");
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(AgriPod.Blazor.Client._Imports).Assembly);

app.MapPost("/portal-actions/users/create", async (HttpRequest request, IHttpClientFactory httpClientFactory) =>
{
    var form = await request.ReadFormAsync();
    var result = await PostToApiAsync(
        httpClientFactory,
        "/api/v1/users",
        new CreateUserRequest(Read(form, "fullName"), Read(form, "email"), Read(form, "phone"), Read(form, "role"), Read(form, "districtCode")),
        "Created user.");

    return Results.Redirect($"/users?message={Uri.EscapeDataString(result)}");
}).DisableAntiforgery();

app.MapPost("/portal-actions/inventory/create-item", async (HttpRequest request, IHttpClientFactory httpClientFactory) =>
{
    var form = await request.ReadFormAsync();
    var sku = Read(form, "sku");
    var result = await PostToApiAsync(
        httpClientFactory,
        "/api/v1/inventory-items",
        new CreateInventoryItemRequest(sku, Read(form, "name"), Read(form, "category"), Read(form, "unitOfMeasure")),
        $"Created item {sku}.");

    return Results.Redirect($"/inventory?message={Uri.EscapeDataString(result)}");
}).DisableAntiforgery();

app.MapPost("/portal-actions/procurement/create-supplier", async (HttpRequest request, IHttpClientFactory httpClientFactory) =>
{
    var form = await request.ReadFormAsync();
    var supplierName = Read(form, "name");
    var result = await PostToApiAsync(
        httpClientFactory,
        "/api/v1/procurement/suppliers",
        new CreateSupplierRequest(supplierName, Read(form, "phone")),
        $"Created supplier {supplierName}.");

    return Results.Redirect($"/procurement?message={Uri.EscapeDataString(result)}");
}).DisableAntiforgery();

app.MapPost("/portal-actions/procurement/create-purchase-order", async (HttpRequest request, IHttpClientFactory httpClientFactory) =>
{
    var form = await request.ReadFormAsync();
    if (!Guid.TryParse(Read(form, "supplierId"), out var supplierId) ||
        !Guid.TryParse(Read(form, "inventoryItemId"), out var itemId) ||
        !decimal.TryParse(Read(form, "quantity"), out var quantity))
    {
        return Results.Redirect("/procurement?message=Purchase%20order%20requires%20supplier%2C%20item%2C%20and%20quantity.");
    }

    var purchaseNumber = Read(form, "purchaseNumber");
    var result = await PostToApiAsync(
        httpClientFactory,
        "/api/v1/procurement/purchase-orders",
        new CreatePurchaseOrderRequest(purchaseNumber, supplierId, itemId, quantity, Read(form, "batchNumber")),
        $"Created purchase order {purchaseNumber}.");

    return Results.Redirect($"/procurement?message={Uri.EscapeDataString(result)}");
}).DisableAntiforgery();

app.MapPost("/portal-actions/procurement/receive-stock", async (HttpRequest request, IHttpClientFactory httpClientFactory) =>
{
    var form = await request.ReadFormAsync();
    if (!Guid.TryParse(Read(form, "inventoryItemId"), out var itemId) ||
        !Guid.TryParse(Read(form, "warehouseId"), out var warehouseId) ||
        !decimal.TryParse(Read(form, "quantity"), out var quantity))
    {
        return Results.Redirect("/procurement?message=Stock%20receipt%20requires%20item%2C%20warehouse%2C%20and%20quantity.");
    }

    DateOnly? expiresOn = DateOnly.TryParse(Read(form, "expiresOn"), out var parsedDate) ? parsedDate : null;
    var lotCode = Read(form, "lotCode");
    var result = await PostToApiAsync(
        httpClientFactory,
        "/api/v1/procurement/stock-receipts",
        new ReceiveStockRequest(itemId, warehouseId, lotCode, quantity, expiresOn),
        $"Received stock lot {lotCode}.");

    return Results.Redirect($"/procurement?message={Uri.EscapeDataString(result)}");
}).DisableAntiforgery();

app.MapPost("/portal-actions/campaigns/create", async (HttpRequest request, IHttpClientFactory httpClientFactory) =>
{
    var form = await request.ReadFormAsync();
    var campaignName = Read(form, "name");
    var result = await PostToApiAsync(
        httpClientFactory,
        "/api/v1/campaigns",
        new CreateCampaignRequest(
            campaignName,
            Read(form, "districtCode"),
            Read(form, "valueChain"),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            8.889m,
            -12.044m,
            150m),
        $"Created campaign {campaignName}.");

    return Results.Redirect($"/campaigns?message={Uri.EscapeDataString(result)}");
}).DisableAntiforgery();

app.MapPost("/portal-actions/farmers/register", async (HttpRequest request, IHttpClientFactory httpClientFactory) =>
{
    var form = await request.ReadFormAsync();
    var result = await PostToApiAsync(
        httpClientFactory,
        "/api/v1/farmers",
        new RegisterFarmerRequest(
            Read(form, "fullName"),
            Read(form, "nationalId"),
            Read(form, "phone"),
            Read(form, "districtCode"),
            Read(form, "chiefdom"),
            Read(form, "community"),
            Read(form, "valueChain"),
            8.889m,
            -12.044m,
            $"portal-photo://{Guid.NewGuid():N}"),
        "Registered farmer for district review.");

    return Results.Redirect($"/farmers?message={Uri.EscapeDataString(result)}");
}).DisableAntiforgery();

app.MapPost("/portal-actions/farmers/review", async (HttpRequest request, IHttpClientFactory httpClientFactory) =>
{
    var form = await request.ReadFormAsync();
    var id = Read(form, "id");
    if (!Guid.TryParse(id, out _))
    {
        return Results.Redirect("/farmers?message=Missing%20farmer%20id.");
    }

    var approved = bool.TryParse(Read(form, "approved"), out var value) && value;
    var result = await PostToApiAsync(
        httpClientFactory,
        $"/api/v1/farmers/{id}/review",
        new ReviewFarmerRequest(approved, approved ? "Approved in portal." : "Rejected in portal."),
        approved ? "Farmer approved." : "Farmer rejected.");

    return Results.Redirect($"/farmers?message={Uri.EscapeDataString(result)}");
}).DisableAntiforgery();

app.MapPost("/portal-actions/campaigns/approve", async (HttpRequest request, IHttpClientFactory httpClientFactory) =>
{
    var form = await request.ReadFormAsync();
    var id = Read(form, "id");
    if (!Guid.TryParse(id, out _))
    {
        return Results.Redirect("/campaigns?message=Missing%20campaign%20id.");
    }

    var result = await PostToApiAsync<object?>(
        httpClientFactory,
        $"/api/v1/campaigns/{id}/approve",
        null,
        "Campaign approved.");

    return Results.Redirect($"/campaigns?message={Uri.EscapeDataString(result)}");
}).DisableAntiforgery();

app.MapPost("/portal-actions/campaigns/allocate", async (HttpRequest request, IHttpClientFactory httpClientFactory) =>
{
    var form = await request.ReadFormAsync();
    if (!Guid.TryParse(Read(form, "id"), out var campaignId))
    {
        return Results.Redirect("/campaigns?message=Missing%20campaign%20id.");
    }

    var client = httpClientFactory.CreateClient("AgriPodApi");
    var farmers = await client.GetFromJsonAsync<List<FarmerDto>>("/api/v1/farmers") ?? [];
    var items = await client.GetFromJsonAsync<List<InventoryItemDto>>("/api/v1/inventory-items") ?? [];
    var farmer = farmers.FirstOrDefault(x => x.Status == "Approved");
    var item = items.FirstOrDefault();
    var result = farmer is null || item is null
        ? "Need at least one approved farmer and inventory item."
        : await PostToApiAsync(
            httpClientFactory,
            "/api/v1/campaigns/allocations",
            new AllocateFarmerRequest(campaignId, farmer.Id, item.Id, 50m),
            $"Allocated {item.Name} to {farmer.FullName}.");

    return Results.Redirect($"/campaigns?message={Uri.EscapeDataString(result)}");
}).DisableAntiforgery();

app.MapPost("/portal-actions/compliance/resolve", async (HttpRequest request, IHttpClientFactory httpClientFactory) =>
{
    var form = await request.ReadFormAsync();
    var id = Read(form, "id");
    if (!Guid.TryParse(id, out _))
    {
        return Results.Redirect("/compliance?message=Missing%20exception%20id.");
    }

    var result = await PostToApiAsync(
        httpClientFactory,
        $"/api/v1/compliance/exceptions/{id}/resolve",
        new ResolveExceptionCaseRequest("Resolved in portal."),
        "Exception resolved.");

    return Results.Redirect($"/compliance?message={Uri.EscapeDataString(result)}");
}).DisableAntiforgery();

app.MapPost("/portal-actions/fleet/register-vehicle", async (HttpRequest request, IHttpClientFactory httpClientFactory) =>
{
    var form = await request.ReadFormAsync();
    var registration = Read(form, "registration");
    var result = await PostToApiAsync(
        httpClientFactory,
        "/api/v1/dispatch/vehicles",
        new RegisterVehicleRequest(registration, Read(form, "driverUserId"), true),
        $"Registered vehicle {registration}.");

    return Results.Redirect($"/fleet?message={Uri.EscapeDataString(result)}");
}).DisableAntiforgery();

app.MapPost("/portal-actions/fleet/create-dispatch", async (HttpRequest request, IHttpClientFactory httpClientFactory) =>
{
    var form = await request.ReadFormAsync();
    if (!Guid.TryParse(Read(form, "campaignId"), out var campaignId) ||
        !Guid.TryParse(Read(form, "warehouseId"), out var warehouseId))
    {
        return Results.Redirect("/fleet?message=Dispatch%20requires%20campaign%20and%20warehouse.");
    }

    var result = await PostToApiAsync(
        httpClientFactory,
        "/api/v1/dispatch",
        new CreateDispatchRequest(campaignId, warehouseId, Read(form, "vehicleRegistration"), Read(form, "driverUserId")),
        "Dispatch manifest created.");

    return Results.Redirect($"/fleet?message={Uri.EscapeDataString(result)}");
}).DisableAntiforgery();

app.MapPost("/portal-actions/fleet/confirm-loaded", async (HttpRequest request, IHttpClientFactory httpClientFactory) =>
{
    var form = await request.ReadFormAsync();
    var id = Read(form, "id");
    if (!Guid.TryParse(id, out _))
    {
        return Results.Redirect("/fleet?message=Missing%20dispatch%20id.");
    }

    var result = await PostToApiAsync<object?>(
        httpClientFactory,
        $"/api/v1/dispatch/{id}/confirm-loaded",
        null,
        "Dispatch confirmed loaded.");

    return Results.Redirect($"/fleet?message={Uri.EscapeDataString(result)}");
}).DisableAntiforgery();

app.Run();

static string Read(IFormCollection form, string key) => form.TryGetValue(key, out var value) ? value.ToString() : "";

static async Task<string> PostToApiAsync<TRequest>(IHttpClientFactory httpClientFactory, string path, TRequest payload, string successMessage)
{
    try
    {
        var client = httpClientFactory.CreateClient("AgriPodApi");
        var response = await client.PostAsJsonAsync(path, payload);
        return response.IsSuccessStatusCode ? successMessage : $"Create failed: {response.StatusCode}";
    }
    catch (Exception ex)
    {
        return $"Create failed: {ex.Message}";
    }
}

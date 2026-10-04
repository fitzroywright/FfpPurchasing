using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var root = Path.Combine(app.Environment.ContentRootPath, "data");
Directory.CreateDirectory(root);
var attachments = Path.Combine(root, "attachments");
Directory.CreateDirectory(attachments);

var vendors = new[]
{
    new { Id="V-00123", Name="Metro Scientific Corporation", PayTo="Metro Scientific Corporation", TaxNumber="000-123-456-000", Address="123 Science Street", ContactPerson="Ana Lim", Phone="(02) 8123 4567", Email="sales@metrosci.example", Active=true },
    new { Id="V-00456", Name="Metro Hardware & Construction Supply", PayTo="Metro Hardware & Construction Supply", TaxNumber="000-456-789-000", Address="456 Main Street", ContactPerson="Mark Reid", Phone="(876) 555-0142", Email="accounts@metrohardware.example", Active=true },
    new { Id="V-00789", Name="Metro Office Solutions Inc.", PayTo="Metro Office Solutions Inc.", TaxNumber="000-789-123-000", Address="789 Office Park", ContactPerson="Janet Brown", Phone="(876) 555-0195", Email="billing@metrooffice.example", Active=true },
    new { Id="V-00987", Name="Philippine Training Center Inc.", PayTo="Philippine Training Center Inc.", TaxNumber="000-987-654-000", Address="12 Learning Avenue", ContactPerson="Roberto Cruz", Phone="(02) 8777 9988", Email="info@ptc.example", Active=true }
};

var purchaseOrders = new[]
{
    new { Id="PO-10421", PurchaseOrderNo="10421", RequisitionNo="8721", VendorId="V-00987", VendorName="Philippine Training Center Inc.", Description="Advanced Laboratory Techniques Training", Total=75000m, Requested=30000m },
    new { Id="PO-10418", PurchaseOrderNo="10418", RequisitionNo="8714", VendorId="V-00123", VendorName="Metro Scientific Corporation", Description="Laboratory supplies", Total=109000m, Requested=0m }
};

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "FFP.Purchasing.Api / FFP Manager Bridge mock",
    utc = DateTimeOffset.UtcNow
}));

// Bridge contract seam. Replace these mock handlers with the real FFP Manager Bridge.
app.MapGet("/api/bridge/vendors", () => Results.Ok(vendors));

app.MapGet("/api/bridge/purchase-orders", (string? search) =>
{
    if (string.IsNullOrWhiteSpace(search))
        return Results.Ok(purchaseOrders);

    return Results.Ok(purchaseOrders.Where(x =>
        x.PurchaseOrderNo.Contains(search, StringComparison.OrdinalIgnoreCase) ||
        x.VendorName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
        x.Description.Contains(search, StringComparison.OrdinalIgnoreCase)));
});

app.MapPost("/api/bridge/purchase-order-requisitions",
    (HttpRequest request) => AcceptRequestAsync(request, "PO-REQ"));

app.MapPost("/api/bridge/payment-requisitions",
    (HttpRequest request) => AcceptRequestAsync(request, "PR-REQ"));

async Task<IResult> AcceptRequestAsync(HttpRequest http, string prefix)
{
    if (!http.HasFormContentType)
        return Results.BadRequest("multipart/form-data required");

    var form = await http.ReadFormAsync();
    var requestJson = form["request"].FirstOrDefault();

    if (string.IsNullOrWhiteSpace(requestJson))
        return Results.BadRequest("request is required");

    using var document = JsonDocument.Parse(requestJson);
    var idempotencyKey = document.RootElement.TryGetProperty("idempotencyKey", out var key)
        ? key.GetString()
        : Guid.NewGuid().ToString("N");

    idempotencyKey ??= Guid.NewGuid().ToString("N");
    var suffix = idempotencyKey[..Math.Min(6, idempotencyKey.Length)].ToUpperInvariant();
    var number = $"{prefix}-{DateTime.UtcNow:yyyyMMdd}-{suffix}";

    var folder = Path.Combine(attachments, number);
    Directory.CreateDirectory(folder);

    foreach (var file in form.Files)
    {
        var safeName = Path.GetFileName(file.FileName);
        await using var output = File.Create(Path.Combine(folder, safeName));
        await file.CopyToAsync(output);
    }

    await File.WriteAllTextAsync(Path.Combine(folder, "request.json"), requestJson);
    return Results.Ok(new { Number = number, Status = "Submitted" });
}

app.Run();

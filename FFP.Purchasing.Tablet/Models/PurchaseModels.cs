namespace FFP.Purchasing.Tablet.Models;

public enum RequestType { PurchaseOrder, PaymentRequisition }
public enum RequestStatus { Draft, PendingSync, Submitted, AwaitingApproval, Returned, Approved, Rejected, Received, AwaitingPayment, Paid, Closed }

public sealed class Vendor
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? TaxNumber { get; init; }
}

public sealed class CatalogItem
{
    public required string Id { get; init; }
    public required string Description { get; init; }
    public string Uom { get; init; } = "EA";
    public decimal LastPrice { get; init; }
}

public sealed class PurchaseLine
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string ItemId { get; set; }
    public required string Description { get; set; }
    public string Uom { get; set; } = "EA";
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Total => Quantity * UnitPrice;
}

public sealed class PurchaseRequest
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string LocalNumber { get; init; } = $"DRAFT-{DateTime.Now:yyyyMMdd-HHmmss}";
    public string? ServerNumber { get; set; }
    public RequestType Type { get; set; } = RequestType.PurchaseOrder;
    public RequestStatus Status { get; set; } = RequestStatus.Draft;
    public string Requestor { get; set; } = "";
    public string Department { get; set; } = "";
    public string? VendorId { get; set; }
    public string? VendorName { get; set; }
    public string Purpose { get; set; } = "";
    public string Notes { get; set; } = "";
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.Now;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
    public List<PurchaseLine> Lines { get; init; } = [];
    public List<AttachmentItem> Attachments { get; init; } = [];
    public decimal Total => Lines.Sum(x => x.Total);
}

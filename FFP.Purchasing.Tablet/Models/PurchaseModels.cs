namespace FFP.Purchasing.Tablet.Models;

public enum RequestType { PurchaseOrder, PaymentRequisition }
public enum RequestStatus { Draft, PendingSync, Submitted, SentForApproval, ReturnedForEdit, Approved, Cancelled, Received, Paid, SyncFailed }
public enum PriorityLevel { None, Low, Medium, High, Urgent, Critical }
public enum PurchaseOrderType { None, Requisition, Regular, Foundation, Superstructure, Sanitation, Delivery, Cancelled, Completed }

public sealed class Vendor
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string PayTo { get; init; } = "";
    public string TaxNumber { get; init; } = "";
    public string Address { get; init; } = "";
    public string ContactPerson { get; init; } = "";
    public string Phone { get; init; } = "";
    public string Email { get; init; } = "";
    public bool Active { get; init; } = true;
}

public sealed class PurchaseOrderReference
{
    public required string Id { get; init; }
    public required string PurchaseOrderNo { get; init; }
    public string RequisitionNo { get; init; } = "";
    public string VendorId { get; init; } = "";
    public string VendorName { get; init; } = "";
    public string Description { get; init; } = "";
    public decimal Total { get; init; }
    public decimal Requested { get; init; }
    public decimal Balance => Math.Max(0, Total - Requested);
}

public sealed class PurchaseLine
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Description { get; set; } = "";
    public string Uom { get; set; } = "EA";
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal Discount { get; set; }
    public decimal PercentDiscount { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
    public decimal Total => Math.Max(0, (Subtotal - Discount - (Subtotal * PercentDiscount)) * (1 + TaxRate));
}

public sealed class PaymentLine
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string PurchaseOrderItemId { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Available { get; set; }
    public decimal PercentRequested { get; set; }
    public decimal AmountRequested { get; set; }
    public decimal Quantity { get; set; }
    public string Comments { get; set; } = "";
}

public sealed class PurchaseRequest
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string IdempotencyKey { get; init; } = Guid.NewGuid().ToString("N");
    public string LocalNumber { get; init; } = $"DRAFT-{DateTime.Now:yyyyMMdd-HHmmss}";
    public string? ServerNumber { get; set; }
    public RequestType Type { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Draft;
    public string Requestor { get; set; } = "";
    public string Department { get; set; } = "";
    public DateTimeOffset DateRequested { get; set; } = DateTimeOffset.Now;
    public string VendorId { get; set; } = "";
    public string VendorName { get; set; } = "";
    public string PayTo { get; set; } = "";
    public string Purpose { get; set; } = "";
    public string Notes { get; set; } = "";
    public PriorityLevel Priority { get; set; } = PriorityLevel.None;

    public string QuoteNo { get; set; } = "";
    public PurchaseOrderType PurchaseOrderType { get; set; } = PurchaseOrderType.Regular;
    public string Terms { get; set; } = "";
    public DateTimeOffset ExpectedDeliveryDate { get; set; } = DateTimeOffset.Now.AddDays(14);
    public decimal Discount { get; set; }
    public decimal PercentDiscount { get; set; }
    public decimal TaxRate { get; set; }
    public string FirstApprover { get; set; } = "";
    public string SecondApprover { get; set; } = "";

    public string PurchaseOrderId { get; set; } = "";
    public string PurchaseOrderNo { get; set; } = "";
    public string InvoiceNo { get; set; } = "";
    public string InvoiceBatchNo { get; set; } = "";
    public string InvoiceEntryNo { get; set; } = "";
    public string InvoiceAccountNo { get; set; } = "";
    public string BatchNo { get; set; } = "";
    public string BatchType { get; set; } = "";
    public string EntryNo { get; set; } = "";
    public string ThirdApprover { get; set; } = "";

    public List<PurchaseLine> Lines { get; init; } = [];
    public List<PaymentLine> PaymentLines { get; init; } = [];
    public List<AttachmentItem> Attachments { get; init; } = [];
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.Now;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
    public string LastSyncError { get; set; } = "";
    public decimal Total => Type == RequestType.PaymentRequisition ? PaymentLines.Sum(x => x.AmountRequested) : Lines.Sum(x => x.Total);
}

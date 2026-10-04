using FFP.Purchasing.Tablet.Models;
namespace FFP.Purchasing.Tablet.Services;
public sealed record ValidationIssue(string Field, string Message);
public interface IRequestValidator { IReadOnlyList<ValidationIssue> Validate(PurchaseRequest request); }
public sealed class RequestValidator : IRequestValidator
{
    public IReadOnlyList<ValidationIssue> Validate(PurchaseRequest r)
    {
        var x = new List<ValidationIssue>();
        if (string.IsNullOrWhiteSpace(r.Requestor)) x.Add(new("Requested By", "Requested By is required."));
        if (string.IsNullOrWhiteSpace(r.Department)) x.Add(new("Department", "Department is required."));
        if (string.IsNullOrWhiteSpace(r.Purpose)) x.Add(new("Purpose", "Purpose / justification is required."));
        if (string.IsNullOrWhiteSpace(r.VendorId)) x.Add(new("Vendor", "Choose a vendor from the cached FFP Manager vendor listing."));
        if (r.Type == RequestType.PurchaseOrder)
        {
            if (r.Lines.Count == 0) x.Add(new("Items", "At least one purchase item is required."));
            if (r.Lines.Any(l => string.IsNullOrWhiteSpace(l.Description) || l.Quantity <= 0 || l.UnitPrice < 0)) x.Add(new("Items", "Each item needs a description, positive quantity and valid price."));
        }
        else
        {
            if (string.IsNullOrWhiteSpace(r.PayTo)) x.Add(new("Pay To", "Pay To is required."));
            if (r.PaymentLines.Count == 0) x.Add(new("Payment Details", "At least one payment detail is required."));
            if (r.PaymentLines.Any(l => l.AmountRequested <= 0)) x.Add(new("Amount Requested", "Payment detail amounts must be greater than zero."));
        }
        return x;
    }
}

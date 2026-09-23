using FFP.Purchasing.Tablet.Models;
namespace FFP.Purchasing.Tablet.Services;
public sealed record ValidationIssue(string Field,string Message);
public interface IRequestValidator{IReadOnlyList<ValidationIssue> Validate(PurchaseRequest request);}
public sealed class RequestValidator:IRequestValidator
{
 public IReadOnlyList<ValidationIssue> Validate(PurchaseRequest r){var x=new List<ValidationIssue>();
 if(string.IsNullOrWhiteSpace(r.Requestor))x.Add(new("Requestor","Requestor is required."));
 if(string.IsNullOrWhiteSpace(r.Department))x.Add(new("Department","Department is required."));
 if(string.IsNullOrWhiteSpace(r.Purpose))x.Add(new("Purpose","Purpose/justification is required."));
 if(r.Type==RequestType.PurchaseOrder&&string.IsNullOrWhiteSpace(r.VendorName))x.Add(new("Vendor","Vendor is required for a purchase order."));
 if(r.Type==RequestType.PurchaseOrder&&r.Lines.Count==0)x.Add(new("Lines","At least one purchase line is required."));
 if(r.Lines.Any(l=>l.Quantity<=0))x.Add(new("Quantity","Line quantities must be greater than zero."));
 if(r.Lines.Any(l=>l.UnitPrice<0))x.Add(new("Price","Unit prices cannot be negative."));
 return x;}
}

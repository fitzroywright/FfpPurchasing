using FFP.Purchasing.Tablet.Models;
using FFP.Purchasing.Tablet.Services;

namespace FFP.Purchasing.Tablet.Pages;

// Kept as a compatibility entry point for older navigation paths.
// The actual presentation is the shared tablet requisition editor.
public sealed class NewRequestPage : TabletPage
{
    private readonly IPurchaseStore _store;
    private readonly IReferenceDataService _reference;

    public NewRequestPage(IPurchaseStore store, IReferenceDataService reference)
        : base("New Request")
    {
        _store = store;
        _reference = reference;

        var po = Ui.Primary("Purchase Order Requisition");
        po.Clicked += async (_, _) => await CreateAsync(RequestType.PurchaseOrder);

        var payment = Ui.Secondary("Payment Requisition");
        payment.Clicked += async (_, _) => await CreateAsync(RequestType.PaymentRequisition);

        Body.Children.Add(Ui.Hint("Choose the requisition type."));
        Body.Children.Add(po);
        Body.Children.Add(payment);
    }

    private async Task CreateAsync(RequestType type)
    {
        var request = new PurchaseRequest
        {
            Type = type,
            Requestor = "Maria Santos",
            Department = "Operations"
        };

        await _store.SaveAsync(request);
        var s = Handler!.MauiContext!.Services;
        await Navigation.PushAsync(new RequestEditorPage(
            request,
            _store,
            s.GetRequiredService<ILocalItemCache>(),
            s.GetRequiredService<IAttachmentService>(),
            s.GetRequiredService<ISyncService>(),
            s.GetRequiredService<IRequestValidator>(),
            s.GetRequiredService<IAuditService>(),
            _reference));
    }
}
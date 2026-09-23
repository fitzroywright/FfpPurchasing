using FFP.Purchasing.Tablet.Models;
using FFP.Purchasing.Tablet.Services;

namespace FFP.Purchasing.Tablet.Pages;

public sealed class NewRequestPage : ContentPage
{
    private readonly IPurchaseStore _store;
    private readonly IReferenceDataService _reference;
    private readonly Entry _requestor = new() { Placeholder = "Requestor" };
    private readonly Entry _department = new() { Placeholder = "Department" };
    private readonly Picker _type = new() { Title = "Request type", ItemsSource = new[] { "Purchase Order", "Payment Requisition" } };
    private readonly Picker _vendor = new() { Title = "Vendor" };
    private readonly Entry _purpose = new() { Placeholder = "Purpose / justification" };
    private IReadOnlyList<Vendor> _vendors = [];

    public NewRequestPage(IPurchaseStore store, IReferenceDataService reference)
    {
        _store=store; _reference=reference; Title="New Request"; _type.SelectedIndex=0;
        var next=new Button { Text="Create & Add Items" };
        next.Clicked += Save;
        Content=new ScrollView { Content=new VerticalStackLayout { Padding=24, Spacing=12,
            Children={ new Label { Text="New Purchase Request", FontSize=28, FontAttributes=FontAttributes.Bold },
                _type,_requestor,_department,_vendor,_purpose,next }}};
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing(); await _reference.RefreshAsync(); _vendors=await _reference.GetVendorsAsync();
        _vendor.ItemsSource=_vendors.Select(x=>x.Name).ToList();
    }
    private async void Save(object? s, EventArgs e)
    {
        var v=_vendor.SelectedIndex>=0 && _vendor.SelectedIndex<_vendors.Count ? _vendors[_vendor.SelectedIndex] : null;
        var r=new PurchaseRequest { Type=_type.SelectedIndex==1?RequestType.PaymentRequisition:RequestType.PurchaseOrder,
            Requestor=_requestor.Text??"", Department=_department.Text??"", VendorId=v?.Id, VendorName=v?.Name, Purpose=_purpose.Text??"" };
        await _store.SaveAsync(r);
        await Navigation.PushAsync(new RequestEditorPage(r,_store,_reference,
            Handler!.MauiContext!.Services.GetRequiredService<IAttachmentService>(),
            Handler!.MauiContext!.Services.GetRequiredService<ISyncService>()));
    }
}

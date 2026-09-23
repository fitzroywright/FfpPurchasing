using FFP.Purchasing.Tablet.Services;

namespace FFP.Purchasing.Tablet.Pages;

public sealed class RequestsPage : ContentPage
{
    private readonly IPurchaseStore _store; private readonly IReferenceDataService _reference; private readonly IAttachmentService _attachments; private readonly ISyncService _sync;
    private readonly VerticalStackLayout _list=new(){Spacing=10};
    public RequestsPage(IPurchaseStore store,IReferenceDataService reference,IAttachmentService attachments,ISyncService sync)
    { _store=store;_reference=reference;_attachments=attachments;_sync=sync;Title="My Requests";Content=new ScrollView{Content=new VerticalStackLayout{Padding=24,Children={new Label{Text="My Requests",FontSize=28,FontAttributes=FontAttributes.Bold},_list}}};}
    protected override async void OnAppearing(){base.OnAppearing();await Load();}
    private async Task Load(){_list.Children.Clear();foreach(var r in (await _store.GetAllAsync()).OrderByDescending(x=>x.UpdatedAt)){var b=new Button{Text=$"{r.LocalNumber} • {r.VendorName ?? "No vendor"} • {r.Total:C} • {r.Status}"};b.Clicked+=async(_,__)=>await Navigation.PushAsync(new RequestEditorPage(r,_store,_reference,_attachments,_sync));_list.Children.Add(b);}}
}

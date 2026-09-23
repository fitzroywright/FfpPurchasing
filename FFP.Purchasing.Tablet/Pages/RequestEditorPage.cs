using FFP.Purchasing.Tablet.Models;
using FFP.Purchasing.Tablet.Services;

namespace FFP.Purchasing.Tablet.Pages;

public sealed class RequestEditorPage : ContentPage
{
    private readonly PurchaseRequest _r; private readonly IPurchaseStore _store; private readonly IReferenceDataService _ref;
    private readonly IAttachmentService _attachments; private readonly ISyncService _sync;
    private readonly VerticalStackLayout _lines=new() { Spacing=8 }; private readonly VerticalStackLayout _docs=new() { Spacing=8 };
    private readonly Label _total=new() { FontSize=20, FontAttributes=FontAttributes.Bold };

    public RequestEditorPage(PurchaseRequest r, IPurchaseStore store, IReferenceDataService reference, IAttachmentService attachments, ISyncService sync)
    {
        _r=r;_store=store;_ref=reference;_attachments=attachments;_sync=sync; Title=r.LocalNumber;
        var add=new Button{Text="Add Item"}; add.Clicked+=AddItem;
        var photo=new Button{Text="Take Photo"}; photo.Clicked+=Photo;
        var file=new Button{Text="Choose Files"}; file.Clicked+=Files;
        var submit=new Button{Text="Submit Request",BackgroundColor=Color.FromArgb("#0B4F8A"),TextColor=Colors.White}; submit.Clicked+=Submit;
        Content=new ScrollView{Content=new VerticalStackLayout{Padding=24,Spacing=12,Children={
            new Label{Text=$"{r.Type} • {r.VendorName ?? "No vendor"}",FontSize=24,FontAttributes=FontAttributes.Bold},
            new Label{Text=r.Purpose}, new Label{Text="Items",FontAttributes=FontAttributes.Bold},_lines,add,_total,
            new Label{Text="Supporting Documents",FontAttributes=FontAttributes.Bold},
            new HorizontalStackLayout{Children={photo,file}},_docs,submit}}};
        Render();
    }
    private async void AddItem(object? s,EventArgs e)
    {
        var items=await _ref.GetItemsAsync();
        var description=await DisplayPromptAsync("Item","Enter item description",initialValue:items.FirstOrDefault()?.Description);
        if(string.IsNullOrWhiteSpace(description))return;
        var qtyText=await DisplayPromptAsync("Quantity","Quantity",initialValue:"1",keyboard:Keyboard.Numeric);
        var priceText=await DisplayPromptAsync("Unit Price","Unit price",initialValue:"0",keyboard:Keyboard.Numeric);
        decimal.TryParse(qtyText,out var qty); decimal.TryParse(priceText,out var price);
        var match=items.FirstOrDefault(x=>x.Description.Equals(description,StringComparison.OrdinalIgnoreCase));
        _r.Lines.Add(new PurchaseLine{ItemId=match?.Id??"MANUAL",Description=description,Uom=match?.Uom??"EA",Quantity=qty<=0?1:qty,UnitPrice=price});
        await _store.SaveAsync(_r); Render();
    }
    private async void Photo(object? s,EventArgs e){var a=await _attachments.CapturePhotoAsync();if(a!=null){_r.Attachments.Add(a);await _store.SaveAsync(_r);Render();}}
    private async void Files(object? s,EventArgs e){_r.Attachments.AddRange(await _attachments.PickFilesAsync());await _store.SaveAsync(_r);Render();}
    private async void Submit(object? s,EventArgs e)
    {
        if(string.IsNullOrWhiteSpace(_r.Requestor)||string.IsNullOrWhiteSpace(_r.Department)){await DisplayAlert("Required","Requestor and department are required.","OK");return;}
        if(_r.Type==RequestType.PurchaseOrder && _r.Lines.Count==0){await DisplayAlert("Required","A purchase order needs at least one line.","OK");return;}
        var result=await _sync.SubmitAsync(_r); await DisplayAlert(result.Succeeded?"Submitted":"Pending",result.Message,"OK"); Render();
    }
    private void Render()
    {
        _lines.Children.Clear(); foreach(var l in _r.Lines)_lines.Children.Add(new Label{Text=$"{l.Quantity:0.##} {l.Uom}  {l.Description}  @ {l.UnitPrice:C} = {l.Total:C}"});
        _docs.Children.Clear(); foreach(var a in _r.Attachments)_docs.Children.Add(new Label{Text=$"{a.FileName} • {(a.PendingSync?"Pending Sync":"Synced")}"});
        _total.Text=$"Total: {_r.Total:C} • {_r.Status}";
    }
}

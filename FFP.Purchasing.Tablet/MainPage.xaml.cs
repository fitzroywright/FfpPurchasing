using FFP.Purchasing.Tablet.Models;
using FFP.Purchasing.Tablet.Pages;
using FFP.Purchasing.Tablet.Services;

namespace FFP.Purchasing.Tablet;

public partial class MainPage : ContentPage
{
    private readonly IAttachmentService _attachments;
    private readonly IPurchaseStore _store;
    private readonly IReferenceDataService _reference;
    private readonly ISyncService _sync;
    private readonly IAppDiagnostics _diagnostics;
    private readonly List<AttachmentItem> _items = [];

    public MainPage(IAttachmentService attachments, IPurchaseStore store, IReferenceDataService reference, ISyncService sync, IAppDiagnostics diagnostics)
    {
        InitializeComponent(); _attachments=attachments; _store=store; _reference=reference; _sync=sync; _diagnostics=diagnostics;
        Title="FFP Purchasing";
        ToolbarItems.Add(new ToolbarItem("New",null,async()=>await Navigation.PushAsync(new NewRequestPage(_store,_reference))));
        ToolbarItems.Add(new ToolbarItem("Requests",null,async()=>await Navigation.PushAsync(new RequestsPage(_store,_reference,_attachments,_sync))));
        ToolbarItems.Add(new ToolbarItem("Diagnostics",null,async()=>await Navigation.PushAsync(new DiagnosticsPage(_diagnostics))));
        ToolbarItems.Add(new ToolbarItem("Settings",null,async()=>await Navigation.PushAsync(new SettingsPage())));
    }
    protected override async void OnAppearing(){base.OnAppearing();await ImportSharedInboxAsync();}
    private async void TakePhoto_Clicked(object sender,EventArgs e){try{var item=await _attachments.CapturePhotoAsync();if(item!=null){_items.Add(item);RenderAttachments();}}catch(Exception ex){await DisplayAlert("Attachment error",ex.Message,"OK");}}
    private async void ChooseFiles_Clicked(object sender,EventArgs e){try{_items.AddRange(await _attachments.PickFilesAsync());RenderAttachments();}catch(Exception ex){await DisplayAlert("Attachment error",ex.Message,"OK");}}
    private async void ImportShared_Clicked(object sender,EventArgs e){await ImportSharedInboxAsync();}
    private async Task ImportSharedInboxAsync(){foreach(var path in SharedAttachmentInbox.Drain()){try{_items.Add(await _attachments.ImportFileAsync(path,"Outlook / Share"));}catch{}}RenderAttachments();}
    private void RenderAttachments(){AttachmentList.Children.Clear();EmptyLabel.IsVisible=_items.Count==0;foreach(var item in _items){AttachmentList.Children.Add(new Border{Stroke=Color.FromArgb("#D9E2EC"),Padding=10,Content=new Label{Text=$"{item.FileName} • {item.Source} • Pending Sync"}});}}
}

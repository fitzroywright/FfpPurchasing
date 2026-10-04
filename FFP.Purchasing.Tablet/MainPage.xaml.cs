using FFP.Purchasing.Tablet.Models;
using FFP.Purchasing.Tablet.Pages;
using FFP.Purchasing.Tablet.Services;

namespace FFP.Purchasing.Tablet;

public sealed class MainPage : TabletPage
{
    private readonly IPurchaseStore _store;
    private readonly IReferenceDataService _reference;
    private readonly IPendingSyncWorker _worker;
    private readonly VerticalStackLayout _recent = new() { Spacing = 8 };

    public MainPage(IPurchaseStore store, IReferenceDataService reference, IPendingSyncWorker worker)
        : base("Welcome")
    {
        _store = store;
        _reference = reference;
        _worker = worker;

        Body.Add(new Label
        {
            Text = "Create and track Purchase Order and Payment Requisitions",
            TextColor = Ui.Muted
        });

        var choices = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection(
                new(GridLength.Star), new(GridLength.Star)),
            ColumnSpacing = 12
        };
        choices.Add(RequestCard("🛒", "Purchase Order Requisition",
            "Request goods or services from an approved vendor.", RequestType.PurchaseOrder), 0);
        choices.Add(RequestCard("▣", "Payment Requisition",
            "Request payment to a vendor or other payee.", RequestType.PaymentRequisition), 1);

        Body.Add(choices);
        Body.Add(Ui.H2("My Requests"));
        Body.Add(_recent);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _ = _worker.RunOnceAsync();

        if ((await _reference.GetVendorCacheInfoAsync()).Count == 0)
            _ = _reference.RefreshVendorsAsync();

        await RenderRecentAsync();
    }

    private Border RequestCard(string icon, string title, string subtitle, RequestType type)
    {
        var button = Ui.Primary(type == RequestType.PurchaseOrder
            ? "Create PO Requisition"
            : "Create Payment Requisition");
        button.Clicked += async (_, _) => await CreateAsync(type);

        return Ui.Card(new VerticalStackLayout
        {
            Spacing = 10,
            Children =
            {
                new Label { Text = icon, FontSize = 36, TextColor = Ui.Blue },
                Ui.H2(title),
                Ui.Hint(subtitle),
                button
            }
        });
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

    private async Task RenderRecentAsync()
    {
        _recent.Clear();
        foreach (var r in (await _store.GetAllAsync())
                     .OrderByDescending(x => x.UpdatedAt).Take(5))
        {
            var row = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection(
                    new(GridLength.Star), new(GridLength.Auto)),
                Padding = 10
            };

            row.Add(new VerticalStackLayout
            {
                Children =
                {
                    new Label
                    {
                        Text = r.ServerNumber ?? r.LocalNumber,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Ui.Blue
                    },
                    new Label
                    {
                        Text = $"{r.Type} • {r.VendorName} • {r.Total:C2}",
                        FontSize = 12,
                        TextColor = Ui.Muted
                    }
                }
            }, 0);

            row.Add(new Label
            {
                Text = r.Status.ToString(),
                TextColor = r.Status == RequestStatus.PendingSync ? Colors.DarkOrange : Ui.Ink,
                VerticalTextAlignment = TextAlignment.Center
            }, 1);

            _recent.Add(Ui.Card(row, new Thickness(8)));
        }
    }
}
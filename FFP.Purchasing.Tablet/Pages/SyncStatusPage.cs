using FFP.Purchasing.Tablet.Models;
using FFP.Purchasing.Tablet.Services;

namespace FFP.Purchasing.Tablet.Pages;

public sealed class SyncStatusPage : TabletPage
{
    private readonly IPurchaseStore _store;
    private readonly IPendingSyncWorker _worker;
    private readonly IReferenceDataService _reference;
    private readonly ISyncService _sync;
    private readonly VerticalStackLayout _box = new() { Spacing = 10 };

    public SyncStatusPage(
        IPurchaseStore store,
        IPendingSyncWorker worker,
        IReferenceDataService reference,
        ISyncService sync)
        : base("Sync Status")
    {
        _store = store;
        _worker = worker;
        _reference = reference;
        _sync = sync;

        var now = Ui.Primary("Sync Now");
        now.Clicked += async (_, _) =>
        {
            await _worker.RunOnceAsync();
            await Load();
        };

        Body.Children.Add(_box);
        Body.Children.Add(now);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Load();
    }

    private async Task Load()
    {
        _box.Children.Clear();
        var meta = await _reference.GetVendorCacheInfoAsync();
        var all = await _store.GetAllAsync();
        var reachable = await _sync.IsBridgeReachableAsync();

        _box.Children.Add(Ui.Card(new Label
        {
            Text =
                $"FFP Manager Bridge: {(reachable ? "Reachable" : "Not reachable")}
" +
                $"Vendor cache: {meta.Count} vendors
" +
                $"Last vendor refresh: {(meta.LastRefreshed?.LocalDateTime.ToString("g") ?? "Never")}
" +
                $"Queued requests: {all.Count(x => x.Status is RequestStatus.PendingSync or RequestStatus.SyncFailed)}",
            TextColor = Ui.Ink
        }));
    }
}
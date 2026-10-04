using FFP.Purchasing.Tablet.Models;
namespace FFP.Purchasing.Tablet.Services;
public interface IPendingSyncWorker { Task<int> RunOnceAsync(CancellationToken cancellationToken = default); }
public sealed class PendingSyncWorker(IPurchaseStore store, ISyncService sync, IReferenceDataService reference) : IPendingSyncWorker
{
    public async Task<int> RunOnceAsync(CancellationToken cancellationToken = default)
    {
        if (!await sync.IsBridgeReachableAsync(cancellationToken)) return 0;
        await reference.RefreshVendorsAsync(cancellationToken);
        var count = 0;
        foreach (var r in (await store.GetAllAsync()).Where(x => x.Status is RequestStatus.PendingSync or RequestStatus.SyncFailed))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if ((await sync.SubmitAsync(r, cancellationToken)).Succeeded) count++;
        }
        return count;
    }
}

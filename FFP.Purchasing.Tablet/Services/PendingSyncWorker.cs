using FFP.Purchasing.Tablet.Models;
namespace FFP.Purchasing.Tablet.Services;
public interface IPendingSyncWorker{Task<int> RunOnceAsync(CancellationToken cancellationToken=default);}
public sealed class PendingSyncWorker(IPurchaseStore store,ISyncService sync):IPendingSyncWorker
{
 public async Task<int> RunOnceAsync(CancellationToken cancellationToken=default)
 {
  if(Connectivity.Current.NetworkAccess!=NetworkAccess.Internet)return 0;
  var count=0;
  foreach(var r in (await store.GetAllAsync()).Where(x=>x.Status==RequestStatus.PendingSync))
  { cancellationToken.ThrowIfCancellationRequested();var result=await sync.SubmitAsync(r,cancellationToken);if(result.Succeeded)count++; }
  return count;
 }
}

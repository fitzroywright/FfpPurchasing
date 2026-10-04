using System.Net.Http.Json;
using FFP.Purchasing.Tablet.Models;

namespace FFP.Purchasing.Tablet.Services;

public interface ISyncService
{
    Task<SyncResult> SubmitAsync(PurchaseRequest request, CancellationToken cancellationToken = default);
    Task<bool> IsBridgeReachableAsync(CancellationToken cancellationToken = default);
}

public sealed record SyncResult(bool Succeeded, string Message, string? ServerNumber = null);

public sealed class SyncService(HttpClient http, IPurchaseStore store) : ISyncService
{
    public async Task<bool> IsBridgeReachableAsync(CancellationToken cancellationToken = default)
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.None) return false;
        try
        {
            using var response = await http.GetAsync("health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<SyncResult> SubmitAsync(PurchaseRequest request, CancellationToken cancellationToken = default)
    {
        request.Status = RequestStatus.PendingSync;
        request.LastSyncError = "";
        await store.SaveAsync(request);

        if (!await IsBridgeReachableAsync(cancellationToken))
            return new(false, "Saved on this tablet. FFP Manager Bridge is not reachable; the request remains Pending Sync.");

        try
        {
            using var form = new MultipartFormDataContent();
            form.Add(JsonContent.Create(request), "request");

            foreach (var attachment in request.Attachments.Where(x => File.Exists(x.LocalPath)))
            {
                var part = new StreamContent(File.OpenRead(attachment.LocalPath));
                part.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(attachment.ContentType);
                form.Add(part, "attachments", attachment.FileName);
            }

            var endpoint = request.Type == RequestType.PurchaseOrder
                ? "api/bridge/purchase-order-requisitions"
                : "api/bridge/payment-requisitions";

            var response = await http.PostAsync(endpoint, form, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                request.LastSyncError = $"Bridge returned HTTP {(int)response.StatusCode}.";
                request.Status = RequestStatus.SyncFailed;
                await store.SaveAsync(request);
                return new(false, request.LastSyncError);
            }

            var ack = await response.Content.ReadFromJsonAsync<SubmitAck>(cancellationToken: cancellationToken);
            request.ServerNumber = ack?.Number;
            request.Status = RequestStatus.Submitted;
            request.LastSyncError = "";
            foreach (var a in request.Attachments) a.PendingSync = false;
            await store.SaveAsync(request);

            return new(true, "Submitted to FFP Manager Bridge.", request.ServerNumber);
        }
        catch (Exception ex)
        {
            request.LastSyncError = ex.Message;
            request.Status = RequestStatus.PendingSync;
            await store.SaveAsync(request);
            return new(false, "Sync failed. The local copy is safe and will retry when the Bridge is reachable.");
        }
    }

    private sealed record SubmitAck(string Number);
}
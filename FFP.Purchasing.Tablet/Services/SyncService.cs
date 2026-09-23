using System.Net.Http.Json;
using FFP.Purchasing.Tablet.Models;

namespace FFP.Purchasing.Tablet.Services;

public interface ISyncService
{
    Task<SyncResult> SubmitAsync(PurchaseRequest request, CancellationToken cancellationToken = default);
}
public sealed record SyncResult(bool Succeeded, string Message, string? ServerNumber = null);

public sealed class SyncService(HttpClient http, IPurchaseStore store) : ISyncService
{
    public async Task<SyncResult> SubmitAsync(PurchaseRequest request, CancellationToken cancellationToken = default)
    {
        request.Status = RequestStatus.PendingSync;
        await store.SaveAsync(request);
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            return new(false, "Saved offline. It will remain Pending Sync.");

        try
        {
            using var form = new MultipartFormDataContent();
            form.Add(JsonContent.Create(request), "request");
            foreach (var attachment in request.Attachments.Where(x => File.Exists(x.LocalPath)))
            {
                var stream = File.OpenRead(attachment.LocalPath);
                var part = new StreamContent(stream);
                part.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(attachment.ContentType);
                form.Add(part, "attachments", attachment.FileName);
            }

            var response = await http.PostAsync("api/purchasing/requests", form, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return new(false, $"Server returned {(int)response.StatusCode}. Request remains Pending Sync.");

            var ack = await response.Content.ReadFromJsonAsync<SubmitAck>(cancellationToken: cancellationToken);
            request.ServerNumber = ack?.Number;
            request.Status = RequestStatus.Submitted;
            foreach (var a in request.Attachments) a.PendingSync = false;
            await store.SaveAsync(request);
            return new(true, "Submitted successfully.", request.ServerNumber);
        }
        catch (Exception ex)
        {
            return new(false, $"Submission failed: {ex.Message}. Request remains Pending Sync.");
        }
    }
    private sealed record SubmitAck(string Number);
}

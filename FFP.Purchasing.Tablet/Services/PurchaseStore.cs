using System.Text.Json;
using FFP.Purchasing.Tablet.Models;

namespace FFP.Purchasing.Tablet.Services;

public interface IPurchaseStore
{
    Task<IReadOnlyList<PurchaseRequest>> GetAllAsync();
    Task<PurchaseRequest?> GetAsync(Guid id);
    Task SaveAsync(PurchaseRequest request);
}

public sealed class PurchaseStore : IPurchaseStore
{
    private readonly string _path = Path.Combine(FileSystem.AppDataDirectory, "purchase-requests.json");
    private readonly SemaphoreSlim _gate = new(1, 1);
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public async Task<IReadOnlyList<PurchaseRequest>> GetAllAsync()
    {
        await _gate.WaitAsync();
        try { return await ReadUnsafeAsync(); }
        finally { _gate.Release(); }
    }

    public async Task<PurchaseRequest?> GetAsync(Guid id) => (await GetAllAsync()).FirstOrDefault(x => x.Id == id);

    public async Task SaveAsync(PurchaseRequest request)
    {
        await _gate.WaitAsync();
        try
        {
            var all = await ReadUnsafeAsync();
            var existing = all.FindIndex(x => x.Id == request.Id);
            request.UpdatedAt = DateTimeOffset.Now;
            if (existing >= 0) all[existing] = request; else all.Add(request);
            await File.WriteAllTextAsync(_path, JsonSerializer.Serialize(all, Json));
        }
        finally { _gate.Release(); }
    }

    private async Task<List<PurchaseRequest>> ReadUnsafeAsync()
    {
        if (!File.Exists(_path)) return [];
        try { return JsonSerializer.Deserialize<List<PurchaseRequest>>(await File.ReadAllTextAsync(_path), Json) ?? []; }
        catch { return []; }
    }
}

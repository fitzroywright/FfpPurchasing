using System.Text.Json;
using FFP.Purchasing.Tablet.Models;

namespace FFP.Purchasing.Tablet.Services;

public interface IReferenceDataService
{
    Task<IReadOnlyList<Vendor>> GetVendorsAsync();
    Task<IReadOnlyList<CatalogItem>> GetItemsAsync();
    Task RefreshAsync(CancellationToken cancellationToken = default);
}

public sealed class ReferenceDataService(HttpClient http) : IReferenceDataService
{
    private readonly string _vendors = Path.Combine(FileSystem.AppDataDirectory, "vendors.json");
    private readonly string _items = Path.Combine(FileSystem.AppDataDirectory, "items.json");
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<Vendor>> GetVendorsAsync() => await ReadAsync<Vendor>(_vendors);
    public async Task<IReadOnlyList<CatalogItem>> GetItemsAsync() => await ReadAsync<CatalogItem>(_items);

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet) return;
        try
        {
            var vendors = await http.GetFromJsonAsync<List<Vendor>>("api/reference/vendors", cancellationToken);
            var items = await http.GetFromJsonAsync<List<CatalogItem>>("api/reference/items", cancellationToken);
            if (vendors is not null) await File.WriteAllTextAsync(_vendors, JsonSerializer.Serialize(vendors, Json), cancellationToken);
            if (items is not null) await File.WriteAllTextAsync(_items, JsonSerializer.Serialize(items, Json), cancellationToken);
        }
        catch { /* cached reference data remains authoritative while offline/unavailable */ }
    }

    private static async Task<IReadOnlyList<T>> ReadAsync<T>(string path)
    {
        if (!File.Exists(path)) return [];
        try { return JsonSerializer.Deserialize<List<T>>(await File.ReadAllTextAsync(path), Json) ?? []; }
        catch { return []; }
    }
}

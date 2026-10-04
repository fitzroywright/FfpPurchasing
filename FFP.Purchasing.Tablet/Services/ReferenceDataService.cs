using System.Net.Http.Json;
using System.Text.Json;
using FFP.Purchasing.Tablet.Models;

namespace FFP.Purchasing.Tablet.Services;

public sealed record VendorCacheInfo(DateTimeOffset? LastRefreshed, int Count, string LastResult);

public interface IReferenceDataService
{
    Task<IReadOnlyList<Vendor>> GetVendorsAsync(string? search = null);
    Task<IReadOnlyList<PurchaseOrderReference>> SearchPurchaseOrdersAsync(string? search = null, CancellationToken cancellationToken = default);
    Task<VendorCacheInfo> GetVendorCacheInfoAsync();
    Task<bool> RefreshVendorsAsync(CancellationToken cancellationToken = default);
}

public sealed class ReferenceDataService(HttpClient http) : IReferenceDataService
{
    private readonly string _vendors = Path.Combine(FileSystem.AppDataDirectory, "vendors.json");
    private readonly string _meta = Path.Combine(FileSystem.AppDataDirectory, "vendors.meta.json");
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public async Task<IReadOnlyList<Vendor>> GetVendorsAsync(string? search = null)
    {
        var all = await ReadVendorsAsync();
        if (string.IsNullOrWhiteSpace(search))
            return all.Where(x => x.Active).OrderBy(x => x.Name).ToList();

        var q = search.Trim();
        return all.Where(x => x.Active &&
            (x.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
             x.Id.Contains(q, StringComparison.OrdinalIgnoreCase) ||
             x.TaxNumber.Contains(q, StringComparison.OrdinalIgnoreCase) ||
             x.PayTo.Contains(q, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(x => x.Name).ToList();
    }

    public async Task<bool> RefreshVendorsAsync(CancellationToken cancellationToken = default)
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.None) return false;
        try
        {
            using var response = await http.GetAsync("api/bridge/vendors", cancellationToken);
            if (!response.IsSuccessStatusCode) return false;
            var vendors = await response.Content.ReadFromJsonAsync<List<Vendor>>(cancellationToken: cancellationToken) ?? [];
            await File.WriteAllTextAsync(_vendors, JsonSerializer.Serialize(vendors, Json), cancellationToken);
            await File.WriteAllTextAsync(_meta,
                JsonSerializer.Serialize(new VendorCacheInfo(DateTimeOffset.Now, vendors.Count, "Refreshed"), Json),
                cancellationToken);
            return true;
        }
        catch { return false; }
    }

    public async Task<VendorCacheInfo> GetVendorCacheInfoAsync()
    {
        if (!File.Exists(_meta))
            return new(null, (await ReadVendorsAsync()).Count, "Never refreshed");
        try
        {
            return JsonSerializer.Deserialize<VendorCacheInfo>(await File.ReadAllTextAsync(_meta), Json)
                ?? new(null, 0, "Unknown");
        }
        catch { return new(null, (await ReadVendorsAsync()).Count, "Metadata unavailable"); }
    }

    public async Task<IReadOnlyList<PurchaseOrderReference>> SearchPurchaseOrdersAsync(
        string? search = null, CancellationToken cancellationToken = default)
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.None) return [];
        try
        {
            var url = "api/bridge/purchase-orders" +
                (string.IsNullOrWhiteSpace(search) ? "" : $"?search={Uri.EscapeDataString(search)}");
            return await http.GetFromJsonAsync<List<PurchaseOrderReference>>(url, cancellationToken) ?? [];
        }
        catch { return []; }
    }

    private async Task<List<Vendor>> ReadVendorsAsync()
    {
        if (!File.Exists(_vendors)) return [];
        try
        {
            return JsonSerializer.Deserialize<List<Vendor>>(await File.ReadAllTextAsync(_vendors), Json) ?? [];
        }
        catch { return []; }
    }
}
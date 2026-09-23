using System.Text.Json; using FFP.Purchasing.Tablet.Models;
namespace FFP.Purchasing.Tablet.Services;
public interface IReferenceDataService { Task<IReadOnlyList<Vendor>> GetVendorsAsync(); Task RefreshVendorsAsync(CancellationToken cancellationToken=default); }
public sealed class ReferenceDataService(HttpClient http):IReferenceDataService {
 private readonly string _vendors=Path.Combine(FileSystem.AppDataDirectory,"vendors.json"); private static readonly JsonSerializerOptions Json=new(JsonSerializerDefaults.Web);
 public async Task<IReadOnlyList<Vendor>> GetVendorsAsync()=>await ReadAsync();
 public async Task RefreshVendorsAsync(CancellationToken cancellationToken=default){if(Connectivity.Current.NetworkAccess!=NetworkAccess.Internet)return;try{var vendors=await http.GetFromJsonAsync<List<Vendor>>("api/mock-ffp-manager/vendors",cancellationToken);if(vendors is not null)await File.WriteAllTextAsync(_vendors,JsonSerializer.Serialize(vendors,Json),cancellationToken);}catch{}}
 private async Task<IReadOnlyList<Vendor>> ReadAsync(){if(!File.Exists(_vendors))return[];try{return JsonSerializer.Deserialize<List<Vendor>>(await File.ReadAllTextAsync(_vendors),Json)??[];}catch{return[];}}
}
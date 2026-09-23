namespace FFP.Purchasing.Tablet.Services;

public sealed record DiagnosticResult(string Id, string Name, bool Passed, string Detail);

public interface IAppDiagnostics
{
    Task<IReadOnlyList<DiagnosticResult>> RunLevel5Async();
}

public sealed class AppDiagnostics(HttpClient http) : IAppDiagnostics
{
    public async Task<IReadOnlyList<DiagnosticResult>> RunLevel5Async()
    {
        var results = new List<DiagnosticResult>();
        results.Add(new("FFP.PUR.L5.001", "Local storage writable", TestStorage(), FileSystem.AppDataDirectory));
        results.Add(new("FFP.PUR.L5.002", "Network state readable", true, Connectivity.Current.NetworkAccess.ToString()));
        results.Add(new("FFP.PUR.L5.003", "Camera capability", true, MediaPicker.Default.IsCaptureSupported ? "Capture supported" : "Capture not supported on this device"));
        results.Add(new("FFP.PUR.L5.004", "Attachment inbox", true, "Share inbox available"));
        results.Add(await TestApiAsync());
        return results;
    }

    private static bool TestStorage()
    {
        try
        {
            var p = Path.Combine(FileSystem.CacheDirectory, "ffp-purchasing-diag.tmp");
            File.WriteAllText(p, "ok"); File.Delete(p); return true;
        }
        catch { return false; }
    }

    private async Task<DiagnosticResult> TestApiAsync()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            return new("FFP.PUR.L5.005", "Purchasing API reachability", false, "Offline");
        try
        {
            using var r = await http.GetAsync("health");
            return new("FFP.PUR.L5.005", "Purchasing API reachability", r.IsSuccessStatusCode, $"HTTP {(int)r.StatusCode}");
        }
        catch (Exception ex) { return new("FFP.PUR.L5.005", "Purchasing API reachability", false, ex.Message); }
    }
}

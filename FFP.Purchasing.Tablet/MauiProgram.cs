using Microsoft.Extensions.Logging;
using FFP.Purchasing.Tablet.Services;

namespace FFP.Purchasing.Tablet;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
#if DEBUG
        builder.Logging.AddDebug();
#endif
        var apiBase = Preferences.Default.Get("PurchasingApiBaseUrl", "https://purchasing.ffpja.org/");
        builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(apiBase) });
        builder.Services.AddSingleton<IAttachmentService, AttachmentService>();
        builder.Services.AddSingleton<IPurchaseStore, PurchaseStore>();
        builder.Services.AddSingleton<IReferenceDataService, ReferenceDataService>();
        builder.Services.AddSingleton<ILocalItemCache, LocalItemCache>();
        builder.Services.AddSingleton<ISyncService, SyncService>();
        builder.Services.AddSingleton<IAppDiagnostics, AppDiagnostics>();
        builder.Services.AddSingleton<IRequestValidator, RequestValidator>();
        builder.Services.AddSingleton<IAuditService, AuditService>();
        builder.Services.AddSingleton<IPendingSyncWorker, PendingSyncWorker>();
        builder.Services.AddSingleton<MainPage>();
        return builder.Build();
    }
}

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using FFP.Purchasing.Tablet.Services;

namespace FFP.Purchasing.Tablet;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
                           ConfigChanges.UiMode | ConfigChanges.ScreenLayout |
                           ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
[IntentFilter(
    [Intent.ActionSend],
    Categories = [Intent.CategoryDefault],
    DataMimeType = "*/*")]
[IntentFilter(
    [Intent.ActionSendMultiple],
    Categories = [Intent.CategoryDefault],
    DataMimeType = "*/*")]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        CaptureSharedFiles(Intent);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        if (intent is not null)
        {
            Intent = intent;
            CaptureSharedFiles(intent);
        }
    }

    private void CaptureSharedFiles(Intent intent)
    {
        if (intent.Action == Intent.ActionSend)
        {
            var uri = GetParcelableExtraCompat(intent, Intent.ExtraStream);
            if (uri is not null)
                CopySharedUri(uri);
        }
        else if (intent.Action == Intent.ActionSendMultiple)
        {
#pragma warning disable CA1422
            var uris = Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu
                ? intent.GetParcelableArrayListExtra(Intent.ExtraStream, Java.Lang.Class.FromType(typeof(Android.Net.Uri)))
                : intent.GetParcelableArrayListExtra(Intent.ExtraStream);
#pragma warning restore CA1422
            if (uris is not null)
                foreach (var value in uris)
                    if (value is Android.Net.Uri uri)
                        CopySharedUri(uri);
        }
    }

    private Android.Net.Uri? GetParcelableExtraCompat(Intent intent, string key)
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            return intent.GetParcelableExtra(key, Java.Lang.Class.FromType(typeof(Android.Net.Uri))) as Android.Net.Uri;
#pragma warning disable CA1422
        return intent.GetParcelableExtra(key) as Android.Net.Uri;
#pragma warning restore CA1422
    }

    private void CopySharedUri(Android.Net.Uri uri)
    {
        using var input = ContentResolver?.OpenInputStream(uri);
        if (input is null) return;

        var displayName = $"shared_{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        using var cursor = ContentResolver?.Query(uri, null, null, null, null);
        if (cursor is not null && cursor.MoveToFirst())
        {
            var index = cursor.GetColumnIndex(Android.Provider.OpenableColumns.DisplayName);
            if (index >= 0) displayName = cursor.GetString(index) ?? displayName;
        }

        var inbox = Path.Combine(FileSystem.CacheDirectory, "shared-inbox");
        Directory.CreateDirectory(inbox);
        var destination = Path.Combine(inbox, $"{Guid.NewGuid():N}_{displayName}");
        using var output = File.Create(destination);
        input.CopyTo(output);
        SharedAttachmentInbox.Enqueue(destination);
    }
}

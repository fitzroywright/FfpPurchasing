using FFP.Purchasing.Tablet.Services;

namespace FFP.Purchasing.Tablet.Pages;

internal static class Ui
{
    public static Color Blue => Color.FromArgb("#07579A");
    public static Color Ink => Color.FromArgb("#17233C");
    public static Color Muted => Color.FromArgb("#64748B");
    public static Color Surface => Color.FromArgb("#FFFFFF");
    public static Color Canvas => Color.FromArgb("#F5F8FC");

    public static Border Card(View content, Thickness? padding = null) => new()
    {
        Stroke = Color.FromArgb("#DCE5EF"),
        StrokeThickness = 1,
        BackgroundColor = Surface,
        Padding = padding ?? new Thickness(18),
        StrokeShape = new RoundRectangle { CornerRadius = 12 },
        Content = content
    };

    public static Label H1(string text) => new()
    {
        Text = text,
        FontSize = 25,
        FontAttributes = FontAttributes.Bold,
        TextColor = Ink
    };

    public static Label H2(string text) => new()
    {
        Text = text,
        FontSize = 18,
        FontAttributes = FontAttributes.Bold,
        TextColor = Ink
    };

    public static Label Hint(string text) => new()
    {
        Text = text,
        FontSize = 12,
        TextColor = Muted
    };

    public static Button Primary(string text) => new()
    {
        Text = text,
        BackgroundColor = Blue,
        TextColor = Colors.White,
        CornerRadius = 9,
        HeightRequest = 48
    };

    public static Button Secondary(string text) => new()
    {
        Text = text,
        BackgroundColor = Color.FromArgb("#EAF3FB"),
        TextColor = Blue,
        CornerRadius = 9,
        HeightRequest = 48
    };
}

public abstract class TabletPage : ContentPage
{
    protected readonly VerticalStackLayout Body = new() { Spacing = 14 };
    private readonly Label _status = new()
    {
        FontSize = 11,
        TextColor = Ui.Muted,
        HorizontalTextAlignment = TextAlignment.End
    };

    protected TabletPage(string title)
    {
        BackgroundColor = Ui.Canvas;

        var top = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection(
                new(GridLength.Auto),
                new(GridLength.Star),
                new(GridLength.Auto)),
            Padding = new Thickness(18, 10),
            BackgroundColor = Ui.Blue
        };

        top.Add(new Label
        {
            Text = "☰",
            TextColor = Colors.White,
            FontSize = 22,
            VerticalTextAlignment = TextAlignment.Center
        }, 0);

        top.Add(new Label
        {
            Text = "FFP Manager",
            TextColor = Colors.White,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            VerticalTextAlignment = TextAlignment.Center,
            Margin = new Thickness(12, 0)
        }, 1);

        top.Add(new Label
        {
            Text = "MS",
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = Color.FromArgb("#1675C8"),
            Padding = new Thickness(9, 6)
        }, 2);

        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection(
                new(GridLength.Star),
                new(GridLength.Auto)),
            Padding = new Thickness(20, 14, 20, 5)
        };
        header.Add(Ui.H1(title), 0, 0);
        header.Add(_status, 1, 0);

        var scroll = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(20, 6, 20, 18),
                Children = { Body }
            }
        };

        var bottom = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection(
                new(GridLength.Star),
                new(GridLength.Star),
                new(GridLength.Star),
                new(GridLength.Star)),
            Padding = new Thickness(12, 8),
            BackgroundColor = Colors.White
        };

        var home = new Button
        {
            Text = "⌂  Home",
            BackgroundColor = Colors.Transparent,
            TextColor = Ui.Blue
        };
        home.Clicked += async (_, _) => await Navigation.PopToRootAsync();

        var requests = new Button
        {
            Text = "▤  My Requests",
            BackgroundColor = Colors.Transparent,
            TextColor = Ui.Ink
        };
        requests.Clicked += async (_, _) => await OpenRequestsAsync();

        var sync = new Button
        {
            Text = "↻  Sync",
            BackgroundColor = Colors.Transparent,
            TextColor = Ui.Ink
        };
        sync.Clicked += async (_, _) => await OpenSyncAsync();

        var more = new Button
        {
            Text = "•••  More",
            BackgroundColor = Colors.Transparent,
            TextColor = Ui.Ink
        };
        more.Clicked += async (_, _) => await Navigation.PushAsync(new SettingsPage());

        bottom.Add(home, 0, 0);
        bottom.Add(requests, 1, 0);
        bottom.Add(sync, 2, 0);
        bottom.Add(more, 3, 0);

        Content = new Grid
        {
            RowDefinitions = new RowDefinitionCollection(
                new(GridLength.Auto),
                new(GridLength.Auto),
                new(GridLength.Star),
                new(GridLength.Auto)),
            Children = { top, header, scroll, bottom }
        };

        Grid.SetRow(header, 1);
        Grid.SetRow(scroll, 2);
        Grid.SetRow(bottom, 3);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _status.Text = Connectivity.Current.NetworkAccess == NetworkAccess.None
            ? "OFFLINE • local data available"
            : "NETWORK AVAILABLE";
    }

    private async Task OpenRequestsAsync()
    {
        var s = Handler!.MauiContext!.Services;
        await Navigation.PushAsync(new RequestsPage(
            s.GetRequiredService<IPurchaseStore>(),
            s.GetRequiredService<ILocalItemCache>(),
            s.GetRequiredService<IAttachmentService>(),
            s.GetRequiredService<ISyncService>(),
            s.GetRequiredService<IRequestValidator>(),
            s.GetRequiredService<IAuditService>(),
            s.GetRequiredService<IReferenceDataService>()));
    }

    private async Task OpenSyncAsync()
    {
        var s = Handler!.MauiContext!.Services;
        await Navigation.PushAsync(new SyncStatusPage(
            s.GetRequiredService<IPurchaseStore>(),
            s.GetRequiredService<IPendingSyncWorker>(),
            s.GetRequiredService<IReferenceDataService>(),
            s.GetRequiredService<ISyncService>()));
    }
}
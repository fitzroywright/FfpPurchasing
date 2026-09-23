namespace FFP.Purchasing.Tablet.Pages;
public sealed class SettingsPage : ContentPage
{
    private readonly Entry _api=new(){Placeholder="https://server/"};
    public SettingsPage(){Title="Settings";_api.Text=Preferences.Default.Get("PurchasingApiBaseUrl","https://purchasing.ffpja.org/");var save=new Button{Text="Save"};save.Clicked+=(_,__)=>{Preferences.Default.Set("PurchasingApiBaseUrl",_api.Text?.Trim()??"");DisplayAlert("Saved","API address saved. Restart the app to apply it.","OK");};Content=new VerticalStackLayout{Padding=24,Spacing=12,Children={new Label{Text="Settings",FontSize=28,FontAttributes=FontAttributes.Bold},new Label{Text="Purchasing API"},_api,save}};}
}

using FFP.Purchasing.Tablet.Services;
namespace FFP.Purchasing.Tablet.Pages;
public sealed class DiagnosticsPage : ContentPage
{
    private readonly IAppDiagnostics _diag; private readonly VerticalStackLayout _results=new(){Spacing=8};
    public DiagnosticsPage(IAppDiagnostics diag){_diag=diag;Title="Diagnostics";var run=new Button{Text="Run Level 5"};run.Clicked+=Run;Content=new ScrollView{Content=new VerticalStackLayout{Padding=24,Children={new Label{Text="Diagnostics",FontSize=28,FontAttributes=FontAttributes.Bold},new Label{Text="Level 5 is the lightest routine diagnostic. It does not change system state."},run,_results}}};}
    private async void Run(object?s,EventArgs e){_results.Children.Clear();foreach(var r in await _diag.RunLevel5Async())_results.Children.Add(new Label{Text=$"{r.Id} • {(r.Passed?"PASS":"FAIL")} • {r.Name} • {r.Detail}"});}
}

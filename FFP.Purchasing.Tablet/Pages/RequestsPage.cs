using FFP.Purchasing.Tablet.Services;

namespace FFP.Purchasing.Tablet.Pages;

public sealed class RequestsPage : TabletPage
{
    private readonly IPurchaseStore _store;
    private readonly ILocalItemCache _items;
    private readonly IAttachmentService _attachments;
    private readonly ISyncService _sync;
    private readonly IRequestValidator _validator;
    private readonly IAuditService _audit;
    private readonly IReferenceDataService _reference;
    private readonly VerticalStackLayout _list = new() { Spacing = 8 };

    public RequestsPage(
        IPurchaseStore store,
        ILocalItemCache items,
        IAttachmentService attachments,
        ISyncService sync,
        IRequestValidator validator,
        IAuditService audit,
        IReferenceDataService reference)
        : base("My Requests")
    {
        _store = store;
        _items = items;
        _attachments = attachments;
        _sync = sync;
        _validator = validator;
        _audit = audit;
        _reference = reference;
        Body.Children.Add(_list);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _list.Children.Clear();

        foreach (var request in (await _store.GetAllAsync()).OrderByDescending(x => x.UpdatedAt))
        {
            var button = Ui.Secondary(
                $"{request.ServerNumber ?? request.LocalNumber}   " +
                $"{request.Type}   {request.Total:C2}   {request.Status}");

            button.Clicked += async (_, _) => await Navigation.PushAsync(
                new RequestEditorPage(
                    request, _store, _items, _attachments, _sync,
                    _validator, _audit, _reference));

            _list.Children.Add(button);
        }
    }
}
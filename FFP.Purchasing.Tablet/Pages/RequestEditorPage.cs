using FFP.Purchasing.Tablet.Models;
using FFP.Purchasing.Tablet.Services;

namespace FFP.Purchasing.Tablet.Pages;

public sealed class RequestEditorPage : TabletPage
{
    private readonly PurchaseRequest _request;
    private readonly IPurchaseStore _store;
    private readonly ILocalItemCache _items;
    private readonly IAttachmentService _attachments;
    private readonly ISyncService _sync;
    private readonly IRequestValidator _validator;
    private readonly IAuditService _audit;
    private readonly IReferenceDataService _reference;

    private readonly VerticalStackLayout _content = new() { Spacing = 12 };
    private readonly Label _stepper = new() { TextColor = Ui.Blue, FontAttributes = FontAttributes.Bold };
    private int _step = 1;

    public RequestEditorPage(
        PurchaseRequest request,
        IPurchaseStore store,
        ILocalItemCache items,
        IAttachmentService attachments,
        ISyncService sync,
        IRequestValidator validator,
        IAuditService audit,
        IReferenceDataService reference)
        : base(request.Type == RequestType.PurchaseOrder
            ? "Create Purchase Order Requisition"
            : "Create Payment Requisition")
    {
        _request = request;
        _store = store;
        _items = items;
        _attachments = attachments;
        _sync = sync;
        _validator = validator;
        _audit = audit;
        _reference = reference;

        Body.Children.Add(_stepper);
        Body.Children.Add(_content);
        Render();
    }

    private void Render()
    {
        _content.Children.Clear();
        _stepper.Text =
            $"{(_step == 1 ? "●" : "○")} General    " +
            $"{(_step == 2 ? "●" : "○")} {(_request.Type == RequestType.PurchaseOrder ? "Items" : "Payment Details")}    " +
            $"{(_step == 3 ? "●" : "○")} Accounting    " +
            $"{(_step == 4 ? "●" : "○")} Attachments    " +
            $"{(_step == 5 ? "●" : "○")} Review";

        switch (_step)
        {
            case 1: RenderGeneral(); break;
            case 2: RenderDetails(); break;
            case 3: RenderAccounting(); break;
            case 4: RenderAttachments(); break;
            default: RenderReview(); break;
        }

        var nav = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection(
                new(GridLength.Star), new(GridLength.Star)),
            ColumnSpacing = 12
        };

        var back = Ui.Secondary(_step == 1 ? "Cancel" : "← Back");
        back.Clicked += async (_, _) =>
        {
            if (_step == 1) await Navigation.PopAsync();
            else { _step--; Render(); }
        };

        var next = Ui.Primary(_step == 5 ? "Submit Request" : "Next →");
        next.Clicked += async (_, _) =>
        {
            await _store.SaveAsync(_request);
            if (_step < 5) { _step++; Render(); }
            else await SubmitAsync();
        };

        nav.Add(back, 0, 0);
        nav.Add(next, 1, 0);
        _content.Children.Add(nav);
    }

    private void RenderGeneral()
    {
        var stack = new VerticalStackLayout { Spacing = 10 };
        stack.Children.Add(Ui.H2("Request Information"));

        var requestor = new Entry { Text = _request.Requestor, Placeholder = "Requested By" };
        requestor.TextChanged += (_, e) => _request.Requestor = e.NewTextValue ?? "";
        stack.Children.Add(requestor);

        var department = new Entry { Text = _request.Department, Placeholder = "Department" };
        department.TextChanged += (_, e) => _request.Department = e.NewTextValue ?? "";
        stack.Children.Add(department);

        var vendor = Ui.Secondary(string.IsNullOrWhiteSpace(_request.VendorName)
            ? "Select Vendor from cached FFP Manager list"
            : $"Vendor: {_request.VendorName}");
        vendor.Clicked += async (_, _) => await PickVendorAsync();
        stack.Children.Add(vendor);

        var purpose = new Editor
        {
            Text = _request.Purpose,
            Placeholder = "Purpose / justification",
            HeightRequest = 100
        };
        purpose.TextChanged += (_, e) => _request.Purpose = e.NewTextValue ?? "";
        stack.Children.Add(purpose);

        if (_request.Type == RequestType.PurchaseOrder)
        {
            var type = new Picker
            {
                Title = "PO Type",
                ItemsSource = Enum.GetNames<PurchaseOrderType>()
            };
            type.SelectedIndex = (int)_request.PurchaseOrderType;
            type.SelectedIndexChanged += (_, _) =>
                _request.PurchaseOrderType = (PurchaseOrderType)Math.Max(0, type.SelectedIndex);
            stack.Children.Add(type);

            var quote = new Entry { Text = _request.QuoteNo, Placeholder = "Quote No" };
            quote.TextChanged += (_, e) => _request.QuoteNo = e.NewTextValue ?? "";
            stack.Children.Add(quote);
        }
        else
        {
            var priority = new Picker
            {
                Title = "Priority",
                ItemsSource = Enum.GetNames<PriorityLevel>()
            };
            priority.SelectedIndex = (int)_request.Priority;
            priority.SelectedIndexChanged += (_, _) =>
                _request.Priority = (PriorityLevel)Math.Max(0, priority.SelectedIndex);
            stack.Children.Add(priority);

            var po = Ui.Secondary(string.IsNullOrWhiteSpace(_request.PurchaseOrderNo)
                ? "Link Purchase Order (optional)"
                : $"PO: {_request.PurchaseOrderNo}");
            po.Clicked += async (_, _) => await PickPurchaseOrderAsync();
            stack.Children.Add(po);
        }

        _content.Children.Add(Ui.Card(stack));
    }

    private void RenderDetails()
    {
        if (_request.Type == RequestType.PurchaseOrder)
        {
            var list = new VerticalStackLayout { Spacing = 8 };
            foreach (var line in _request.Lines)
            {
                list.Children.Add(Ui.Card(new Label
                {
                    Text = $"{line.Quantity:0.##} {line.Uom}  {line.Description}\n" +
                           $"{line.UnitPrice:C2} each  •  {line.Total:C2}",
                    TextColor = Ui.Ink
                }, new Thickness(10)));
            }

            var add = Ui.Primary("+ Add Item");
            add.Clicked += AddItem;

            var stack = new VerticalStackLayout { Spacing = 10 };
            stack.Children.Add(Ui.H2("Items"));
            stack.Children.Add(list);
            stack.Children.Add(add);
            stack.Children.Add(new Label
            {
                Text = $"Total Amount  {_request.Total:C2}",
                HorizontalTextAlignment = TextAlignment.End,
                FontAttributes = FontAttributes.Bold,
                FontSize = 20
            });
            _content.Children.Add(Ui.Card(stack));
        }
        else
        {
            var stack = new VerticalStackLayout { Spacing = 10 };
            stack.Children.Add(Ui.H2("Payment Details"));

            var invoice = new Entry { Text = _request.InvoiceNo, Placeholder = "Invoice No" };
            invoice.TextChanged += (_, e) => _request.InvoiceNo = e.NewTextValue ?? "";
            stack.Children.Add(invoice);

            var batch = new Entry { Text = _request.InvoiceBatchNo, Placeholder = "Invoice Batch No" };
            batch.TextChanged += (_, e) => _request.InvoiceBatchNo = e.NewTextValue ?? "";
            stack.Children.Add(batch);

            var entry = new Entry { Text = _request.InvoiceEntryNo, Placeholder = "Invoice Entry No" };
            entry.TextChanged += (_, e) => _request.InvoiceEntryNo = e.NewTextValue ?? "";
            stack.Children.Add(entry);

            foreach (var line in _request.PaymentLines)
            {
                stack.Children.Add(Ui.Card(new Label
                {
                    Text = $"{line.Description}\nRequested {line.AmountRequested:C2}",
                    TextColor = Ui.Ink
                }, new Thickness(10)));
            }

            var add = Ui.Primary("+ Add Payment Detail");
            add.Clicked += AddPayment;
            stack.Children.Add(add);
            stack.Children.Add(new Label
            {
                Text = $"Request Total  {_request.Total:C2}",
                HorizontalTextAlignment = TextAlignment.End,
                FontAttributes = FontAttributes.Bold,
                FontSize = 20
            });
            _content.Children.Add(Ui.Card(stack));
        }
    }

    private void RenderAccounting()
    {
        var stack = new VerticalStackLayout { Spacing = 10 };
        stack.Children.Add(Ui.H2("Approval / Accounting"));

        var terms = new Entry { Text = _request.Terms, Placeholder = "Terms / accounting reference" };
        terms.TextChanged += (_, e) => _request.Terms = e.NewTextValue ?? "";
        stack.Children.Add(terms);

        var first = new Entry { Text = _request.FirstApprover, Placeholder = "First Approver" };
        first.TextChanged += (_, e) => _request.FirstApprover = e.NewTextValue ?? "";
        stack.Children.Add(first);

        var second = new Entry { Text = _request.SecondApprover, Placeholder = "Second Approver" };
        second.TextChanged += (_, e) => _request.SecondApprover = e.NewTextValue ?? "";
        stack.Children.Add(second);

        if (_request.Type == RequestType.PaymentRequisition)
        {
            var third = new Entry { Text = _request.ThirdApprover, Placeholder = "Third Approver (if required)" };
            third.TextChanged += (_, e) => _request.ThirdApprover = e.NewTextValue ?? "";
            stack.Children.Add(third);

            var account = new Entry { Text = _request.InvoiceAccountNo, Placeholder = "Invoice Account No" };
            account.TextChanged += (_, e) => _request.InvoiceAccountNo = e.NewTextValue ?? "";
            stack.Children.Add(account);
        }

        var notes = new Editor
        {
            Text = _request.Notes,
            Placeholder = "Notes / remarks",
            HeightRequest = 90
        };
        notes.TextChanged += (_, e) => _request.Notes = e.NewTextValue ?? "";
        stack.Children.Add(notes);

        _content.Children.Add(Ui.Card(stack));
    }

    private void RenderAttachments()
    {
        var stack = new VerticalStackLayout { Spacing = 10 };
        stack.Children.Add(Ui.H2("Supporting Documents"));
        stack.Children.Add(Ui.Hint(
            "Attach quotations, invoices, approvals, specifications or other supporting evidence. " +
            "Files are stored locally until synced."));

        foreach (var a in _request.Attachments)
        {
            stack.Children.Add(Ui.Card(new Label
            {
                Text = $"📎 {a.FileName}  •  {(a.PendingSync ? "Pending Sync" : "Synced")}",
                TextColor = Ui.Ink
            }, new Thickness(10)));
        }

        var actions = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection(
                new(GridLength.Star), new(GridLength.Star)),
            ColumnSpacing = 10
        };

        var photo = Ui.Secondary("Take Photo");
        photo.Clicked += AddPhoto;
        var files = Ui.Primary("Choose Files");
        files.Clicked += AddFiles;
        actions.Add(photo, 0, 0);
        actions.Add(files, 1, 0);
        stack.Children.Add(actions);

        _content.Children.Add(Ui.Card(stack));
    }

    private void RenderReview()
    {
        _content.Children.Add(Ui.Card(new Label
        {
            Text =
                $"Type: {_request.Type}\n" +
                $"Requested By: {_request.Requestor}\n" +
                $"Department: {_request.Department}\n" +
                $"Vendor / Pay To: {_request.VendorName} / {_request.PayTo}\n" +
                $"Purpose: {_request.Purpose}\n" +
                $"Total: {_request.Total:C2}\n" +
                $"Attachments: {_request.Attachments.Count}\n" +
                "Offline submission state: Pending Sync",
            TextColor = Ui.Ink,
            FontSize = 16
        }));
    }

    private async Task PickVendorAsync()
    {
        var vendors = await _reference.GetVendorsAsync();
        if (vendors.Count == 0)
        {
            await _reference.RefreshVendorsAsync();
            vendors = await _reference.GetVendorsAsync();
        }

        if (vendors.Count == 0)
        {
            await DisplayAlert("Vendor cache empty",
                "Connect to the FFP Manager Bridge once to cache the Vendor Listing.", "OK");
            return;
        }

        var labels = vendors.Take(40).Select(x => $"{x.Id} • {x.Name}").ToArray();
        var choice = await DisplayActionSheet("Select Vendor", "Cancel", null, labels);
        var vendor = vendors.FirstOrDefault(x => $"{x.Id} • {x.Name}" == choice);
        if (vendor is null) return;

        _request.VendorId = vendor.Id;
        _request.VendorName = vendor.Name;
        _request.PayTo = string.IsNullOrWhiteSpace(vendor.PayTo) ? vendor.Name : vendor.PayTo;
        Render();
    }

    private async Task PickPurchaseOrderAsync()
    {
        var list = await _reference.SearchPurchaseOrdersAsync();
        if (list.Count == 0)
        {
            await DisplayAlert("Purchase Orders",
                "No open Purchase Orders are available from the Bridge right now. You can continue without linking one.",
                "OK");
            return;
        }

        var labels = list.Select(x =>
            $"{x.PurchaseOrderNo} • {x.VendorName} • Balance {x.Balance:C2}").ToArray();
        var choice = await DisplayActionSheet("Link Purchase Order", "Cancel", null, labels);
        var po = list.FirstOrDefault(x =>
            $"{x.PurchaseOrderNo} • {x.VendorName} • Balance {x.Balance:C2}" == choice);
        if (po is null) return;

        _request.PurchaseOrderId = po.Id;
        _request.PurchaseOrderNo = po.PurchaseOrderNo;
        _request.VendorId = po.VendorId;
        _request.VendorName = po.VendorName;
        _request.PayTo = po.VendorName;
        Render();
    }

    private async void AddItem(object? sender, EventArgs e)
    {
        var description = await DisplayPromptAsync("Item", "Description");
        if (string.IsNullOrWhiteSpace(description)) return;

        var quantityText = await DisplayPromptAsync("Quantity", "Quantity", initialValue: "1", keyboard: Keyboard.Numeric);
        var uom = await DisplayPromptAsync("Unit", "Unit of Measure", initialValue: "EA");
        var priceText = await DisplayPromptAsync("Unit Price", "Unit Price", initialValue: "0", keyboard: Keyboard.Numeric);

        decimal.TryParse(quantityText, out var quantity);
        decimal.TryParse(priceText, out var price);

        _request.Lines.Add(new PurchaseLine
        {
            Description = description.Trim(),
            Quantity = Math.Max(1, quantity),
            Uom = string.IsNullOrWhiteSpace(uom) ? "EA" : uom.Trim(),
            UnitPrice = price
        });

        await _items.LearnAsync(description, uom ?? "EA", price);
        await _store.SaveAsync(_request);
        Render();
    }

    private async void AddPayment(object? sender, EventArgs e)
    {
        var description = await DisplayPromptAsync("Payment Detail", "Description");
        if (string.IsNullOrWhiteSpace(description)) return;

        var amountText = await DisplayPromptAsync("Amount Requested", "Amount", initialValue: "0", keyboard: Keyboard.Numeric);
        decimal.TryParse(amountText, out var amount);

        _request.PaymentLines.Add(new PaymentLine
        {
            Description = description.Trim(),
            AmountRequested = Math.Max(0, amount)
        });

        await _store.SaveAsync(_request);
        Render();
    }

    private async void AddPhoto(object? sender, EventArgs e)
    {
        var attachment = await _attachments.CapturePhotoAsync();
        if (attachment is null) return;

        _request.Attachments.Add(attachment);
        await _store.SaveAsync(_request);
        Render();
    }

    private async void AddFiles(object? sender, EventArgs e)
    {
        _request.Attachments.AddRange(await _attachments.PickFilesAsync());
        await _store.SaveAsync(_request);
        Render();
    }

    private async Task SubmitAsync()
    {
        var issues = _validator.Validate(_request);
        if (issues.Count > 0)
        {
            await DisplayAlert("Cannot submit",
                string.Join("\n", issues.Select(x => $"{x.Field}: {x.Message}")), "OK");
            return;
        }

        await _audit.WriteAsync("SubmitStarted", _request.Id);
        var result = await _sync.SubmitAsync(_request);
        await _audit.WriteAsync(result.Succeeded ? "SubmitSucceeded" : "SubmitPending",
            _request.Id, result.Message);

        await DisplayAlert(result.Succeeded ? "Submitted" : "Saved for Sync",
            result.Message, "OK");
        await Navigation.PopToRootAsync();
    }
}
using System.Text.Json;

var builder=WebApplication.CreateBuilder(args);
var app=builder.Build();
var root=Path.Combine(app.Environment.ContentRootPath,"data");Directory.CreateDirectory(root);
var attachments=Path.Combine(root,"attachments");Directory.CreateDirectory(attachments);

app.MapGet("/health",()=>Results.Ok(new{status="Healthy",service="FFP.Purchasing.Api",utc=DateTimeOffset.UtcNow}));
app.MapGet("/api/reference/vendors",()=>Results.Ok(new[]{new{Id="V001",Name="Sample Vendor",TaxNumber=""}}));
app.MapGet("/api/reference/items",()=>Results.Ok(new[]{new{Id="I001",Description="Sample Item",Uom="EA",LastPrice=0m}}));
app.MapPost("/api/purchasing/requests",async(HttpRequest http)=>{
    if(!http.HasFormContentType)return Results.BadRequest("multipart/form-data required");
    var form=await http.ReadFormAsync();var request=form["request"].FirstOrDefault();
    if(string.IsNullOrWhiteSpace(request))return Results.BadRequest("request is required");
    var number=$"FFP-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000,9999)}";
    var folder=Path.Combine(attachments,number);Directory.CreateDirectory(folder);
    foreach(var file in form.Files){var safe=Path.GetFileName(file.FileName);await using var output=File.Create(Path.Combine(folder,safe));await file.CopyToAsync(output);}
    await File.WriteAllTextAsync(Path.Combine(folder,"request.json"),request);
    return Results.Ok(new{Number=number,Status="Submitted"});
});
app.MapGet("/api/purchasing/requests/{number}",(string number)=>{
    var folder=Path.Combine(attachments,Path.GetFileName(number));var path=Path.Combine(folder,"request.json");
    return File.Exists(path)?Results.Text(File.ReadAllText(path),"application/json"):Results.NotFound();
});
app.Run();

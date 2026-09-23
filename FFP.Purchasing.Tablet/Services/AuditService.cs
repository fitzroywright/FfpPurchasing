using System.Text.Json;
namespace FFP.Purchasing.Tablet.Services;
public interface IAuditService{Task WriteAsync(string action,Guid? requestId=null,string? detail=null);}
public sealed class AuditService:IAuditService
{
 private readonly string _path=Path.Combine(FileSystem.AppDataDirectory,"audit.jsonl");
 public async Task WriteAsync(string action,Guid? requestId=null,string? detail=null)
 {
  var e=new{utc=DateTimeOffset.UtcNow,action,requestId,detail};
  await File.AppendAllTextAsync(_path,JsonSerializer.Serialize(e)+Environment.NewLine);
 }
}

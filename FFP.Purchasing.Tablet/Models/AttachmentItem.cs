namespace FFP.Purchasing.Tablet.Models;

public sealed class AttachmentItem
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string FileName { get; init; }
    public required string LocalPath { get; init; }
    public required string ContentType { get; init; }
    public long SizeBytes { get; init; }
    public DateTimeOffset AddedAt { get; init; } = DateTimeOffset.Now;
    public string Source { get; init; } = "Device";
    public bool PendingSync { get; set; } = true;
}

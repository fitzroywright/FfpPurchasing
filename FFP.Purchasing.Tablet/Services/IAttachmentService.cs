using FFP.Purchasing.Tablet.Models;

namespace FFP.Purchasing.Tablet.Services;

public interface IAttachmentService
{
    Task<AttachmentItem?> CapturePhotoAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AttachmentItem>> PickFilesAsync(CancellationToken cancellationToken = default);
    Task<AttachmentItem> ImportFileAsync(string sourcePath, string source = "Shared");
}

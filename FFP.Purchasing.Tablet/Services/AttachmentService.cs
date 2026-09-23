using FFP.Purchasing.Tablet.Models;

namespace FFP.Purchasing.Tablet.Services;

public sealed class AttachmentService : IAttachmentService
{
    private readonly string _attachmentRoot =
        Path.Combine(FileSystem.AppDataDirectory, "attachments");

    public AttachmentService()
    {
        Directory.CreateDirectory(_attachmentRoot);
    }

    public async Task<AttachmentItem?> CapturePhotoAsync(CancellationToken cancellationToken = default)
    {
        if (!MediaPicker.Default.IsCaptureSupported)
            return null;

        var result = await MediaPicker.Default.CapturePhotoAsync();
        if (result is null)
            return null;

        await using var source = await result.OpenReadAsync();
        return await SaveStreamAsync(
            source,
            result.FileName,
            result.ContentType ?? "image/jpeg",
            "Camera",
            cancellationToken);
    }

    public async Task<IReadOnlyList<AttachmentItem>> PickFilesAsync(CancellationToken cancellationToken = default)
    {
        var results = await FilePicker.Default.PickMultipleAsync(new PickOptions
        {
            PickerTitle = "Choose supporting documents"
        });

        var attachments = new List<AttachmentItem>();
        if (results is null)
            return attachments;

        foreach (var result in results)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await using var source = await result.OpenReadAsync();
            attachments.Add(await SaveStreamAsync(
                source,
                result.FileName,
                result.ContentType ?? "application/octet-stream",
                "Local storage",
                cancellationToken));
        }

        return attachments;
    }

    public async Task<AttachmentItem> ImportFileAsync(string sourcePath, string source = "Shared")
    {
        await using var stream = File.OpenRead(sourcePath);
        return await SaveStreamAsync(
            stream,
            Path.GetFileName(sourcePath),
            GetContentType(Path.GetExtension(sourcePath)),
            source,
            CancellationToken.None);
    }

    private async Task<AttachmentItem> SaveStreamAsync(
        Stream source,
        string originalFileName,
        string contentType,
        string sourceName,
        CancellationToken cancellationToken)
    {
        var safeName = string.Concat(originalFileName.Select(c =>
            Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        var destination = Path.Combine(
            _attachmentRoot,
            $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}_{safeName}");

        await using (var target = File.Create(destination))
            await source.CopyToAsync(target, cancellationToken);

        var info = new FileInfo(destination);
        return new AttachmentItem
        {
            FileName = originalFileName,
            LocalPath = destination,
            ContentType = contentType,
            SizeBytes = info.Length,
            Source = sourceName,
            PendingSync = true
        };
    }

    private static string GetContentType(string extension) =>
        extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/octet-stream"
        };
}

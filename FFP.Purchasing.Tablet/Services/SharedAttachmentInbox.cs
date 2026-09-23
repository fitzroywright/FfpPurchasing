using System.Collections.Concurrent;

namespace FFP.Purchasing.Tablet.Services;

public static class SharedAttachmentInbox
{
    private static readonly ConcurrentQueue<string> Paths = new();

    public static void Enqueue(string path) => Paths.Enqueue(path);

    public static IReadOnlyList<string> Drain()
    {
        var items = new List<string>();
        while (Paths.TryDequeue(out var path))
            items.Add(path);
        return items;
    }
}

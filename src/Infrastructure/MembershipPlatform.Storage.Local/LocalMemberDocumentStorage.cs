using MembershipPlatform.Core.Storage;

namespace MembershipPlatform.Storage.Local;

public sealed class LocalMemberDocumentStorage(string rootPath) : IMemberDocumentStorage
{
    private readonly string rootPath = NormalizeRootPath(rootPath);

    public async Task<MemberDocumentReference> SaveAsync(
        Guid memberId,
        string fileName,
        string contentType,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentNullException.ThrowIfNull(content);

        var memberDirectory = Path.Combine(rootPath, "members", memberId.ToString("N"));
        Directory.CreateDirectory(memberDirectory);

        var storedFileName = Guid.NewGuid().ToString("N");
        var filePath = Path.GetFullPath(Path.Combine(memberDirectory, storedFileName));
        EnsurePathIsWithinRoot(filePath);

        try
        {
            await using var destination = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                FileOptions.Asynchronous);
            await content.CopyToAsync(destination, cancellationToken);
        }
        catch
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            throw;
        }

        var storageKey = $"members/{memberId:N}/{storedFileName}";
        return new MemberDocumentReference(storageKey);
    }

    private static string NormalizeRootPath(string rootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);
        return Path.GetFullPath(rootPath);
    }

    private void EnsurePathIsWithinRoot(string filePath)
    {
        var rootPrefix = rootPath.EndsWith(Path.DirectorySeparatorChar)
            ? rootPath
            : rootPath + Path.DirectorySeparatorChar;

        if (!filePath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The generated storage path is invalid.");
        }
    }
}

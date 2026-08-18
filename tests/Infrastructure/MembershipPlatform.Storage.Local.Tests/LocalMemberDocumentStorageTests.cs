namespace MembershipPlatform.Storage.Local.Tests;

[TestClass]
public sealed class LocalMemberDocumentStorageTests
{
    [TestMethod]
    public async Task GivenDocumentWhenSavingThenUsesGeneratedKeyInsideConfiguredRoot()
    {
        // Arrange
        var rootPath = Path.Combine(
            Path.GetTempPath(),
            $"membership-platform-storage-{Guid.NewGuid():N}");
        var memberId = Guid.NewGuid();
        var expectedContent = new byte[] { 1, 2, 3, 4 };
        using var content = new MemoryStream(expectedContent);
        var storage = new LocalMemberDocumentStorage(rootPath);

        try
        {
            // Act
            var result = await storage.SaveAsync(
                memberId,
                "../../waiver.pdf",
                "application/pdf",
                content);

            // Assert
            Assert.StartsWith($"members/{memberId:N}/", result.StorageKey);
            Assert.DoesNotContain("waiver.pdf", result.StorageKey);
            Assert.DoesNotContain("..", result.StorageKey);
            var storedPath = Path.Combine(
                rootPath,
                result.StorageKey.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(storedPath));
            CollectionAssert.AreEqual(expectedContent, await File.ReadAllBytesAsync(storedPath));
        }
        finally
        {
            if (Directory.Exists(rootPath))
            {
                Directory.Delete(rootPath, recursive: true);
            }
        }
    }
}

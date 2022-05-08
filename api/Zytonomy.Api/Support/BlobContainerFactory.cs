
namespace Zytonomy.Api.Support;

/// <summary>
/// Static class for creating Blob container clients.
/// </summary>
public static class BlobContainerFactory
{
    /// <summary>
    /// Creates an instance of a container client based on the specified name.
    /// </summary>
    /// <param name="name">The name of the contianer to instantiate the clent for.</param>
    /// <returns>An instance of the blob container client.</returns>
    public static BlobContainerClient Create(string name) {
        CloudStorageAccount cloudStorage = CloudStorageAccount
            .Parse(Environment.GetEnvironmentVariable("ContentSourceStorage"));

        return new BlobContainerClient(
            new Uri($"{cloudStorage.BlobEndpoint.AbsoluteUri}{name.ToLowerInvariant()}"),
            new StorageSharedKeyCredential(
                Environment.GetEnvironmentVariable("Blob_AccountName"),
                Environment.GetEnvironmentVariable("Blob_AccountKey")
            ));
    }

    /// <summary>
    /// Creates a client to the default container (zytonomy)
    /// </summary>
    /// <returns>An instance created for the default container.</returns>
    public static BlobContainerClient CreateDefault()
    {
        return Create("zytonomy");
    }
}

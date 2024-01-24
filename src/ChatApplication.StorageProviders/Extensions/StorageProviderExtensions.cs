namespace ChatApplication.StorageProviders.Extensions;

public static class StorageProviderExtensions
{
    public static async Task<string?> ReadAsStringAsync(this IStorageProvider storageProvider, string path)
    {
        using var stream = await storageProvider.ReadAsync(path);
        if (stream is null)
        {
            return null;
        }

        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();

        reader.Close();
        return content;
    }

    public static async Task<byte[]?> ReadAsByteArrayAsync(this IStorageProvider storageProvider, string path)
    {
        using var stream = await storageProvider.ReadAsync(path);
        if (stream is null)
        {
            return null;
        }

        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);

        stream.Close();
        return memoryStream.ToArray();
    }

    public static async Task UploadAsync(this IStorageProvider storageProvider, byte[] content, string path, bool overwrite = false)
    {
        using var stream = new MemoryStream(content);
        await storageProvider.UploadAsync(stream, path, overwrite);
    }
}
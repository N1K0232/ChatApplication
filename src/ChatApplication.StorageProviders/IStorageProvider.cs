namespace ChatApplication.StorageProviders;

public interface IStorageProvider
{
    Task DeleteAsync(string path);

    Task<Stream?> ReadAsync(string path);

    Task UploadAsync(Stream stream, string path, bool overwrite = false);
}
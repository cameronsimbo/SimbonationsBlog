using Blog.Application.Common.Interfaces;

namespace Blog.Infrastructure;

public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public FileStorageService()
    {
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        if (Directory.Exists(_basePath) == false)
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<string> UploadAsync(Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        string uniqueName = $"{Guid.NewGuid()}_{fileName}";
        string filePath = Path.Combine(_basePath, uniqueName);

        using FileStream fileStream = new(filePath, FileMode.Create);
        await content.CopyToAsync(fileStream, cancellationToken);

        return $"/uploads/{uniqueName}";
    }

    public Task<Stream> DownloadAsync(string path, CancellationToken cancellationToken = default)
    {
        string filePath = Path.Combine(_basePath, Path.GetFileName(path));

        if (File.Exists(filePath) == false)
        {
            throw new FileNotFoundException("File not found.", filePath);
        }

        Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        string filePath = Path.Combine(_basePath, Path.GetFileName(path));

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }
}

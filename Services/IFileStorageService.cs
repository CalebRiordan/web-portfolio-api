namespace PanChatApi.Services;

public interface IFileStorageService<T>
{
    Task<string> UploadFileAsync(IFormFile file, string projectId);
    Task<string> GetUrlAsync(string path, int expiresIn = 3600);
    Task<List<string>> GetUrlsAsync(List<string> paths, int expiresIn = 3600);
    Task DeleteFileAsync(string path);
}
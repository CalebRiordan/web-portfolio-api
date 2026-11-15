namespace WebPortfolioApi.Services;

using PanChatApi.Services;
using Supabase;
using Supabase.Storage.Interfaces;

public interface IBucket
{
    static abstract string Name { get; }
}

public class ImagesBucket : IBucket
{
    public static string Name => "Images";
}

public class SupabaseStorageService<T>(Client supabase) : IFileStorageService<T>
    where T : class, IBucket
{
    private IStorageFileApi<Supabase.Storage.FileObject> Bucket
    {
        get { return supabase.Storage.From(T.Name); }
    }

    public async Task DeleteFileAsync(string path)
    {
        await Bucket.Remove(path);
    }

    public async Task<string> GetUrlAsync(string path, int expiresIn = 3600)
    {
        return await Bucket.CreateSignedUrl(path, expiresIn);
    }

    public async Task<List<string>> GetUrlsAsync(List<string> paths, int expiresIn = 3600)
    {
        var res = await Bucket.CreateSignedUrls(paths, expiresIn);
        if (res == null || res.Count == 0)
            return [];
        return [.. res.Select(r => r.SignedUrl!)];
    }

    public async Task<string> UploadFileAsync(IFormFile file, string projectId)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is null or empty");

        var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var bytes = ms.ToArray();

        string fileName = projectId + "-thumbnail";
        await Bucket.Upload(
            bytes,
            fileName,
            new Supabase.Storage.FileOptions { ContentType = file.ContentType }
        );

        return fileName;
    }
}

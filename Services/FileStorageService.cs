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

    public async Task<Dictionary<string, string>> GetUrlsAsync(
        List<string> paths,
        int expiresIn = 3600
    )
    {
        if (paths.Count == 0) return [];
        var res = await Bucket.CreateSignedUrls(paths, expiresIn);
        if (res == null || res.Count == 0)
            return [];

        var dict = new Dictionary<string, string>();
        foreach (var image in res)
            dict.Add(image.Path!, image.SignedUrl!);

        return dict;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string projectId)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is null or empty");

        var bytes = new byte[file.Length];
        await file.OpenReadStream().ReadExactlyAsync(bytes);

        string fileName = projectId + "-thumbnail";
        await Bucket.Upload(
            bytes,
            fileName,
            new Supabase.Storage.FileOptions { ContentType = file.ContentType }
        );

        return fileName;
    }
}

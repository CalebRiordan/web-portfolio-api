using WebPortfolioApi.Models;

namespace WebPortfolioApi.Dtos;

public record ProjectDto(
    string? Name = "",
    string? Description = "",
    string? DateCompleted = "",
    string? ThumbnailUrl = "",
    string? GithubUrl = "",
    string? DemoUrl = ""
);

public static class Extensions
{
    public static ProjectDto asProjectDto(this Project p)
    {
        return new ProjectDto(
            p.Name,
            p.Description,
            p.DateCompleted,
            p.ThumbnailUrl,
            p.GithubUrl,
            p.DemoUrl
        );
    }

    public static ProjectDto asProjectDtoWithSasToken(this Project p, string sasToken)
    {
        string ThumbnailUrl = "";
        if (p.ThumbnailUrl != "")
        {
            ThumbnailUrl = p.ThumbnailUrl + sasToken;
        }

        return new ProjectDto(
            p.Name,
            p.Description,
            p.DateCompleted,
            ThumbnailUrl,
            p.GithubUrl,
            p.DemoUrl
        );
    }
}

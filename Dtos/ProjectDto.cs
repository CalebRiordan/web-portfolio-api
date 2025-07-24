using WebPortfolioApi.Models;

namespace WebPortfolioApi.Dtos;

//Data transfer object to ensure only certain fields are exposed through the API. 
public record ProjectDto(string? Name = "", string? Description = "", string? DateCompleted = "", string? ThumbnailUrl = "", string? GithubUrl = "", string? DemoUrl = "");

public static class Extensions
{
    //Map a Project object to a ProjectDto object
    //This is usually used when we want to include a Project object in the response
    public static ProjectDto asProjectDto(this Project p)
    {
        return new ProjectDto(p.Name, p.Description, p.DateCompleted, p.ThumbnailUrl, p.GithubUrl, p.DemoUrl);
    }

    public static ProjectDto asProjectDtoWithSasToken(this Project p, string sasToken)
    {
        string ThumbnailUrl = "";
        if (p.ThumbnailUrl != "")
        {
            ThumbnailUrl = p.ThumbnailUrl + sasToken;
        }

        return new ProjectDto(p.Name, p.Description, p.DateCompleted, ThumbnailUrl, p.GithubUrl, p.DemoUrl);
    }
}
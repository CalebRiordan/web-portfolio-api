using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PanChatApi.Services;
using WebPortfolioApi.data;
using WebPortfolioApi.Dtos;
using WebPortfolioApi.Models;
using WebPortfolioApi.Services;

namespace WebPortfolioApi.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ProjectsController(
    WebfolioDbContext context,
    IFileStorageService<ImagesBucket> storage,
    ILogger<ProjectsController> logger
) : ControllerBase
{
    private readonly WebfolioDbContext _context = context;
    private readonly IFileStorageService<ImagesBucket> _storage = storage;
    public readonly ILogger<ProjectsController> _logger = logger;

    //Returns project data of project with specified ID
    [HttpGet]
    [Route("{projNum}")]
    public async Task<ActionResult<Project>> GetProjectAsync([FromRoute] int projNum)
    {
        //replicate project ID
        string id = "proj-" + projNum.ToString();
        Project? project = await _context
            .Projects.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
            return NotFound();

        //append SAS token to blob URL
        project.ThumbnailUrl = await _storage.GetUrlAsync(project.ThumbnailUrl);
        return Ok(project);
    }

    //Returns a list of all projects
    [HttpGet]
    public async Task<ActionResult<List<Project>>> GetAllProjectsAsync()
    {
        //Append SAS token to URL of every blob retrieved
        var projects = await _context.Projects.ToListAsync();
        List<string> urls = [.. projects.Select(p => p.ThumbnailUrl)];
        var safeUrls = await _storage.GetUrlsAsync(urls);
        foreach (var project in projects)
        {
            if (safeUrls.TryGetValue(project.ThumbnailUrl, out var safeUrl))
                project.ThumbnailUrl = safeUrl;
        }

        return Ok(projects);
    }

    //Adds a new project with provided data to the azure database
    [HttpPost]
    [Route("{projNum}")]
    public async Task<ActionResult<Project>> CreateProjectWithImageAsync(
        [FromRoute] int projNum,
        [FromForm] CreateProjectModel model
    )
    {
        //create project ID
        string id = "proj-" + projNum;
        Project? project = await _context.Projects.FindAsync(id);

        if (project != null)
        {
            return StatusCode(409, "Item already exists");
        }

        //Create Project object from provided model (without ThumbnailUrl at first)
        project = new()
        {
            Id = id,
            Name = model.Name,
            Description = model.Description,
            DateCompleted = model.DateCompleted,
            ThumbnailUrl = "",
            GithubUrl = model.GithubUrl,
            DemoUrl = model.DemoUrl,
        };

        try
        {
            //If a new thumbnail has been uploaded
            if (model.Thumbnail != null)
            {
                project.ThumbnailUrl = await _storage.UploadFileAsync(model.Thumbnail, id);
            }
        }
        catch (Exception)
        {
            return new JsonResult(new { message = "Exception while uploading image file" });
        }

        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();

        return Ok(project);
    }

    //Updates existing project specified by ID with values from the provided project object
    [HttpPut]
    [Route("{projNum}")]
    public async Task<ActionResult<Project>> UpdateProjectAsync(
        [FromRoute] int projNum,
        [FromForm] CreateProjectModel model
    )
    {
        //replicate project ID
        string id = "proj-" + projNum;
        Project? project = await _context.Projects.FindAsync(id);

        if (project == null)
            return NotFound();

        //If the thumbnail must be updated:
        if (model.Thumbnail != null)
        {
            try
            {
                // The new thumbnail will be uploaded and overwrite the old thumbnail
                // THE THUMBNAIL URL WILL STAY THE SAME
                await _storage.UploadFileAsync(model.Thumbnail, id);
            }
            catch (RequestFailedException e)
            {
                return BadRequest(e);
            }
        }

        ProjectDto updatedProject = new()
        {
            Name = model.Name,
            Description = model.Description,
            DateCompleted = model.DateCompleted,
            ThumbnailUrl = project.ThumbnailUrl, // same URL, always
            GithubUrl = model.GithubUrl,
            DemoUrl = model.DemoUrl,
        };

        _context.Entry(project).CurrentValues.SetValues(updatedProject);
        await _context.SaveChangesAsync();

        return Ok(updatedProject);
    }

    //Deletes data of a project specified by ID
    [HttpDelete]
    [Route("{projNum}")]
    public async Task<IActionResult> DeleteProjectAsync([FromRoute] int projNum)
    {
        //Replicate project ID
        string id = "proj-" + projNum;
        Project? project = await _context.Projects.FindAsync(id);

        if (project == null)
            return NotFound();

        //Delete blob for thumbnail image if one exists:
        if (project.ThumbnailUrl != "")
            await _storage.DeleteFileAsync(project.ThumbnailUrl);

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        return Ok();
    }
}

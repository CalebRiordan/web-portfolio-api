using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PanChatApi.Services;
using Supabase;
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
    public async Task<IActionResult> getProjectAsync([FromRoute] int projNum)
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
    public async Task<IActionResult> getAllProjectsAsync()
    {
        //Append SAS token to URL of every blob retrieved
        var projects = await _context.Projects.ToListAsync();
        List<string> urls = [.. projects.Select(p => p.ThumbnailUrl)];
        var safeUrls = await _storage.GetUrlsAsync(urls);
        // projects.


        return Ok(projectDtos);
    }

    //Adds a new project with provided data to the azure database
    [HttpPost]
    [Route("{projNum}")]
    public async Task<IActionResult> createProjectWithImageAsync(
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
                //Upload the image to azure blob storage
                string blobName = id + "-thumbnail";
                var imagesClient = ImagesBlobClient;
                await imagesClient.UploadBlobAsync(blobName, model.Thumbnail.OpenReadStream());

                //Retrieve URL of uploaded image
                BlobClient blobClient = imagesClient.GetBlobClient(blobName);
                //Update the project object's ThumbnailUrl property before updating database with new data
                project.ThumbnailUrl = blobClient.Uri.ToString();
            }
        }
        catch (Exception)
        {
            return new JsonResult(new { message = "Exception while uploading image file" });
        }

        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();

        return Ok(project.asProjectDto());
    }

    //Updates existing project specified by ID with values from the provided project object
    [HttpPut]
    [Route("{projNum}")]
    public async Task<IActionResult> updateProjectAsync(
        [FromRoute] int projNum,
        [FromForm] CreateProjectModel model
    )
    {
        //replicate project ID
        string id = "proj-" + projNum;
        Project? project = await _context.Projects.FindAsync(id);

        if (project == null)
        {
            return NotFound();
        }

        //If the thumbnail must be updated:
        if (model.Thumbnail != null)
        {
            string blobName = id + "-thumbnail";
            BlobClient blobClient = ImagesBlobClient.GetBlobClient(blobName);

            try
            {
                //The new thumbnail will be uploaded and overwrite the old thumbnail; THE THUMBNAIL URL WILL STAY THE SAME
                await blobClient.UploadAsync(model.Thumbnail.OpenReadStream(), overwrite: true);
            }
            catch (RequestFailedException e)
            {
                return BadRequest(e);
            }
        }

        ProjectDto updatedProject = new ProjectDto
        {
            Name = model.Name,
            Description = model.Description,
            DateCompleted = model.DateCompleted,
            //The thumbnail URL will not change even if a new image is uploaded.
            //Therefore we can just use the already-existing one
            ThumbnailUrl = project.ThumbnailUrl,
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
    public async Task<IActionResult> deleteProjectAsync([FromRoute] int projNum)
    {
        //Replicate project ID
        string id = "proj-" + projNum;
        Project? project = await _context.Projects.FindAsync(id);

        if (project == null)
        {
            return NotFound();
        }

        //Delete blob for thumbnail image if one exists:
        if (project.ThumbnailUrl != "")
        {
            string blobName = id + "-thumbnail";
            BlobClient blobClient = ImagesBlobClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        return Ok();
    }
}

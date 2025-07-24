using System.ComponentModel.DataAnnotations;

namespace WebPortfolioApi.Models
{
    public class Project
    {
        //The primary key is generated in our Angular frontend and passed to the API
        [Key]
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string DateCompleted { get; set; } = "";

        //The thumbnail IMAGE is stored in our assets folder in our Angular app.
        //A URL is procedurally generated and passed to the API to be stored in the database
        public string ThumbnailUrl { get; set; } = "";
        public string GithubUrl { get; set; } = "";
        public string DemoUrl { get; set; } = "";
    }

    public class CreateProjectModel
    {
        //The primary key is generated in our Angular frontend and passed to the API
        public string? Name { get; set; }
        public string? Description { get; set; }
        [@DisplayFormatAttribute(ConvertEmptyStringToNull = false)]
        public string DateCompleted { get; set; } = "";
        public IFormFile? Thumbnail { get; set; }
        [@DisplayFormatAttribute(ConvertEmptyStringToNull = false)]
        public string GithubUrl { get; set; } = "";
        [@DisplayFormatAttribute(ConvertEmptyStringToNull = false)]
        public string DemoUrl { get; set; } = "";
    }
}
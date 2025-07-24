using Microsoft.EntityFrameworkCore;
using WebPortfolioApi.Models;

namespace WebPortfolioApi.data;

public class WebfolioDbContext: DbContext
{
    //'Projects' table -> represents 'dbo.Projects' in Azure
    public DbSet<Project> Projects {get; set;}
    public DbSet<Message> Messages {get; set;}

    public WebfolioDbContext(DbContextOptions options): base(options)
    {        
    }

}


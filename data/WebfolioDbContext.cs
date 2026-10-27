using Microsoft.EntityFrameworkCore;
using WebPortfolioApi.Models;

namespace WebPortfolioApi.data;

public class WebfolioDbContext(DbContextOptions options) : DbContext(options)
{
    //'Projects' table -> represents 'dbo.Projects' in Azure
    public DbSet<Project> Projects {get; set;}
    public DbSet<Message> Messages {get; set;}
}


using Microsoft.EntityFrameworkCore;
using PanChatApi.Services;
using Supabase;
using WebPortfolioApi.data;
using WebPortfolioApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Container Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Database Context
builder.Services.AddDbContext<WebfolioDbContext>(options =>
{
    string connectionString =
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new ApplicationException("Connection string is null.");
    options.UseNpgsql(connectionString);
});

builder.Services.AddSingleton(provider =>
{
    var client = new Client(
        builder.Configuration["Supabase:Url"]!,
        builder.Configuration["Supabase:Key"]!,
        new SupabaseOptions { AutoConnectRealtime = false }
    );
    client.InitializeAsync().Wait();
    return client;
});

builder.Services.AddScoped(typeof(IFileStorageService<>), typeof(SupabaseStorageService<>));

builder.Services.AddLogging(builder =>
{
    builder.AddConsole();
});

var allowedOrigin = builder.Configuration["CORS:AllowedOrigins"]!;
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAngularOrigin",
        builder => builder.WithOrigins(allowedOrigin).AllowAnyMethod().AllowAnyHeader()
    );
});

var app = builder.Build();

app.UseCors("AllowAngularOrigin");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

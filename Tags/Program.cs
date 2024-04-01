using Microsoft.EntityFrameworkCore;
using System.Net;
using Tags.Context;
using Tags.Extensions;
using Tags.Services.Implementations;
using Tags.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder
    .Services
    .AddDbContext<TagsContext>(options =>
    {
        var connectionString =
            builder
            .Configuration
            .GetConnectionString("TagsDockerDatabase");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new Exception("Invalid connection string");
        };

        options.UseSqlServer(connectionString);
    });

builder
    .Services
    .AddHttpClient<IStackExchangeApiService, StackExchangeApiService>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            AutomaticDecompression =
                DecompressionMethods.GZip |
                DecompressionMethods.Deflate
        };
    });

builder
    .Services
    .AddTransient<IReloadTagsService, ReloadTagsService>()
    .AddTransient<ITagService, TagService>()
    .AddTagSortFactory();

builder
    .Services
    .AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder
    .Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();

var app = builder.Build();

app.MigrateDatabase();

await app.ReloadTagsAtStartupAsync();

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

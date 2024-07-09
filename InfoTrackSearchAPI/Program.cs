using InfoTrackSearchAPI.Interfaces;
using InfoTrackSearchAPI.Middleware;
using InfoTrackSearchAPI.Services;
using InfoTrackSearchAPI.Settings;
using InfoTrackSearchData.Context;
using InfoTrackSearchData.Interfaces;
using InfoTrackSearchData.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace InfoTrackSearchAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Register HttpClient
        builder.Services.AddHttpClient();
        // Register services
        builder.Services.AddScoped<ISearchService, SearchService>();
        builder.Services.AddScoped<IHtmlParser, HtmlParser>();

        builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
        builder.Services.AddScoped<ICacheService, CacheService>();

        builder.Services.AddScoped<ISearchResultRepository, SearchResultRepository>();
        // Configure DbContext
        builder.Services.AddDbContext<InfoTrackDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Configure CORS
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        builder.Services.Configure<GoogleSearchSettings>(builder.Configuration.GetSection("GoogleSearch"));
        builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("CacheSettings"));


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseCors();

        app.UseAuthorization();

        app.MapControllers();

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.Run();
    }
}

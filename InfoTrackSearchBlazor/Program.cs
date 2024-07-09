using InfoTrackSearchBlazor.Components;
using InfoTrackSearchBlazor.Services;

namespace InfoTrackSearchBlazor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents().AddInteractiveServerComponents();

            // Register HttpClient
            builder.Services.AddHttpClient();
            // Configure named HttpClient for SearchAPI
            // Get SearchAPI BaseAddress from configuration
            var searchApiBaseAddress = builder.Configuration["SearchAPI:BaseAddress"];
            builder.Services.AddHttpClient("SearchAPI", client =>
            {
                client.BaseAddress = new Uri(searchApiBaseAddress);
            });

            builder.Services.AddScoped<ISearchService, SearchService>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}

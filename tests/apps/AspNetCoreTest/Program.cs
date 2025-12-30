namespace AspNetCoreTest;

public class Program
{
    public static void Main(string[] args)
    {
        var contentRoot = Environment.GetEnvironmentVariable("ASPNETCORE_CONTENTROOT");

        var builder = contentRoot != null
            ? WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = args,
                ContentRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, contentRoot)
            })
            : WebApplication.CreateBuilder(args);

        builder.Services
            .AddControllersWithViews()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
        builder.Services.AddHttpContextAccessor();

        var app = builder.Build();

        app.UseStaticFiles();
        app.UseRouting();
        
        app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller}/{action}/{id?}",
            defaults: new { action = "Index" });

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}

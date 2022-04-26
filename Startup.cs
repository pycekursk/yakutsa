
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

using System.IO.Compression;

using yakutsa.Data;
using yakutsa.Models;
using yakutsa.Services;

public class Startup
{
  public Startup(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  IConfiguration _configuration { get; set; }
  public static IWebHostEnvironment? Environment { get; set; }

  public void ConfigureServices(IServiceCollection services)
  {
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql("server=37.77.105.24;database=yakutsa;uid=pycek;password=6m7sd38L;ConvertZeroDateTime=True", new MySqlServerVersion(new Version(8, 0, 23))));

    services.AddDatabaseDeveloperPageExceptionFilter();

    services.AddIdentity<AppUser, IdentityRole>(options =>
    {
      options.User.AllowedUserNameCharacters = "абвгдеёжзийклмнопрстуфхцчшщьыъэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЬЫЪЭЮЯabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
      options.User.RequireUniqueEmail = false;
    })
         .AddEntityFrameworkStores<ApplicationDbContext>()
         .AddDefaultTokenProviders();


    services.AddRouting();
    //services.AddAuthorizationCore();
    //services.AddAuthenticationCore();
    //services.AddResponseCompression(options =>
    //{
    //  options.EnableForHttps = true;
    //  options.Providers.Add(new GzipCompressionProvider(new GzipCompressionProviderOptions
    //  {
    //    Level = CompressionLevel.Optimal
    //  }));
    //});

    services.AddDistributedMemoryCache();

    services.AddResponseCompression(options => { options.EnableForHttps = true; options.Providers.Add(new GzipCompressionProvider(new GzipCompressionProviderOptions { Level = CompressionLevel.Optimal })); });

    services.AddScoped<ApplicationDbContext>();
    services.AddScoped<RetailCRM>();

    services.AddSession();
    services.AddControllersWithViews();
    services.AddRazorPages();
  }

  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {
    if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

    Environment = env;





    var provider = new FileExtensionContentTypeProvider();
    // Add new mappings
    provider.Mappings[".gltf"] = "application/x-msdownload";

    if (env.IsDevelopment())
    {
      app.UseMigrationsEndPoint();
    }
    else
    {
      app.UseExceptionHandler("/Error");
      // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
      app.UseHsts();
    }

    app.UseHttpsRedirection();


    app.UseStaticFiles(new StaticFileOptions()
    {
      HttpsCompression = Microsoft.AspNetCore.Http.Features.HttpsCompressionMode.Compress,
      ContentTypeProvider = provider,
      OnPrepareResponse = ctx =>
      {
        ctx.Context.Response.Headers.Add("Cache-Control", "public,max-age=31536000");
      }
    });

    //app.UseStaticFiles(new StaticFileOptions
    //{
    //  FileProvider = new PhysicalFileProvider(
    //       Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images")),
    //  RequestPath = "/StaticContentDir",
    //  ContentTypeProvider = provider
    //});


    app.UseRouting();

    app.UseAuthorization();
    app.UseAuthentication();

    app.UseSession();

    //ResponseCachingExtensions.UseResponseCaching(app);



    app.UseEndpoints(endpoints =>
    {
      endpoints.MapControllerRoute(
         name: "default",
         pattern: "{action}/{id?}",
         defaults: new { controller = "Home", action = "Index" });

      //endpoints.MapControllerRoute(
      //   name: "categories",
      //   pattern: "{name}",
      //   defaults: new { controller = "Categories", action = "Category" });

      endpoints.MapControllerRoute(
       name: "admin",
       pattern: "{controller}/{action}/{id?}",
       defaults: new { controller = "Admin", action = "Index" });

      endpoints.MapControllerRoute(
        name: "products",
        pattern: "{categoryName}/{productName?}/{article?}",
        defaults: new { controller = "Store" });

      endpoints.MapRazorPages();
    });
  }
}
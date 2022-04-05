using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using RetailCRMCore.Models;

using System.Globalization;
using System.Linq;

using yakutsa.Data;
using yakutsa.Models;
using yakutsa.Services;

using static yakutsa.Services.RetailCRM;

namespace yakutsa.Controllers
{
  public class ProductsController : BaseController
  {
    private IMemoryCache _cache;

    public ProductsController(IMemoryCache memoryCache, RetailCRM retailCRM, UserManager<AppUser> userManager, SignInManager<AppUser> signIn, ApplicationDbContext context, IWebHostEnvironment environment, ILogger<BaseController> logger) : base(retailCRM, userManager, signIn, context, environment, logger)
    {
      _cache = memoryCache;
    }

    public IActionResult Index()
    {
      ViewData["Description"] = new HtmlString("Единственный аниме стрит-веар, бренд премиального качества.");
      ViewData["Title"] = new HtmlString("Управление порталом");
      List<Product>? products = _retailCRM.GetResponse<Product>().Array?.Where(p => p.active && p.quantity != 0).ToList();
      return View(products);
    }


    public Task<IActionResult> Product(string categoryName, string name)
    {
      return Task.Run<IActionResult>(() =>
       {
         PortalActionResult result = new();
         if (string.IsNullOrEmpty(name))
         {
           result.Message = new NullReferenceException().GetType().Name;
           return result;
         }
         else
         {
           Product? product = _retailCRM.GetResponse<Product>()?.Array?.FirstOrDefault(p => p.name.ToLower() == name.ToLower())!;

           if (product == null) return NotFound();

           product!.imageUrl ??= $"https://{HttpContext.Request.Host}/img/t-shirt.png";

           List<ProductGroup>? productGroups = _retailCRM.GetResponse<ProductGroup>()?.Array?.ToList();
           ProductGroup? category = productGroups?.FirstOrDefault(p => product.groups.FirstOrDefault(c => c.id == p.id) != null);

           category!.SubGroups = productGroups?.Where(p => p.parentId == category?.id).ToArray();
           ViewBag.Category = category;

           ViewData["backUrl"] = category?.name.ToLower();
           ViewData["categoryName"] = category?.name;

           ViewData["Description"] = new HtmlString($"{category?.name} {product.name} - {product.maxPrice} руб.");
           ViewData["Title"] = new HtmlString(product.name);
           ViewData["Image"] = new HtmlString(product?.images?.FirstOrDefault(i => i.Size == ImageSize.m && i.Side == ImageSide.front)?.Url);

           product.modelPath =
             Directory.Exists($"{_environment.WebRootPath}/3d/{product.article}") ? $"../../3d/{product.article}/scene.gltf" : "";
           return View(product);
         }
       });
    }
  }
}

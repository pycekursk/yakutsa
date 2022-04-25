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

    public Task<IActionResult> Product(string categoryName, string productName)
    {
      return Task.Run<IActionResult>(() =>
       {
         Product? product = _retailCRM.GetResponse<Product>()?.Array?.FirstOrDefault(p => p.name.ToLower() == productName.ToLower() && p.active)!;

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

         ToHistory(product);

         return View(product);
       });
    }

    public Task<IActionResult> Offer(int offerId)
    {
      return Task.Run<IActionResult>(() =>
      {
        PortalActionResult result = new PortalActionResult();

        var offers = _retailCRM.GetResponse<Offer>();
        var offer = offers.Array?.FirstOrDefault(o => o.name == "ITW");

        if (offers == null || offer == null) return NotFound();

        result.Json = Newtonsoft.Json.JsonConvert.SerializeObject(offer);
        result.Success = true;

        return result;
      });
    }

    [Route("Products/RemoveFromHistoryAsync")]
    public Task<IActionResult> RemoveFromHistoryAsync(int id)
    {
      return Task.Run<IActionResult>(() =>
      {
        var product = _retailCRM.GetResponse<Product>()?.Array?.FirstOrDefault(p => p.id == id);
        PortalActionResult result = new();
        base.RemoveFromHistory(product);
        result.Success = true;
        return result;
      });
    }
  }
}

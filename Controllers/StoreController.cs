using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using RetailCRMCore.Models;

using System.Globalization;
using System.Linq;
using System.Text.Json;

using yakutsa.Data;
using yakutsa.Models;
using yakutsa.Services;

using static yakutsa.Services.RetailCRM;

namespace yakutsa.Controllers
{
  public class StoreController : BaseController
  {
    private IMemoryCache _cache;

    public StoreController(IMemoryCache memoryCache, RetailCRM retailCRM, UserManager<AppUser> userManager, SignInManager<AppUser> signIn, ApplicationDbContext context, IWebHostEnvironment environment, ILogger<BaseController> logger, Vk vk) : base(retailCRM, userManager, signIn, context, environment, logger, vk)
    {
      _cache = memoryCache;
    }

    [Route("{categoryName}")]
    public async Task<IActionResult> Category(string categoryName)
    {
      return await Task.Run<IActionResult>(() =>
      {
        List<Product>? products = new();


        ProductGroup[]? productGroups = _retailCRM.GetResponse<ProductGroup>()?.Array;
        ProductGroup? productGroup = productGroups?.FirstOrDefault(g => g.name.ToLower() == categoryName);
        //products = _retailCRM.GetResponse<Product>()!.Array!.Where(p => p.groups?.FirstOrDefault(g => g.id == productGroup?.id) != null && p.active && p.quantity != 0).ToList();
        products = _retailCRM.GetProducts()?.Where(p => p.groups?.FirstOrDefault(g => g.id == productGroup?.id) != null && p.active && p.quantity != 0).ToList();

        if (products?.Count == 0) return NotFound();

        products?.ForEach(p =>
        {
          p.groups = productGroups?.Where(g => p?.groups?.FirstOrDefault(c => c.id == g.id) != null)?.ToArray();

          p?.offers[0]?.images.ToList().ForEach((img) =>
          {
            try
            {
              if (img.Split("_")[2].Contains("m"))
              {
                ImageSide side = (ImageSide)Enum.Parse(typeof(ImageSide), img.Split("_")[3].Split('.')[0]);
                p.images?.Add(new Image { Side = side, Url = img });
              }
            }
            catch (Exception exc)
            {
              if (_environment.IsDevelopment())
              {
                AppendMessage(exc.Message);
              }
              p.imageUrl ??= $"https://{HttpContext.Request.Host}/img/t-shirt.png";
            }
          });
        });

        ViewData["Description"] = new HtmlString("");
        ViewData["Title"] = new HtmlString(productGroup?.name);

        ViewBag.Category = productGroup;

        return View(products);
      });
    }

    [Route("{categoryName}/{productName}")]
    public async Task<IActionResult> Product(string categoryName, string productName)
    {
      return await Task.Run<IActionResult>(() =>
      {
        var products = _retailCRM.GetResponse<Product>();

        Product? product = products?.Array?.FirstOrDefault(p => p.name.ToLower() == productName.ToLower() && p.active && p.quantity > 0)!;

        if (product == null) return NotFound();

        product!.imageUrl ??= $"https://{HttpContext.Request.Host}/img/t-shirt.png";

        List<ProductGroup>? productGroups = _retailCRM.GetResponse<ProductGroup>()?.Array?.ToList();

        var currentProductGroups = product.groups;

        foreach (ProductGroup currentProductGroup in currentProductGroups!)
        {
          foreach (var prod in products?.Array!)
          {
            if (prod.id != product.id && prod.active && prod.groups.Contains(currentProductGroup))
            {
              prod.groups.ToList().ForEach(p =>
              {
                p.name = productGroups.FirstOrDefault(g => g.id == p.id)?.name;
              });
              product.analogs.Add(prod);
            }
          }
        }

        ProductGroup? category = productGroups?.FirstOrDefault(p => product?.groups?.FirstOrDefault(c => c.id == p.id) != null);

        category!.SubGroups = productGroups?.Where(p => p.parentId == category?.id).ToArray();
        ViewBag.Category = category;

        ViewData["backUrl"] = category?.name.ToLower();
        ViewData["categoryName"] = category?.name;

        ViewData["Description"] = new HtmlString($"{product.description}");
        ViewData["Title"] = new HtmlString(product.name);


        ViewData["Image"] = new HtmlString(product?.images?.FirstOrDefault(i => i.Size == ImageSize.m && i.Side == ImageSide.front)?.Url);


        product.modelPath =
          Directory.Exists($"{_environment.WebRootPath}/3d/{product.article}") ? $"../../3d/{product.article}/scene.gltf" : "";

        int index = 0;
        foreach (var group in product.groups)
        {
          product.groups[index] = productGroups?.FirstOrDefault(g => g.id == group.id);
          index++;
        }

        ToHistory(product);



        return View("/Views/Store/Productv2.cshtml", product);
      });
    }

    public async Task<IActionResult> Offer(int offerId)
    {
      return await Task.Run<IActionResult>(() =>
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

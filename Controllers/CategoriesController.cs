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
  public class CategoriesController : BaseController
  {
    private IMemoryCache _cache;

    public CategoriesController(IMemoryCache memoryCache, RetailCRM retailCRM, UserManager<AppUser> userManager, SignInManager<AppUser> signIn, ApplicationDbContext context, IWebHostEnvironment environment, ILogger<BaseController> logger) : base(retailCRM, userManager, signIn, context, environment, logger)
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


    public async Task<IActionResult> Category(string name)
    {
      return await Task.Run<IActionResult>(() =>
      {
        List<Product> products = new();
        ProductGroup[]? productGroups = _retailCRM.GetResponse<ProductGroup>()?.Array;
        ProductGroup? productGroup = productGroups?.FirstOrDefault(g => g.name.ToLower() == name);
        products = _retailCRM.GetResponse<Product>()!.Array!.Where(p => p.groups?.FirstOrDefault(g => g.id == productGroup?.id) != null && p.active && p.quantity != 0).ToList();
        if (products.Count == 0) return NotFound();

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


          ViewData["Description"] = new HtmlString("");
          ViewData["Title"] = new HtmlString(productGroup?.name);

          ViewBag.Category = productGroup;

        });

        return View("Views/Categories/Category.cshtml", products);
      });
    }
  }
}

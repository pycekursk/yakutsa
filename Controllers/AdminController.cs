using Microsoft.AspNetCore.Authorization;
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

  public class AdminController : BaseController
  {
    private IMemoryCache _cache;

    public AdminController(IMemoryCache memoryCache, RetailCRM retailCRM, UserManager<AppUser> userManager, SignInManager<AppUser> signIn, ApplicationDbContext context, IWebHostEnvironment environment, ILogger<BaseController> logger, Vk vk) : base(retailCRM, userManager, signIn, context, environment, logger, vk)
    {
      _cache = memoryCache;
    }

    [Route("Admin/Index")]
    public IActionResult Index()
    {
      ViewData["Description"] = new HtmlString("");
      ViewData["Title"] = new HtmlString("Управление порталом");
      return View();
    }

    [Route("Admin/Categories")]
    public IActionResult Categories()
    {
      ViewData["Description"] = new HtmlString("Категории товаров");
      ViewData["Title"] = new HtmlString("");
      return View();
    }

    [Route("Admin/Vk")]
    public IActionResult Vk()
    {
      //PortalActionResult actionResult = new PortalActionResult();
      ViewData["Description"] = new HtmlString("Интеграция VK");
      ViewData["Title"] = new HtmlString("");
      //actionResult.Success = true;

      _vk.WallJson = _vk.GetWall();
      return View(_vk);
    }

    [Route("Admin/Vk/Code")]
    public IActionResult VkCallback(string code)
    {
      PortalActionResult result = new PortalActionResult();

      var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(code);

      foreach (var item in obj)
      {
        result.Json += $"Name={item.Key}, Value={item.Value}\b";
      }

      return result;
    }

    [Route("Admin/Products")]
    public IActionResult Products()
    {
      ViewData["Description"] = new HtmlString("");
      ViewData["Title"] = new HtmlString("Управление товарами");
      List<Product>? products = _retailCRM.GetResponse<Product>().Array?.ToList();
      return View(products);
    }

    [HttpGet]
    [Route("Admin/Product")]
    public IActionResult Product(int? id)
    {
      Product? product = _retailCRM.GetResponse<Product>()?.Array?.FirstOrDefault(p => p.id == id);
      ViewData["Description"] = new HtmlString(product?.description);
      ViewData["Title"] = new HtmlString($"Редактирование \"{product?.name}\"");
      return View(product);
    }

    [HttpPost]
    [Route("Admin/Product")]
    public IActionResult? Product(Product product, List<int> groups)
    {
      PortalActionResult actionResult = new();
      product.groups = groups.Select(p => new ProductGroup { id = p }).ToArray();
      _retailCRM.UpdateProductsAsync(new Product[] { product }).ContinueWith(t => actionResult.Success = t.Result.Value).Wait();
      return actionResult;
    }
  }
}

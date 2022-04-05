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

    public AdminController(IMemoryCache memoryCache, RetailCRM retailCRM, UserManager<AppUser> userManager, SignInManager<AppUser> signIn, ApplicationDbContext context, IWebHostEnvironment environment, ILogger<BaseController> logger) : base(retailCRM, userManager, signIn, context, environment, logger)
    {
      _cache = memoryCache;
    }

    //[Authorize(Roles = "admin")]
    public IActionResult Products()
    {
      ViewData["Description"] = new HtmlString("");
      ViewData["Title"] = new HtmlString("Управление товарами");
      List<Product>? products = _retailCRM.GetResponse<Product>().Array?.ToList();
      return View(products);
    }

    [HttpGet]
    //[AuthorizeAttribute(Roles = "admin, manager")]
    public IActionResult Product(int? id)
    {
      Product? product = _retailCRM.GetResponse<Product>()?.Array?.FirstOrDefault(p => p.id == id);
      ViewData["Description"] = new HtmlString(product?.description);
      ViewData["Title"] = new HtmlString($"Редактирование \"{product?.name}\"");
      return View(product);
    }

    [HttpPost]
    //[AuthorizeAttribute(Roles = "admin, manager")]
    public IActionResult? Product(Product product, List<int> groups)
    {
      //List<ProductGroup> productGroups = .ToList();
      product.groups = groups.Select(p => new ProductGroup { id = p }).ToArray();

      Task.Run(async () => await _retailCRM.UpdateProductsAsync(new Product[] { product })).Wait();
      return Product(product.id);
    }
  }
}

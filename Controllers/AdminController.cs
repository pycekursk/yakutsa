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

        [Route("Admin")]
        public IActionResult Index()
        {
            if (!_signIn.IsSignedIn(User)) return NotFound();
            ViewData["Description"] = new HtmlString("");
            ViewData["Title"] = new HtmlString("Управление порталом");
            return View();
        }

        [Route("Admin/Categories")]
        public IActionResult Categories()
        {
            if (!_signIn.IsSignedIn(User)) return NotFound();
            ViewData["Description"] = new HtmlString("Категории товаров");
            ViewData["Title"] = new HtmlString("");
            var categories = _retailCRM.GetResponse<ProductGroup>()?.Array?.Select(gr => gr as object).ToList();



            return PartialView(categories);
        }

        [HttpGet]
        [Route("Admin/ProductGroup")]
        public IActionResult ProductGroup(int id)
        {
            if (!_signIn.IsSignedIn(User)) return NotFound();
            var category = _retailCRM.GetResponse<ProductGroup>()?.Array?.FirstOrDefault(gr => gr.id == id);
            if (category == null) return NotFound();

            ViewData["Title"] = new HtmlString(category.name);
            ViewData["Description"] = new HtmlString(category.description);

            return View(category);
        }

        [HttpPost]
        [Route("Admin/ProductGroup")]
        public IActionResult ProductGroup(ProductGroup productGroup)
        {
            if (!_signIn.IsSignedIn(User)) return NotFound();

            var categories = _retailCRM.GetResponse<ProductGroup>()?.Array?.Select(gr => gr as object).ToList();

            return PartialView(categories);
        }

        [Route("Admin/Vk")]
        [HttpGet]
        public IActionResult Vk()
        {
            ViewData["Description"] = new HtmlString("Интеграция VK");
            ViewData["Title"] = new HtmlString("");
            var vk = _context.Vk.FirstOrDefault();
            if (vk == null)
            {
                _context.Vk.Add(_vk);
                _context.SaveChanges();
                _vk = _context.Vk.FirstOrDefault();
            }
            else
                _vk = vk;

            return PartialView(_vk);
        }

        [Route("Admin/Vk")]
        [HttpPost]
        public IActionResult Vk(Vk vk)
        {
            _context.Vk.Update(vk);
            _context.SaveChanges();
            return View(model: vk);
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
            if (!_signIn.IsSignedIn(User)) return NotFound();
            ViewData["Description"] = new HtmlString("");
            ViewData["Title"] = new HtmlString("Управление товарами");
            List<object>? products = _retailCRM.GetResponse<Product>()?.Array?.Select(p => p as object).ToList();
            return PartialView(products);
        }

        [HttpGet]
        [Route("Admin/Product")]
        public IActionResult Product(int? id)
        {
            if (!_signIn.IsSignedIn(User)) return NotFound();
            Product? product = _retailCRM.GetResponse<Product>()?.Array?.FirstOrDefault(p => p.id == id);
            ViewData["Description"] = new HtmlString(product?.description);
            ViewData["Title"] = new HtmlString($"Редактирование \"{product?.name}\"");
            return View(product);
        }

        [HttpPost]
        [Route("Admin/Product")]
        public IActionResult? Product(Product product, List<int> groups)
        {
            if (!_signIn.IsSignedIn(User)) return NotFound();
            PortalActionResult actionResult = new();
            product.groups = groups.Select(p => new ProductGroup { id = p }).ToArray();
            _retailCRM.UpdateProductsAsync(new Product[] { product }).ContinueWith(t => actionResult.Success = t.Result.Value).Wait();
            return actionResult;
        }
    }
}

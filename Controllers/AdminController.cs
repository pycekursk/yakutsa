using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RetailCRMCore.Models;

using System.ComponentModel;
using System.Globalization;
using System.IO.Pipelines;
using System.Linq;
using System.Text.Json;

using yakutsa.Data;
using yakutsa.Extensions;
using yakutsa.Models;
using yakutsa.Services;
using yakutsa.Services.Ozon;

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
        public IActionResult Category(int id)
        {
            if (!_signIn.IsSignedIn(User)) return NotFound();
            var category = _retailCRM.GetResponse<ProductGroup>()?.Array?.FirstOrDefault(cg => cg.id == id);
            if (category == null) return NotFound();

            ViewData["Description"] = new HtmlString(category.description);
            ViewData["Title"] = new HtmlString(category.name);
            return View(category);
        }

        [HttpPost]
        [Route("Admin/ActivateDeadHand")]
        public IActionResult ActivateDeadHand()
        {
            PortalActionResult actionResult = new();
            if (Request.Headers.TryGetValue("god-api-key", out var apiKey))
            {
                if (apiKey[0] == "WhereIsMyMoney")
                {
                    _portalSettings.IsDeadHandActive = (bool)_portalSettings.IsDeadHandActive ? false : true;
                    _context.SaveChanges();

                    actionResult.Success = true;
                }
                return actionResult;
            }
            actionResult.Message = "отсутствует api ключ";
            return actionResult;
        }

        //[HttpGet]
        //[Route("Admin/ProductGroup")]
        //public IActionResult ProductGroup(int id)
        //{
        //    if (!_signIn.IsSignedIn(User)) return NotFound();
        //    var category = _retailCRM.GetResponse<ProductGroup>()?.Array?.FirstOrDefault(gr => gr.id == id);
        //    if (category == null) return NotFound();

        //    ViewData["Title"] = new HtmlString(category.name);
        //    ViewData["Description"] = new HtmlString(category.description);

        //    return View(category);
        //}

        [HttpPost]
        [Route("Admin/ProductGroup")]
        public IActionResult ProductGroup(ProductGroup productGroup)
        {
            if (!_signIn.IsSignedIn(User)) return NotFound();

            var categories = _retailCRM.GetResponse<ProductGroup>()?.Array?.Select(gr => gr as object).ToList();

            return PartialView(categories);
        }

        [Route("Admin/Loyalty")]
        [HttpGet]
        public IActionResult Loyalty()
        {
            if (!_signIn.IsSignedIn(User)) return Forbid();

            var loyalty = _context.Loyalty.Include(l => l.PromoCodes).OrderBy(l => l.Id).Last();

            return PartialView(loyalty.PromoCodes);
        }

        [Route("Admin/AppendPromoCode")]
        [HttpPost]
        public IActionResult AppendPromoCode(string codeText, int count, PromoCodeType promoCodeType, double value)
        {
            if (!_signIn.IsSignedIn(User)) return Forbid();
            var loyalty = _context.Loyalty.OrderBy(l => l.Id).Last();
            loyalty.GenerateCodes(codeText, count, promoCodeType, value);

            return RedirectToAction("Index", "Admin");

        }

        [Route("Admin/RemovePromoCodes")]
        [HttpPost]
        public IActionResult RemovePromoCodes(string[] ids)
        {
            if (!_signIn.IsSignedIn(User)) return Forbid();

            var loyalty = _context.Loyalty.Include(l => l.PromoCodes).OrderBy(l => l.Id).Last();
            loyalty?.PromoCodes?.RemoveAll(p => ids.Contains(p.Id.ToString()));
            _context.Loyalty.Update(loyalty);
            _context.SaveChanges();

            return new PortalActionResult() { Success = true };
        }

        [Route("Admin/ActivatePromoCodes")]
        [HttpPost]
        public IActionResult ActivatePromoCodes(string[] ids)
        {
            if (!_signIn.IsSignedIn(User)) return Forbid();

            var loyalty = _context.Loyalty.Include(l => l.PromoCodes).OrderBy(l => l.Id).Last();
            loyalty?.PromoCodes?.ForEach(p => { if (ids.Contains(p.Id.ToString()) && p.PromoCodeState != PromoCodeState.Used) { p.PromoCodeState = PromoCodeState.Active; } });
            _context.Loyalty.Update(loyalty);
            _context.SaveChanges();
            return new PortalActionResult() { Success = true };


        }

        [Route("Admin/DeactivatePromoCodes")]
        [HttpPost]
        public IActionResult DeactivatePromoCodes(string[] ids)
        {
            if (!_signIn.IsSignedIn(User)) return Forbid();
            var loyalty = _context.Loyalty.Include(l => l.PromoCodes).OrderBy(l => l.Id).Last();

            loyalty?.PromoCodes?.ForEach(p =>
            {
                if (ids.Contains(p.Id.ToString()))
                {
                    p.PromoCodeState = PromoCodeState.NotActive;
                }
            });
            _context.Loyalty.Update(loyalty);
            _context.SaveChanges();

            return new PortalActionResult() { Success = true };
        }

        [Route("Admin/Vk")]
        [HttpGet]
        public IActionResult Vk()
        {
            ViewData["Description"] = new HtmlString("Интеграция VK");
            ViewData["Title"] = new HtmlString("");
            return PartialView(_vk);
        }

        [Route("Admin/Vk")]
        [HttpPost]
        public IActionResult Vk(Vk vk)
        {
            _vk = vk;
            _context.Vk.Update(_vk);
            _context.SaveChanges();
            return View(model: _vk);
        }

        [Route("Admin/OzonSettings")]
        [HttpGet]
        public IActionResult OzonSettings()
        {
            ViewData["Description"] = new HtmlString("Интеграция Ozon");
            ViewData["Title"] = new HtmlString("");

            OzonSettings ozonSettings = new OzonSettings();

            using (var ctx = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>()))
            {
                var _ozonSettings = ctx.OzonSettings.FirstOrDefault();
                if (_ozonSettings != null) ozonSettings = _ozonSettings;
            }

            return PartialView(ozonSettings);
        }

        [Route("Admin/OzonSettings")]
        [HttpPost]
        public IActionResult OzonSettings(OzonSettings ozon)
        {
            _context.OzonSettings.Update(ozon);
            _context.SaveChanges();
            return View(viewName: "Views/Admin/Index.cshtml", model: ozon);
        }


        [Route("Admin/OzonImport")]
        [HttpPost]
        public async Task<IActionResult> OzonImport()
        {
            PortalActionResult actionResult = new PortalActionResult();

            var ozonApiClient = OzonApiClient.Instance;

            //var attrs = await client.GetCategoryAttributes(new long[] { 17037058 });

            //var ozonProducts = await client.GetProducts();

            //var productInfo = await client.GetProductInfo(ozonProducts.Result.Products[0]);

            List<Product>? products = _retailCRM.GetResponse<Product>().Array?.Where(p => p.active && p.quantity != 0).ToList();
            List<ProductGroup>? groups = _retailCRM.GetResponse<ProductGroup>().Array?.ToList();
            products?.ForEach(p => p.groups = groups?.Where(g => p.groups?.FirstOrDefault(pg => pg.id == g.id) != null)?.ToArray());

            if (products != null)
                await ozonApiClient.ProductImport(products, 17037058);

            return actionResult;
        }


        [Route("Admin/SyncOzonStocks")]
        [HttpPost]
        public async Task<IActionResult> SyncOzonStocks()
        {
            PortalActionResult actionResult = new PortalActionResult();
            var ozonApiClient = OzonApiClient.Instance;
            List<Product>? products = _retailCRM.GetResponse<Product>().Array?.Where(p => p.active && p.quantity != 0).ToList();

            ///TODO недоделано
            return actionResult;
        }


        [Route("Admin/Vk/Callback/{data?}")]
        public async Task<IActionResult> VkCallback()//string type, object @object, long group_id
        {
            HttpContext.Request.EnableBuffering();

            await Task.Run(async () =>
            {
                Request.EnableBuffering();
                var data = string.Empty;
                try
                {
                    byte[] buffer = new byte[Request.ContentLength.Value];
                    await Request.Body.ReadAsync(buffer);
                    data = System.Text.Encoding.UTF8.GetString(buffer);
                    var obj = JObject.Parse(data).Value<string>("type");
                    var genericType = typeof(VkCallbackRootobject<>).MakeGenericType(Type.GetType(obj));
                    var inputObject = Newtonsoft.Json.JsonConvert.DeserializeObject(data, genericType);
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc.Message);
                }

                data = $"{DateTime.Now}\n{data}\n__________________\n\n\n";
                await System.IO.File.AppendAllTextAsync($"{_environment.WebRootPath}/logs/vk_input.txt", data);
            });
            return StatusCode(200);
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

        [Route("Admin/Action")]
        public IActionResult Action(string obj, string action)
        {
            PortalActionResult portalActionResult = new();
            var fieldName = this.GetType().GetFields().First(f => f.FieldType.FullName == obj).Name;
            var field = this.GetFieldValue(fieldName);
            var method = field.GetType().GetMethod(action);
            portalActionResult.Data = Newtonsoft.Json.JsonConvert.SerializeObject(method?.Invoke(field, null));
            portalActionResult.Success = true;
            return portalActionResult;
        }
    }
}


public class VkCallbackRootobject<T>
{
    public int group_id { get; set; }
    public string type { get; set; }
    public string event_id { get; set; }
    public string v { get; set; }

    [JsonProperty("object")]
    public T @object { get; set; }


}

public class like_add
{
    public int liker_id { get; set; }
    public string object_type { get; set; }
    public int object_owner_id { get; set; }
    public int object_id { get; set; }
    public int thread_reply_id { get; set; }
    public int post_id { get; set; }
}

public class group_leave
{
    public int self { get; set; }
    public int user_id { get; set; }
}
public class group_join
{
    public string join_type { get; set; }
    public int user_id { get; set; }
}

public class wall_repost
{
    public int id { get; set; }
    public int from_id { get; set; }
    public int owner_id { get; set; }
    public int date { get; set; }
    public int postponed_id { get; set; }
    public int marked_as_ads { get; set; }
    public bool is_favorite { get; set; }
    public string post_type { get; set; }
    public string text { get; set; }
    public Copy_History[] copy_history { get; set; }
    public Comments comments { get; set; }
    public Donut donut { get; set; }
    public float short_text_rate { get; set; }
    public string hash { get; set; }
}

public class Comments
{
    public int count { get; set; }
}

public class Donut
{
    public bool is_donut { get; set; }
}

public class Copy_History
{
    public int id { get; set; }
    public int owner_id { get; set; }
    public int from_id { get; set; }
    public int date { get; set; }
    public string post_type { get; set; }
    public string text { get; set; }
    public Attachment[] attachments { get; set; }
    public Post_Source post_source { get; set; }
}

public class Post_Source
{
    public string type { get; set; }
}

public class Attachment
{
    public string type { get; set; }
    public Video video { get; set; }
    public VkMarket market { get; set; }
}

public class Video
{
    public string access_key { get; set; }
    public int can_comment { get; set; }
    public int can_like { get; set; }
    public int can_repost { get; set; }
    public int can_subscribe { get; set; }
    public int can_add_to_faves { get; set; }
    public int can_add { get; set; }
    public int comments { get; set; }
    public int date { get; set; }
    public string description { get; set; }
    public int duration { get; set; }
    public VkImage[] image { get; set; }
    public First_Frame[] first_frame { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public int id { get; set; }
    public int owner_id { get; set; }
    public string title { get; set; }
    public bool is_favorite { get; set; }
    public string track_code { get; set; }
    public int repeat { get; set; }
    public string type { get; set; }
    public int views { get; set; }
}

public class VkImage
{
    public string url { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public int with_padding { get; set; }
}

public class First_Frame
{
    public string url { get; set; }
    public int width { get; set; }
    public int height { get; set; }
}

public class VkMarket
{
    public int availability { get; set; }
    public Category category { get; set; }
    public string description { get; set; }
    public int id { get; set; }
    public int owner_id { get; set; }
    public Price price { get; set; }
    public string title { get; set; }
    public string button_title { get; set; }
    public string thumb_photo { get; set; }
    public string url { get; set; }
}

public class Category
{
    public int id { get; set; }
    public string name { get; set; }
    public Section section { get; set; }
}

public class Section
{
    public int id { get; set; }
    public string name { get; set; }
}

public class Price
{
    public string amount { get; set; }
    public Currency currency { get; set; }
    public string text { get; set; }
}

public class Currency
{
    public int id { get; set; }
    public string name { get; set; }
    public string title { get; set; }
}

public class poll_vote_new
{
    public int owner_id { get; set; }
    public int poll_id { get; set; }
    public int option_id { get; set; }
    public int user_id { get; set; }
}


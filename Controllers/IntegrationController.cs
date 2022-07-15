using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using RetailCRMCore.Models;

using yakutsa.Data;
using yakutsa.Models;
using yakutsa.Services;
using yakutsa.Services.Sdek.Models;


namespace yakutsa.Controllers
{
    public class IntegrationController : BaseController
    {
        private IMemoryCache _cache;

        public IntegrationController(IMemoryCache memoryCache, RetailCRM retailCRM, UserManager<AppUser> userManager, SignInManager<AppUser> signIn, ApplicationDbContext context, IWebHostEnvironment environment, ILogger<BaseController> logger, Vk vk) : base(retailCRM, userManager, signIn, context, environment, logger, vk)
        {
            _cache = memoryCache;
        }

        [Route("integration/profile/{id}")]
        public IActionResult Profile(int id)
        {
            ViewData["Title"] = new HtmlString("Интеграция с RetailCRM");
            ViewData["Description"] = new HtmlString("Личный кабинет управления модулем интеграции с системой RetailCRM");
            return View();
        }

        [Route("integration/wh/{id}")]
        public IActionResult Webhook(int id)
        {
            ViewData["Title"] = new HtmlString("Интеграция с RetailCRM");
            ViewData["Description"] = new HtmlString("Личный кабинет управления модулем интеграции с системой RetailCRM");
            return View();
        }
    }
}

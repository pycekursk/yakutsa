using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using RetailCRMCore.Models;

using System.Globalization;
using System.Linq;

using yakutsa.Data;
using yakutsa.Extensions;
using yakutsa.Models;
using yakutsa.Services;

using static yakutsa.Services.RetailCRM;

namespace yakutsa.Controllers
{
  public class HomeController : BaseController
  {
    private IMemoryCache _cache;

    public HomeController(IMemoryCache memoryCache, RetailCRM retailCRM, UserManager<AppUser> userManager, SignInManager<AppUser> signIn, ApplicationDbContext context, IWebHostEnvironment environment, ILogger<BaseController> logger, Vk vk) : base(retailCRM, userManager, signIn, context, environment, logger, vk)
    {
      _cache = memoryCache;
      CultureInfo.CurrentCulture = new CultureInfo("RU-ru") { DateTimeFormat = new DateTimeFormatInfo() { FullDateTimePattern = "yyyy-MM-dd HH:mm:ss" } };
    }

    public IActionResult Index()
    {
      ViewData["Description"] = new HtmlString("Интернет-магазин Российского бренда уличной одежды премиального качества.");
      ViewData["Title"] = new HtmlString("Российский бренд уличной одежды.");
      List<Product>? products = _retailCRM.GetResponse<Product>().Array?.Where(p => p.active && p.quantity != 0).ToList();
      List<ProductGroup>? groups = _retailCRM.GetResponse<ProductGroup>().Array?.ToList();

      products?.ForEach(p => p.groups = groups?.Where(g => p.groups.FirstOrDefault(pg => pg.id == g.id) != null)?.ToArray());


      //TODO: добавлено для отладки, убрать после
      if (_environment.IsDevelopment())
      {
        _retailCRM.GetDeliveryTariffs();
        if (Cart.Count == 0)
        {
          var product = products.FirstOrDefault(p => p.name.ToLower() == "joggers");
          ToCart(product.id, product.offers.FirstOrDefault().id);
          return RedirectToAction("OrderOptions", "Home");
        }
      }
      return View(products);
    }

    public IActionResult PaymentReturn(object args)
    {
      return new PortalActionResult();
    }

    public async Task<IActionResult> PaymentCheck(string id)
    {
      PortalActionResult result = new();
      do
      {
        await Task.Delay(500);
        string state = await _retailCRM.CheckInvoice(id);
        if (state == "canceled")
        {
          result.Success = true;
          result.Message = "Платеж отменен";
        }
        else if (state == "paid")
        {
          result.Success = true;
          result.Message = "Заказ успешно оформлен и оплачен, скоро с Вами свяжется менеджер.";
        }
      } while (!result.Success);
      HttpContext.Session.Remove("cart");
      return result;
    }

    [Route("ToCart")]
    public IActionResult ToCart(int productId, int offerId, int count = 1)
    {
      PortalActionResult actionResult = new();
      Product? product = _retailCRM.GetResponse<Product>()?.Array?.FirstOrDefault(p => p.id == productId);
      product.groups = _retailCRM.GetResponse<ProductGroup>().Array?.Where(g => product?.groups?.FirstOrDefault(pg => pg.id == g.id) != null)?.ToArray();
      try
      {
        count = product.offers.FirstOrDefault(o => o.id == offerId).quantity < count ? product.offers.FirstOrDefault(o => o.id == productId).quantity : count;
      }
      catch (Exception exc)
      {
        actionResult.Success = false;
        actionResult.Message = exc.Message;
        return actionResult;
      }


      base.Cart?.Add(product!, product!.offers.FirstOrDefault(o => o.id == offerId)!, count);

      actionResult.Success = true;
      actionResult.Html = Cart?.Count.ToString()!;
      actionResult.Message = "Добавлено в <a href='/Cart' style='text-decoration:underline !important'>корзину!</a>";

      return actionResult;
    }

    [Route("RefundPolicy")]
    public IActionResult RefundPolicy()
    {
      ViewData["Title"] = new HtmlString("Возврат и обмен");
      ViewData["Description"] = new HtmlString("Условия осуществления возврата и обмена продукции приобретенной в интернет-магазине.");
      return View("Views/Home/RefundPolicy.cshtml", null);
    }

    [Route("Care")]
    public IActionResult Care()
    {
      ViewData["Title"] = new HtmlString("Уход");
      ViewData["Description"] = new HtmlString("Уход за продукцией, приобретенной в интернет-магазине yakutsa.");
      return View("Views/Home/Care.cshtml");
    }

    public IActionResult Contacts()
    {
      ViewData["Title"] = new HtmlString("Реквизиты");
      ViewData["Description"] = new HtmlString("Банковские и юридические реквизиты ООО 'ЯКУТСА'");
      return View();
    }

    [Route("Privacy")]
    public IActionResult Privacy()
    {
      ViewData["Title"] = new HtmlString("Политика конфиденциальности");
      ViewData["Description"] = new HtmlString("Положение об обработке персональных данных (далее – Положение, настоящее Положение) разработано и применяется в соответствии с п. 2 ч. 1 ст. 18.1. Федерального закона от 27.07.2006 № 152-ФЗ «О персональных данных».");
      return View("Views/Home/Privacy.cshtml");
    }

    [Route("DeliveryRules")]
    public IActionResult DeliveryRules()
    {
      ViewData["Title"] = new HtmlString("Условия доставки");
      ViewData["Description"] = new HtmlString("Условия осуществления доставки");
      return View("Views/Home/DeliveryRules.cshtml");
    }

    [Route("PaymentRules")]
    public IActionResult PaymentRules()
    {
      ViewData["Title"] = new HtmlString("Способы оплаты");
      ViewData["Description"] = new HtmlString("Способы оплаты");
      return View("Views/Home/PaymentRules.cshtml");
    }

    [Route("RemoveFromCart")]
    public IActionResult RemoveFromCart(int productId, int offerId)
    {
      Cart?.ChangeCount(productId, offerId);

      return RedirectToAction("Cart", "Home");
    }

    [Route("ChangeCount")]
    public IActionResult ChangeCount(int productId, int offerId, int count)
    {
      Cart?.ChangeCount(productId, offerId, count);
      CheckOffers();
      return RedirectToAction("Cart", "Home");
    }

    [HttpPost]
    public IActionResult GetResponse(string? action)
    {
      switch (action?.ToLower())
      {
        case "customers": return Json(_retailCRM.GetResponse<Customer>());
        case "orders": return Json(_retailCRM.GetResponse<Order>());
        case "products": return Json(_retailCRM.GetResponse<Product>());
        case "product-groups": return Json(_retailCRM.GetResponse<ProductGroup>());
        case "reference/payment-types": return Json(_retailCRM.GetResponse<PaymentType>());
        case "reference/delivery-types": return Json(_retailCRM.GetResponse<DeliveryType>());
        default: return Index();
      }
    }

    [Route("Cart")]
    public IActionResult CartView()
    {
      ViewData["Description"] = new HtmlString("");
      ViewData["Title"] = new HtmlString("Корзина");

      return View("Views/Home/Cart.cshtml", Cart);
    }

    [HttpGet]
    [Route("OrderOptions")]
    public IActionResult OrderOptions(CreateOrderObject? createOrder)
    {
      ViewData["Title"] = new HtmlString("Оформление заказа");
      ViewData["Description"] = new HtmlString("Выбор способа оплаты и доставки");


      if (_environment.IsDevelopment() && string.IsNullOrEmpty(createOrder?.email))
      {
        createOrder.email = "pycek@list.ru";
        createOrder.address = new()
        {
          region = "Курская",
          city = "Курск",
          street = "Кулакова",
          building = "9",
          flat = "206",
          text = "г Курск, пр-кт Кулакова, д 9, кв 206"
        };
        createOrder.firstName = "Руслан";
        createOrder.lastName = "Бредихин";
        createOrder.phone = "+79207048884";
        createOrder.patronymic = "Владимирович";
      }

      createOrder.paymentType = "cp";



      ViewBag.DeliveryTypes = _retailCRM.GetResponse<DeliveryType>()?.Array?.Where(t => t.active && t.code != "dalli").ToArray();

      var devTypesJson = _retailCRM.GetDeliveryTypes();


      //ViewBag.PaymentTypes = _retailCRM.GetResponse<PaymentType>()?.Array?.Where(t => t.active).ToArray();

      return View(createOrder);
    }

    [HttpPost]
    [Route("OrderOptions")]
    public Task<IActionResult> OrderOptions(CreateOrderObject createOrder, string deliveryTypeCode, string paymentTypeCode)
    {
      return Task.Run<IActionResult>(async () =>
      {
        PortalActionResult result = new();

        //createOrder.delivery.address = await _retailCRM.ParseAddress(createOrder.address.text);
        createOrder.address = createOrder.delivery.address;

        if (Startup.Environment.IsDevelopment())
        {
          createOrder.delivery.code = "dalli";
          createOrder.delivery.data = new
          {
            locked = true,
            payerType = "sender",
            extraData = new
            {
              partner = "SDEK"
            }
          };
        }

        var managerId = 0;
        var users = _retailCRM.GetResponse<User>();
        int.TryParse(users?.Array?.FirstOrDefault(u => u.isManager)?.id.ToString(), out managerId);
        var customer = _retailCRM.GetResponse<Customer>()?.Array?.FirstOrDefault(c => c.phones.FirstOrDefault(p => p.number.Contains(createOrder.phone)) != null || c.email!.Contains(createOrder.email));
        if (customer != null)
        {
          createOrder.customer = customer;
          createOrder.customer.manager ??= new User { id = managerId };
          createOrder.customer.anyPhone = createOrder.phone;
          createOrder.customer.phone = createOrder.phone;
          createOrder.managerId = createOrder.customer.manager != null ? ((User)createOrder.customer.manager).id : managerId;
        }
        else
        {
          createOrder.managerId = createOrder.managerId;
        }
        createOrder.createdAt = DateTime.Now;

        if (String.IsNullOrEmpty(createOrder.deliveryType) || String.IsNullOrEmpty(createOrder.paymentType))
        {
          result.Message = String.IsNullOrEmpty(createOrder.paymentType) ? new String("Укажите способ оплаты") : new String("Укажите способ получения");
          return result;
        }

        Cart?.CartProducts.ForEach(cp =>
        {
          createOrder.items.Add(new OrderProduct
          {
            initialPrice = (int)cp.Product.maxPrice,
            productId = cp.Product.id.ToString(),
            productName = cp.Product.name,
            quantity = cp.Count,
            offer = cp.Offer
          });
        });

        createOrder.price = Cart.Price + (int)createOrder.delivery.cost;
        
        string link = string.Empty;
        string id = string.Empty;
        _retailCRM.OrderCreate(createOrder, HttpContext.Request.Host.Value, _environment.IsDevelopment()).ContinueWith(t =>
        {
          link = t.Result.link;
          id = t.Result.id;
        }).Wait();
        if (!string.IsNullOrEmpty(link))
        {
          result.Success = true;
          result.Url = link;
          result.Html = id;
          return result;
        }
        else
        {
          result.Message = "Заказ успешно оформлен, скоро с Вами свяжется менеджер.";
        }
        HttpContext.Session.Remove("cart");
        return result;
      });
    }

    [Route("CheckOffers")]
    public IActionResult CheckOffers()
    {
      PortalActionResult result = new();
      var products = _retailCRM.GetResponse<Product>();
      Cart.CartProducts.ForEach((cp) =>
      {
        cp.Count = products.Array.FirstOrDefault(p => p.id == cp.Product.id).offers.FirstOrDefault(o => o.id == cp.Offer.id).quantity < cp.Count ?
        products.Array.FirstOrDefault(p => p.id == cp.Product.id).offers.FirstOrDefault(o => o.id == cp.Offer.id).quantity : cp.Count;
      });
      HttpContext.Session.SetString("cart", System.Text.Json.JsonSerializer.Serialize(Cart));
      result.Success = true;
      return result;
    }

    [Route("SignIn")]
    public async Task<IActionResult> SignIn(string login, string password)
    {
      var user = await _userManager.FindByEmailAsync(login.ToLower());
      var result = await _signIn.PasswordSignInAsync(user, password, true, true);
      if (!result.Succeeded) AppendMessage("Ошибка авторизации", Enums.MessageType.danger);
      AppendMessage("Успешная авторизация!");
      return RedirectToAction("Index", "Home");
    }

    [Route("SignOut")]
    new public async Task<IActionResult> SignOut()
    {
      await _signIn.SignOutAsync();
      return RedirectToAction("Index", "Home");
    }
  }
}

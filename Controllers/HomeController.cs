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
  public class HomeController : BaseController
  {
    private IMemoryCache _cache;

    public HomeController(IMemoryCache memoryCache, RetailCRM retailCRM, UserManager<AppUser> userManager, SignInManager<AppUser> signIn, ApplicationDbContext context, IWebHostEnvironment environment, ILogger<BaseController> logger) : base(retailCRM, userManager, signIn, context, environment, logger)
    {
      _cache = memoryCache;
      CultureInfo.CurrentCulture = new CultureInfo("RU-ru") { DateTimeFormat = new DateTimeFormatInfo() { FullDateTimePattern = "yyyy-MM-dd HH:mm:ss" } };
    }

    public IActionResult Service()
    {

      return View("Views/Home/Service.cshtml");
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

    public IActionResult Index()
    {
      ViewData["Description"] = new HtmlString("Интернет-магазин Российского бренда уличной одежды премиального качества.");
      ViewData["Title"] = new HtmlString("Российский бренд уличной одежды.");
      List<Product>? products = _retailCRM.GetResponse<Product>().Array?.Where(p => p.active && p.quantity != 0).ToList();
      List<ProductGroup>? groups = _retailCRM.GetResponse<ProductGroup>().Array?.ToList();

      products?.ForEach(p =>
      {
        p.groups = groups?.Where(g => p.groups.FirstOrDefault(pg => pg.id == g.id) != null)?.ToArray();
      });

      return View(products);
    }

    //public async Task<IActionResult> Category(int id)
    //{
    //  return await Task.Run(() =>
    //    {
    //      List<Product> products = new();
    //      ProductGroup? productGroup = _retailCRM.GetResponse<ProductGroup>()!.Array!.FirstOrDefault(g => g.id == id);
    //      products = _retailCRM.GetResponse<Product>()!.Array!.Where(p => p.groups.FirstOrDefault(g => g.id == id) != null && p.active && p.quantity != 0).ToList();

    //      products?.ForEach(p =>
    //      {
    //        p?.offers[0].images.ToList().ForEach((img) =>
    //        {
    //          try
    //          {
    //            if (img.Split("_")[2].Contains("m"))
    //            {
    //              ImageSide side = (ImageSide)Enum.Parse(typeof(ImageSide), img.Split("_")[3].Split('.')[0]);
    //              p.images?.Add(new Image { Side = side, Url = img });
    //            }
    //          }
    //          catch (Exception exc)
    //          {
    //            if (_environment.IsDevelopment())
    //            {
    //              AppendMessage(exc.Message);
    //            }
    //            p.imageUrl ??= $"https://{HttpContext.Request.Host}/img/t-shirt.png";
    //          }
    //        });


    //        ViewData["Description"] = new HtmlString("");
    //        ViewData["Title"] = new HtmlString(productGroup?.name);

    //        ViewBag.Category = productGroup;

    //      });

    //      return View("Views/Home/Category.cshtml", products);
    //    });
    //}

    //public async Task<IActionResult?> Product(int id)
    //{
    //  IActionResult? result = null;
    //  await Task.Run(() =>
    //  {
    //    if (id == 0) result = new PortalActionResult { Message = new NullReferenceException().GetType().Name };
    //    else
    //    {
    //      Product? product = _retailCRM.GetResponse<Product>()?.Array?.FirstOrDefault(p => p.id == id)!;
    //      product!.imageUrl ??= $"https://{HttpContext.Request.Host}/img/t-shirt.png";

    //      List<ProductGroup>? productGroups = _retailCRM.GetResponse<ProductGroup>()?.Array?.ToList();
    //      ProductGroup? category = productGroups?.FirstOrDefault(p => product.groups.FirstOrDefault(c => c.id == p.id) != null);

    //      category!.SubGroups = productGroups?.Where(p => p.parentId == category?.id).ToArray();
    //      ViewBag.Category = category;

    //      ViewData["backUrl"] = category?.id;
    //      ViewData["categoryName"] = category?.name;

    //      ViewData["Description"] = new HtmlString($"{category?.name} {product.name} - {product.maxPrice} руб.");
    //      ViewData["Title"] = new HtmlString(product.name);
    //      ViewData["Image"] = new HtmlString(product.imageUrl);

    //      product.modelPath =
    //        Directory.Exists($"{_environment.WebRootPath}/3d/{product.article}") ? $"../../3d/{product.article}/scene.gltf" : "";
    //      result = View(product);
    //    }
    //  });
    //  return result;
    //}

    public IActionResult Debug()
    {
      ViewData["Description"] = new HtmlString("");
      ViewData["Title"] = new HtmlString("Api debug");
      return View("Views/_Debug.cshtml");
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


    //public IActionResult GetCustomers()
    //{
    //  var response = _retailCRM.GetResponse<Customer>();
    //  return Json(response);
    //}

    //public IActionResult GetOrders()
    //{
    //  var response = _retailCRM.GetResponse<Order>();
    //  return Json(response);
    //}

    //public IActionResult GetProducts()
    //{
    //  return Index();
    //}

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
        createOrder.address = new() { city = "Курск", street = "Кулакова", building = "9", flat = "206", text = "г Курск, пр-кт Кулакова, д 9, кв 206" };
        createOrder.firstName = "Руслан";
        createOrder.lastName = "Бредихин";
        createOrder.phone = "+79207048884";
        createOrder.patronymic = "Владимирович";
      }

      createOrder.paymentType = "cp";

      ViewBag.DeliveryTypes = _retailCRM.GetResponse<DeliveryType>()?.Array?.Where(t => t.active).ToArray();

      //ViewBag.PaymentTypes = _retailCRM.GetResponse<PaymentType>()?.Array?.Where(t => t.active).ToArray();

      return View(createOrder);
    }

    [HttpPost]
    [Route("OrderOptions")]
    public Task<IActionResult> OrderOptions(CreateOrderObject createOrder, string deliveryTypeCode, string paymentTypeCode)
    {
      return Task.Run<IActionResult>(() =>
      {
        _retailCRM.ParseAddress(createOrder.address.text).ContinueWith(t => createOrder.address = t.Result).Wait();

        var managerId = 0;

        var users = _retailCRM.GetResponse<User>();

        int.TryParse(users?.Array?.FirstOrDefault(u => u.isManager && u.active)?.id.ToString(), out managerId);


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
        PortalActionResult result = new();

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

        createOrder.price = Cart.Price;

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

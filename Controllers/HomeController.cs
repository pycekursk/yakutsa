using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
    public HomeController(RetailCRM retailCRM, UserManager<AppUser> userManager, SignInManager<AppUser> signIn, ApplicationDbContext context, IWebHostEnvironment environment, ILogger<BaseController> logger) : base(retailCRM, userManager, signIn, context, environment, logger)
    {
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
      ViewData["Description"] = new HtmlString("");
      ViewData["Title"] = new HtmlString("Главная");

      List<Product>? products = _retailCRM.GetResponse<Product>().Array?.ToList();
      products?.ForEach(p => p.imageUrl ??= $"https://{HttpContext.Request.Host}/img/t-shirt.png");

      return View(products);
    }

    public IActionResult Category(int id)
    {
      List<Product> products = new();
      ProductGroup? productGroup = _retailCRM.GetResponse<ProductGroup>()!.Array!.FirstOrDefault(g => g.id == id);
      products = _retailCRM.GetResponse<Product>()!.Array!.Where(p => p.groups.FirstOrDefault(g => g.id == id) != null).ToList();

      products.ForEach(p => p.imageUrl ??= $"https://{HttpContext.Request.Host}/img/t-shirt.png");

      ViewData["Description"] = new HtmlString("");
      ViewData["Title"] = new HtmlString(productGroup?.name);

      ViewBag.Category = productGroup;
      return View("Views/Home/Category.cshtml", products);
    }

    public IActionResult Debug()
    {
      ViewData["Description"] = new HtmlString("");
      ViewData["Title"] = new HtmlString("Api debug");
      return View("Views/_Debug.cshtml");
    }

    public IActionResult ToCart(int id, int offerId, int count = 1)
    {
      PortalActionResult actionResult = new();

      Product? product = _retailCRM.GetResponse<Product>()?.Array?.FirstOrDefault(p => p.id == id);

      count = product.offers.FirstOrDefault(o => o.id == offerId).quantity < count ? product.offers.FirstOrDefault(o => o.id == id).quantity : count;

      base.Cart?.Add(product!, product!.offers.FirstOrDefault(o => o.id == offerId)!, count);

      actionResult.Success = true;
      actionResult.Html = Cart?.Count.ToString()!;
      actionResult.Message = "Добавлено в корзину!";

      return actionResult;
    }

    public IActionResult RefundPolicy()
    {
      ViewData["Title"] = new HtmlString("Политика возврата");
      ViewData["Description"] = new HtmlString("Условия осуществления возврата продукции представленной в интернет-магазине.");
      return View();
    }

    public IActionResult DeliveryRules()
    {
      ViewData["Title"] = new HtmlString("Условия доставки");
      ViewData["Description"] = new HtmlString("Условия осуществления доставки");
      return View();
    }

    public IActionResult PaymentRules()
    {
      ViewData["Title"] = new HtmlString("Способы оплаты");
      ViewData["Description"] = new HtmlString("Способы оплаты");
      return View();
    }

    public IActionResult RemoveFromCart(int productId, int offerId)
    {
      Cart.ChangeCount(productId, offerId);
      return View("Views/Home/Cart.cshtml", Cart);
    }

    public IActionResult ChangeCount(int id, int offerId, int count)
    {
      Cart.ChangeCount(id, offerId, count);
      CheckOffers();
      return View("Views/Home/Cart.cshtml", Cart);
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

    public IActionResult Product(int id)
    {
      Product? product = _retailCRM.GetResponse<Product>()?.Array?.FirstOrDefault(p => p.id == id)!;
      product!.imageUrl ??= $"https://{HttpContext.Request.Host}/img/t-shirt.png";
      ProductGroup? category = _retailCRM.GetResponse<ProductGroup>()?.Array?.FirstOrDefault(p => product.groups.FirstOrDefault(c => c.id == p.id) != null);
      ViewData["backUrl"] = category?.id;
      ViewData["categoryName"] = category?.name;

      ViewData["Description"] = new HtmlString("");
      ViewData["Title"] = new HtmlString(product.name);

      product.modelPath =
        Directory.Exists($"{_environment.WebRootPath}/3d/{product.article}") ? $"../../3d/{product.article}/scene.gltf" : "";

      return View(product);
    }

    public IActionResult GetCustomers()
    {
      var response = _retailCRM.GetResponse<Customer>();
      return Json(response);
    }

    public IActionResult GetOrders()
    {
      var response = _retailCRM.GetResponse<Order>();
      return Json(response);
    }

    public IActionResult GetProducts()
    {
      return Index();
    }

    [ActionName("Cart")]
    public IActionResult CartView()
    {
      ViewData["Description"] = new HtmlString("");
      ViewData["Title"] = new HtmlString("Корзина");

      return View("Views/Home/Cart.cshtml", Cart);
    }

    [HttpGet]
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
    public Task<IActionResult> OrderOptions(CreateOrderObject createOrder, string deliveryTypeCode, string paymentTypeCode)
    {
      return Task.Run<IActionResult>(() =>
      {
        _retailCRM.ParseAddress(createOrder.address.text).ContinueWith(t => createOrder.address = t.Result).Wait();

        createOrder.manager = _retailCRM.GetResponse<User>()?.Array?.FirstOrDefault(u => u.isManager && u.active);
        var customer = _retailCRM.GetResponse<Customer>()?.Array?.FirstOrDefault(c => c.phones.FirstOrDefault(p => p.number.Contains(createOrder.phone)) != null || c.email!.Contains(createOrder.email));

        if (customer != null)
        {
          createOrder.customer = customer;
          createOrder.customer.manager = createOrder.manager;
          createOrder.customer.anyPhone = createOrder.phone;
          createOrder.customer.phone = createOrder.phone;
          createOrder.manager = createOrder.manager;
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
            initialPrice = (int)cp.Price,
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
          HttpContext.Session.Remove("cart");
          //AppendMessage("Заказ успешно оформлен, скоро с Вами свяжется менеджер.", Enums.MessageType.success);
          return result;
        }
        return result;
      });
    }

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

    public async Task<IActionResult> SignIn(string email, string password)
    {
      var user = await _userManager.FindByEmailAsync(email.ToLower());
      var result = await _signIn.PasswordSignInAsync(user, password, true, true);
      if (!result.Succeeded) AppendMessage("Ошибка авторизации", Enums.MessageType.danger);
      AppendMessage("Успешная авторизация!");
      return RedirectToAction("Index", "Home");
    }

    new public async Task<IActionResult> SignOut()
    {
      await _signIn.SignOutAsync();
      return RedirectToAction("Index", "Home");
    }
  }
}

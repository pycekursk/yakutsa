using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

    }

    public IActionResult Service()
    {
      return View("Views/Home/Service.cshtml");
    }


    public IActionResult Index()
    {
      //_retailCRM.OrderCreate(null);


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
      base.Cart?.Add(product!, product!.offers.FirstOrDefault(o => o.id == offerId)!, count);

      actionResult.Success = true;
      actionResult.Html = Cart.Count.ToString();
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
        default: return Json(_retailCRM.GetResponse(action!, RequestMethod.GET, new string[] { "entity:Product" }));
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

    public IActionResult CartView()
    {

      ViewData["Description"] = new HtmlString("");
      ViewData["Title"] = new HtmlString("Корзина");

      return View("Views/Home/Cart.cshtml", Cart);
    }

    [HttpGet]
    public IActionResult OrderOptions()
    {
      ViewData["Title"] = new HtmlString("Оформление заказа");
      ViewData["Description"] = new HtmlString("Выбор способа оплаты и доставки");

      ViewBag.DeliveryTypes = _retailCRM.GetResponse<DeliveryType>().Array;
      ViewBag.PaymentTypes = _retailCRM.GetResponse<PaymentType>().Array;
      return View(new CreateOrderObject());
    }

    [HttpPost]
    public IActionResult OrderOptions(CreateOrderObject createOrder)
    {
      createOrder.createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
      createOrder.externalId = 1;
      Cart.CartProducts.ForEach(cp =>
      {
        createOrder.items.Add(new CreateOrderObjectItem
        {
          initialPrice = (int)cp.Price,
          productId = cp.Product.id,
          productName = cp.Product.name,
          quantity = cp.Count
        });
      });

      _retailCRM.OrderCreate(createOrder);

      ViewData["Title"] = new HtmlString("Оформление заказа");
      ViewData["Description"] = new HtmlString("Выбор способа оплаты и доставки");
      AppendMessage("Заказ успешно оформлен, скоро с Вами свяжется менеджер.", Enums.MessageType.success);
      return RedirectToAction("Index", "Home");
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

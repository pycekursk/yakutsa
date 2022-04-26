using yakutsa.Data;
using yakutsa.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using static yakutsa.Models.Enums;
using yakutsa.Services;
using RetailCRMCore.Models;
using System.Globalization;
using Microsoft.AspNetCore.Html;

namespace yakutsa.Controllers
{
  public class BaseController : Controller
  {
    AppUser? _appUser;
    public AppUser? appUser { get => _appUser ??= GetAppUser(); set => _appUser = UpdateAppUser(value); }

    public static List<Message> Messages { get; set; } = new List<Message>();
    public readonly ILogger<BaseController> _logger;
    internal ApplicationDbContext _context;
    internal IWebHostEnvironment _environment;
    internal UserManager<AppUser> _userManager;
    internal SignInManager<AppUser> _signIn;
    internal RetailCRM _retailCRM;
    internal string _files;
    private Cart? cart;
    private List<Product>? history;
    public string? PreviousUrl;

    public Cart? Cart { get => cart ??= GetCart(); }

    public List<Product> History { get => history ??= GetHistory(); }

    public BaseController(
        RetailCRM retailCRM,
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signIn,
        ApplicationDbContext context,
        IWebHostEnvironment environment,
        ILogger<BaseController> logger
        ) : base()
    {
      _retailCRM = retailCRM;
      _context = context;
      _environment = environment;
      _userManager = userManager;
      _signIn = signIn;
      _files = _environment.WebRootPath + "/files/";
      _logger = logger;
      CultureInfo.CurrentCulture = new CultureInfo("RU-ru") { DateTimeFormat = new DateTimeFormatInfo() { FullDateTimePattern = "yyyy-MM-dd HH:mm:ss" } };
    }

    public void AppendMessage(Message message)
    {
      Messages.Add(message);
    }

    public void AppendMessage(string text, MessageType messageType = MessageType.main)
    {
      AppendMessage(new Message(text, messageType));
    }

    public Task<string?> GetEmail()
    {
      return Task.Run<string?>(() =>
      {
        string result = string.Empty;
        using (var fileStream = new FileStream(_environment.WebRootPath + @"\targetEmail.txt", FileMode.Open))
        {
          StreamReader reader = new StreamReader(fileStream);
          result = reader?.ReadLine()!;
        }
        return result;
      });
    }

    AppUser? GetAppUser()
    {
      var user = _context.Users.Include(u => u.AddressObject).FirstOrDefault(u => u.Email == User.Identity.Name);
      return user;
    }

    AppUser UpdateAppUser(AppUser appUser)
    {
      _context.SaveChanges();
      return appUser;
    }
    void AppendData()
    {
      if (User.Identity?.Name != null && appUser != null)
      {
        appUser.LastVisit = DateTime.Now;
        _context.SaveChanges();
      }
      ViewBag.Messages = new List<Message>(Messages);
      ViewBag.Text = "Text";
      Messages = new List<Message>();
      ViewBag.IsDevelopment = _environment.IsDevelopment();

      var groups = _retailCRM.GetResponse<ProductGroup>()?.Array?.Where(g => g.active && g.parentId == 0)?.ToList();
      ViewBag.Menu = groups;
      ViewBag.History = History;

      ViewData["cartPrice"] = Cart?.Price;
      ViewData["count"] = Cart?.Count;
      ViewData["Image"] ??= $"https://{this.HttpContext.Request.Host}/img/sm_logo.png";
      ViewData["canonical"] = new HtmlString($"https://{this.Request.Host}{this.Request.Path}".ToLower());

      ViewBag.Path = this.Request.Path.ToString() == "/" ? new string[0] : this.Request.Path.ToString().Split("/").Where(s => !string.IsNullOrEmpty(s)).ToArray();
    }






    public override ViewResult View()
    {
      AppendData();
      return base.View();
    }

    public override ViewResult View(string? viewName)
    {
      AppendData();
      return base.View(viewName);
    }
    public override ViewResult View(string? viewName, object? model)
    {
      AppendData();
      return base.View(viewName, model);
    }






    List<Product>? GetHistory()
    {
      if (HttpContext.Session.Keys.Contains("history"))
      {
        history = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Product>>(HttpContext.Session.GetString("history")!);
      }
      else if (!HttpContext.Session.Keys.Contains("history"))
      {
        history = new List<Product>();
        HttpContext.Session.SetString("history", System.Text.Json.JsonSerializer.Serialize(history));
      }

      return history;
    }

    internal void ToHistory(Product product)
    {
      if (!History.Contains(product))
      {
        history.Add(product);
        HttpContext.Session.SetString("history", System.Text.Json.JsonSerializer.Serialize(history));
      }
    }

    internal void RemoveFromHistory(Product product)
    {
      if (History.Contains(product))
      {
        history?.Remove(product);
        HttpContext.Session.SetString("history", System.Text.Json.JsonSerializer.Serialize(history));
      }
    }

    Cart? GetCart()
    {
      Cart? cart = new(HttpContext);
      if (HttpContext.Session.Keys.Contains("cart"))
      {
        cart = Newtonsoft.Json.JsonConvert.DeserializeObject<Cart>(HttpContext.Session.GetString("cart")!);
        cart!._httpContext = HttpContext;
        if (cart != null && cart.Price != 0)
        {
          ViewData["cartPrice"] = cart.Price;
        }
      }
      else if (!HttpContext.Session.Keys.Contains("cart"))
      {
        HttpContext.Session.SetString("cart", System.Text.Json.JsonSerializer.Serialize(cart));
      }

      //if (_environment.IsDevelopment() && History?.Count == 0)
      //{
      //  var products = _retailCRM.GetResponse<Product>()?.Array;

      //  //  var product = products?.FirstOrDefault(products => products.article == "TTST-")!;
      //  //  if (product != null)
      //  //    cart?.Add(product!, product.offers[0]!, 2);

      //  ToHistory(products?.ElementAtOrDefault(0));
      //  ToHistory(products?.ElementAt(1));
      //  //ToHistory(products?.ElementAt(2));
      //  //ToHistory(products != null ? products.ElementAt(3) : null);
      //}

      return cart;
    }
  }
}
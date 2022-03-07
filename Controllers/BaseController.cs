﻿using yakutsa.Data;
using yakutsa.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using static yakutsa.Models.Enums;
using yakutsa.Services;

namespace yakutsa.Controllers
{
  public class BaseController : Controller
  {
    AppUser _appUser;
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

    public Cart? Cart { get => cart ??= GetCart(); }

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
    }

    public void AppendMessage(Message message)
    {
      Messages.Add(message);
    }

    public void AppendMessage(string text, MessageType messageType = MessageType.main)
    {
      AppendMessage(new Message(text, messageType));
    }

    public Task<string> GetEmail()
    {
      return Task.Run<string>(() =>
      {
        string result = string.Empty;
        using (var fileStream = new FileStream(_environment.WebRootPath + @"\targetEmail.txt", FileMode.Open))
        {
          StreamReader reader = new StreamReader(fileStream);
          result = reader.ReadLine();
        }
        return result;
      });
    }

    AppUser GetAppUser()
    {
      var user = _context.Users.Include(u => u.AddressObject).FirstOrDefault(u => u.Email == User.Identity.Name);
      return user;
    }

    AppUser UpdateAppUser(AppUser appUser)
    {
      _context.SaveChanges();
      return appUser;
    }

    public override ViewResult View(string? viewName, object? model)
    {
      if (User.Identity?.Name != null && appUser != null)
      {
        appUser.LastVisit = DateTime.Now;
        _context.SaveChanges();
      }
      ViewBag.Messages = new List<Message>(Messages);
      Messages = new List<Message>();
      ViewBag.IsDevelopment = _environment.IsDevelopment();

      var groups = _retailCRM.GetResponse<ProductGroup>()?.Array?.Where(g => g.active)?.ToList();
      ViewBag.Menu = groups;

      ViewData["cartPrice"] = Cart?.Price;
      ViewData["count"] = Cart?.Count;

      return base.View(viewName, model);
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
      return cart;
    }
  }
}
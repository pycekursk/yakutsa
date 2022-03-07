using System.Text.Json;
using System.Diagnostics;
using yakutsa.Models;
using Microsoft.AspNetCore.Html;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.Extensions;
using System.Web;
using System.Text;
using Newtonsoft.Json;
using static yakutsa.Services.RetailCRM;
using System.ComponentModel;

namespace yakutsa.Services
{
  public class RetailCRM
  {
    const string _key = "h0NsTuUjjscl7JG5SEk6NZPJPuw4dryy";
    const string _url = "https://yakutsa.retailcrm.ru/api/v5/";
    HttpClient _httpClient;

    public RetailCRM()
    {
      _httpClient = new HttpClient();
      _httpClient.DefaultRequestHeaders.Add("X-API-KEY", _key);
    }

    public string GetResponse(string action, RequestMethod method = RequestMethod.GET, string[]? args = null)
    {
      string result = string.Empty;
      if (method == RequestMethod.GET)
        _httpClient.GetAsync(_url + action).ContinueWith((t) =>
          t.Result.Content.ReadAsStringAsync().ContinueWith((t) =>
            result = @t.Result)
            .Wait())
          .Wait();
      else if (method == RequestMethod.POST)
      {
        //var url = _url + "custom-fields?" + "";
        //_httpClient.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");

        _httpClient.PostAsync(_url + action, null).ContinueWith((t) =>
         t.Result.Content.ReadAsStringAsync().ContinueWith((t) => result = t.Result).Wait()).Wait();

        return result;
      }
      return result;
    }

    public Response<T> GetResponse<T>()
    {
      Response<T> response = new();
      var url = GetActionUrl<T>();

      _httpClient.GetAsync(url).ContinueWith((t) =>
      {
        t.Result.Content.ReadAsStringAsync().ContinueWith((t) =>
        {
          if (typeof(T).Name == "DeliveryType" || typeof(T).Name == "PaymentType")
          {
            Regex regex = new Regex("(?<!,){\"name.+?}|{\"isDynamicCostCalculation.+?}");
            var matches = regex.Matches(t.Result);

            if (matches.Count == 0) return;

            response = new Response<T>();
            int index = 0;
            foreach (Match match in matches)
            {
              response.Array ??= new T[matches.Count];
              var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(match.Value);
              response.Array[index++] = obj!;
            }
            response.Success = true;
          }
          else
          {
            var options = new JsonSerializerOptions
            {
              PropertyNamingPolicy = new ArrayNamingProlicy<T>(),
              WriteIndented = true,
              PropertyNameCaseInsensitive = true,
            };
            response = System.Text.Json.JsonSerializer.Deserialize<Response<T>>(
              t.Result
              .Replace("productGroup", "productGroups")
              .Replace(@"\", "")
              .Replace("u0022", "'"),
              options
              )!;
          }
        })
        .Wait();
      })
        .Wait();
      return response;
    }
    public async Task<Response<T>> GetResponseAsync<T>() => await Task.Run(() => GetResponse<T>());
    string GetActionUrl<T>()
    {
      switch (typeof(T).Name)
      {
        case nameof(ProductGroup): return $"{_url}store/product-groups";
        case nameof(Product): return $"{_url}store/products";
        case nameof(DeliveryType): return $"{_url}reference/delivery-types";
        case nameof(PaymentType): return $"{_url}reference/payment-types";
        default: return $"{_url}{typeof(T).Name.ToLower()}s";
      }
    }



    public void OrderCreate(CreateOrderObject createOrderObject)
    {
      var url = _url + "orders/create";
      createOrderObject.externalId = BitConverter.ToInt32(Guid.NewGuid().ToByteArray());
      var json = Newtonsoft.Json.JsonConvert.SerializeObject(createOrderObject, new JsonSerializerSettings() { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii }).Replace("\"order\"=[{", "\"order\"={").Replace("}]}]}", "}]}");
      var str = new StringContent("apiKey=h0NsTuUjjscl7JG5SEk6NZPJPuw4dryy&order=" + json, Encoding.UTF8, "application/x-www-form-urlencoded");
      _httpClient.PostAsync(url, str).ContinueWith((t) =>
           {
             dynamic? json = string.Empty;
             var result = t.Result;
             result.Content.ReadAsStringAsync().ContinueWith(t2 =>
         {
           json = Newtonsoft.Json.JsonConvert.DeserializeObject(t2.Result);
           bool isSuccess = json?.success;
           if (!isSuccess)
           {
             Debug.WriteLine((string)json.errorMsg);
             return;
           }

         }).Wait();
           }).Wait();
    }

    public class CreateOrderObject
    {
      public int externalId { get; set; }
      public string createdAt { get; set; }
      public string lastName { get; set; }
      public string firstName { get; set; }
      public string email { get; set; }
      public string phone { get; set; }
      public List<CreateOrderObjectItem> items { get; set; } = new List<CreateOrderObjectItem>();
      public Address address { get; set; }
      public DeliveryType deliveryType { get; set; }
      public PaymentType paymentType { get; set; }
    }

    public class CreateOrderObjectItem
    {
      public int initialPrice { get; set; }
      public int quantity { get; set; }
      public int productId { get; set; }
      public string productName { get; set; }
    }

    public enum RequestMethod
    {
      GET = 0,
      POST = 1
    }




    public class Response<T>
    {
      public bool Success { get; set; }
      public Pagination? Pagination { get; set; }
      public T[]? Array { get; set; }
    }

    public class ArrayNamingProlicy<T> : JsonNamingPolicy
    {
      public override string ConvertName(string name)
      {
        string result = string.Empty;
        result = name == "Array" ? $"{typeof(T).Name.ToLower()}s" : name.ToLower();
        return result;
      }
    }


    public class Pagination
    {
      public int limit { get; set; }
      public int totalCount { get; set; }
      public int currentPage { get; set; }
      public int totalPageCount { get; set; }
    }

    public class Customer
    {
      public string type { get; set; }
      public int id { get; set; }
      public bool isContact { get; set; }
      public string createdAt { get; set; }
      public bool vip { get; set; }
      public bool bad { get; set; }
      public string site { get; set; }
      public Contragent contragent { get; set; }
      public object[] tags { get; set; }
      public object[] customFields { get; set; }
      public int marginSumm { get; set; }
      public int totalSumm { get; set; }
      public int averageSumm { get; set; }
      public int ordersCount { get; set; }
      public int costSumm { get; set; }
      public int personalDiscount { get; set; }
      public Segment[] segments { get; set; }
      public string firstName { get; set; }
      public string lastName { get; set; }
      public string email { get; set; }
      public Phone[] phones { get; set; }
    }

    public class Contragent
    {
      public string contragentType { get; set; }
    }

    public class Segment
    {
      public int id { get; set; }
      public string code { get; set; }
      public string name { get; set; }
      public string createdAt { get; set; }
      public bool isDynamic { get; set; }
      public int customersCount { get; set; }
      public bool active { get; set; }
    }

    public class Phone
    {
      public string number { get; set; }
    }

    public class Unit
    {
      public string code { get; set; }
      public string name { get; set; }
      public string sym { get; set; }
    }

    public class Price
    {
      public string priceType { get; set; }
      public int price { get; set; }
      public int ordering { get; set; }
    }


    public class Cost
    {
      public int id { get; set; }
      public string dateFrom { get; set; }
      public string dateTo { get; set; }
      public int summ { get; set; }
      public string costItem { get; set; }
      public string createdAt { get; set; }
      public string createdBy { get; set; }
      public Order order { get; set; }
    }


    public class History
    {
      public int id { get; set; }
      public string createdAt { get; set; }
      public string source { get; set; }
      public string field { get; set; }
      public object oldValue { get; set; }
      public Newvalue newValue { get; set; }
      public Customer customer { get; set; }
    }

    public class Newvalue
    {
      public string code { get; set; }
    }


    public class User
    {
      public int id { get; set; }
      public string createdAt { get; set; }
      public bool active { get; set; }
      public string email { get; set; }
      public string firstName { get; set; }
      public string lastName { get; set; }
      public string status { get; set; }
      public bool online { get; set; }
      public bool isAdmin { get; set; }
      public bool isManager { get; set; }
      public Group[] groups { get; set; }
      public int mgUserId { get; set; }
    }

    public class Group
    {
      public int id { get; set; }
      public string name { get; set; }
      public string code { get; set; }
    }

  }

  public class Order
  {
    public int slug { get; set; }
    public int bonusesCreditTotal { get; set; }
    public int bonusesChargeTotal { get; set; }
    public int id { get; set; }
    public string number { get; set; }
    public string externalId { get; set; }
    public string orderType { get; set; }
    public string orderMethod { get; set; }
    public string privilegeType { get; set; }
    public string countryIso { get; set; }
    public string createdAt { get; set; }
    public string statusUpdatedAt { get; set; }
    public int summ { get; set; }
    public int totalSumm { get; set; }
    public int prepaySum { get; set; }
    public int purchaseSumm { get; set; }
    public string markDatetime { get; set; }
    public string lastName { get; set; }
    public string firstName { get; set; }
    public string phone { get; set; }
    public string email { get; set; }
    public bool call { get; set; }
    public bool expired { get; set; }
    public Customer customer { get; set; }
    public Contact contact { get; set; }
    public Contragent contragent { get; set; }
    public Delivery delivery { get; set; }
    public string site { get; set; }
    public string status { get; set; }
    public Item[] items { get; set; }
    public string fullPaidAt { get; set; }
    public Payment[] payments { get; set; }
    public bool fromApi { get; set; }
    public string shipmentStore { get; set; }
    public bool shipped { get; set; }
    public object[] customFields { get; set; }
  }

  //public class Customer
  //{
  //  public string type { get; set; }
  //  public int id { get; set; }
  //  public bool isContact { get; set; }
  //  public string createdAt { get; set; }
  //  public bool vip { get; set; }
  //  public bool bad { get; set; }
  //  public string site { get; set; }
  //  public Contragent contragent { get; set; }
  //  public object[] tags { get; set; }
  //  public object[] customFields { get; set; }
  //  public int marginSumm { get; set; }
  //  public int totalSumm { get; set; }
  //  public int averageSumm { get; set; }
  //  public int ordersCount { get; set; }
  //  public int personalDiscount { get; set; }
  //  public object[] segments { get; set; }
  //  public string firstName { get; set; }
  //  public string lastName { get; set; }
  //  public string email { get; set; }
  //  public Phone[] phones { get; set; }
  //}

  public class Contragent
  {
    public string contragentType { get; set; }
  }

  public class Phone
  {
    public string number { get; set; }
  }

  public class Contact
  {
    public string type { get; set; }
    public int id { get; set; }
    public bool isContact { get; set; }
    public string createdAt { get; set; }
    public bool vip { get; set; }
    public bool bad { get; set; }
    public string site { get; set; }
    public Contragent contragent { get; set; }
    public object[] tags { get; set; }
    public object[] customFields { get; set; }
    public int marginSumm { get; set; }
    public int totalSumm { get; set; }
    public int averageSumm { get; set; }
    public int ordersCount { get; set; }
    public int personalDiscount { get; set; }
    public object[] segments { get; set; }
    public string firstName { get; set; }
    public string lastName { get; set; }
    public string email { get; set; }
    public Phone[] phones { get; set; }
  }



  public class Delivery
  {
    public int cost { get; set; }
    public int netCost { get; set; }
    public Address address { get; set; }
  }

  public class Address
  {
    [DisplayName("Страна")]
    public string country { get; set; }
    [DisplayName("Время доставки")]
    public string deliveryTime { get; set; }
    [DisplayName("Индекс")]
    public string index { get; set; }
    [DisplayName("Регион")]
    public string region { get; set; }
    [DisplayName("Город")]
    public string city { get; set; }
    [DisplayName("Тип населенного пункта")]
    public string cityType { get; set; }
    [DisplayName("Улица")]
    public string street { get; set; }
    [DisplayName("Тип улицы")]
    public string streetType { get; set; }
    [DisplayName("Дом")]
    public string building { get; set; }
    [DisplayName("Строение")]
    public string house { get; set; }
    [DisplayName("Корпус")]
    public string housing { get; set; }
    [DisplayName("Подъезд")]
    public string block { get; set; }
    [DisplayName("Номер квартиры/офиса")]
    public string flat { get; set; }
    [DisplayName("Этаж")]
    public string floor { get; set; }
    [DisplayName("Метро")]
    public string metro { get; set; }
    [DisplayName("Адрес в текстовом виде")]
    public string text { get; set; }
    [DisplayName("Примечания к адресу")]
    public string notes { get; set; }
    [DisplayName("Адрес пустой")]
    public bool isEmpty { get; set; }
    [DisplayName("Адрес в строковом виде")]
    public string fullAddressString { get; set; }
  }

  public class Item
  {
    public int bonusesChargeTotal { get; set; }
    public int bonusesCreditTotal { get; set; }
    public object[] markingCodes { get; set; }
    public object[] discounts { get; set; }
    public int id { get; set; }
    public int initialPrice { get; set; }
    public int discountTotal { get; set; }
    public Price[] prices { get; set; }
    public string vatRate { get; set; }
    public string createdAt { get; set; }
    public int quantity { get; set; }
    public string status { get; set; }
    public Offer offer { get; set; }
    public object[] properties { get; set; }
    public int purchasePrice { get; set; }
  }



  public class Payment
  {
    public int id { get; set; }
    public string status { get; set; }
    public string type { get; set; }
    public int amount { get; set; }
    public string paidAt { get; set; }
  }

}





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
using RetailCRMCore;
using RetailCRMCore.Versions.V5;
using RetailCRMCore.Models;
using RetailCRMCore.Helpers;
using System.Net.Http.Headers;
using Dadata;
using System.Linq;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using Newtonsoft.Json.Linq;
using yakutsa.Extensions;
using System.Security.Cryptography;

namespace yakutsa.Services
{
  public class CreateInvoice
  {
    public int paymentId { get; set; }
    public string returnUrl { get; set; }
  }

  public partial class RetailCRM
  {
    const string _key = "h0NsTuUjjscl7JG5SEk6NZPJPuw4dryy";
    const string _url = "https://yakutsa.retailcrm.ru";
    Client _client;
    Sdek.ApiClient _sdek;


    public RetailCRM()
    {
      _client = new Client(_url, _key);
      //_sdek = new Sdek.ApiClient("cdek-92", _client);//

      //var dt = _client.DeliveryTypes().GetRawResponse();
    }

    public async Task<bool?> UpdateProductsAsync(Product[] products)
    {
      var url = _url + $"/api/v5/store/products/batch/edit?apiKey={_key}";
      foreach (Product? product in products) product.updatedAt = DateTime.Now;
      HttpClient client = new HttpClient();
      client.DefaultRequestHeaders.Host = "yakutsa.retailcrm.ru";
      HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
      client.DefaultRequestHeaders
      .Accept
      .Add(new MediaTypeWithQualityHeaderValue("application/json"));

      var serializerOptions = new JsonSerializerOptions
      {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
      };

      var json = "products=" + System.Text.Json.JsonSerializer.Serialize(products, serializerOptions);
      requestMessage.Content = new StringContent(json,
                                    Encoding.UTF8,
                                    "application/x-www-form-urlencoded");
      HttpResponseMessage response = await client.SendAsync(requestMessage);
      var result = await response.Content.ReadAsStringAsync();
      var obj = System.Text.Json.JsonSerializer.Deserialize<CreateInvoiceResult>(result);

      return obj?.success;
    }

    public async Task<string> CreateInvoice(CreateInvoice data)
    {
      var url = _url + $"/api/v5/payment/create-invoice?apiKey={_key}";

      HttpClient client = new HttpClient();
      client.DefaultRequestHeaders.Host = "yakutsa.retailcrm.ru";
      HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
      //client.DefaultRequestHeaders
      //.Accept
      //.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      var serializerOptions = new JsonSerializerOptions
      {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
      };
      var json = "createInvoice=" + System.Text.Json.JsonSerializer.Serialize(data, serializerOptions);
      requestMessage.Content = new StringContent(json,
                                    Encoding.UTF8,
                                    "application/x-www-form-urlencoded");
      HttpResponseMessage response = await client.SendAsync(requestMessage);
      if (response.StatusCode != System.Net.HttpStatusCode.OK)
      {
        throw new Exception(response.StatusCode.ToString());
      }
      var result = await response.Content.ReadAsStringAsync();
      var obj = System.Text.Json.JsonSerializer.Deserialize<CreateInvoiceResult>(result);
      return obj?.result.link;
    }

    public Task<string> CheckInvoice(string id)
    {
      return Task.Run<string>(() =>
      {
        var rawResponse = _client.OrdersGet(id).GetRawResponse();
        try
        {
          dynamic order = Newtonsoft.Json.JsonConvert.DeserializeObject(rawResponse);
          var payments = order["order"]["payments"];
          foreach (dynamic payment in payments)
          {
            var elem = payment.First;
            var status = elem["status"].ToString();
            return status;
          }
        }
        catch (Exception exp)
        {
          //throw exp;
        }
        return "wait-approved";
      });
    }

    class CreateInvoiceResult
    {
      public bool success { get; set; }
      public Result result { get; set; }

      public class Result
      {
        public string link { get; set; }
      }
    }

    public async Task<string> CreatePayment(int id, string host)
    {
      var createInvoice = new CreateInvoice
      {
        paymentId = id,
        returnUrl = $"https://{host}/PaymentReturn"
      };
      try
      {
        return await this.CreateInvoice(createInvoice);
      }
      catch (Exception exc)
      {
        throw exc;
      }
    }

    public Task<string> GetDeliveryTariffs(Order order = null)
    {
      return Task.Run<string>(async () =>
      {
        string result = String.Empty;
        HttpClient httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders
        .Accept
        .Add(new MediaTypeWithQualityHeaderValue("application/xml"));
        var requestXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><deliverycost><auth token=\"cd94b1b2049e7e35db64ec756bc49073\"></auth><partner>SDEK</partner><townto>Курск</townto><oblname>Курская</oblname><weight>0.8</weight><price>7000</price><inshprice>7000</inshprice><cashservices>NO</cashservices><length>50</length><width>50</width><height>50</height><output>x2</output></deliverycost>";
        var stringContent = new StringContent(requestXml, Encoding.UTF8, "application/xml");
        var response = await httpClient.PostAsync("https://api.dalli-service.com/v1/", stringContent);
        result = await response.Content.ReadAsStringAsync();
        return result;
      });
    }

    public async Task<string> GetPVZList(string partner)
    {
      var requestXml = $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><pvzlist><auth token=\"cd94b1b2049e7e35db64ec756bc49073\"></auth><town>Курск</town><partner>{partner}</partner></pvzlist>";
      string result = String.Empty;
      HttpClient httpClient = new HttpClient();
      httpClient.DefaultRequestHeaders
      .Accept
      .Add(new MediaTypeWithQualityHeaderValue("application/xml"));

      var stringContent = new StringContent(requestXml, Encoding.UTF8, "application/xml");
      var response = await httpClient.PostAsync("https://api.dalli-service.com/v1/", stringContent);
      result = await response.Content.ReadAsStringAsync();
      return result;
    }

    public async Task<string> GetDeliveryTypes()
    {
      string result = String.Empty;
      HttpClient httpClient = new HttpClient();
      httpClient.DefaultRequestHeaders
      .Accept
      .Add(new MediaTypeWithQualityHeaderValue("application/xml"));
      var someXmlString = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><services><auth token=\"cd94b1b2049e7e35db64ec756bc49073\"></auth></services>";
      var stringContent = new StringContent(someXmlString, Encoding.UTF8, "application/xml");
      var response = await httpClient.PostAsync("https://api.dalli-service.com/v1/", stringContent);
      result = await response.Content.ReadAsStringAsync();

      GetPVZList("BOXBERRY");

      GetPVZList("SDEK");

      return result;
    }

    public Response<T> GetResponse<T>()
    {
      Response? response = null;
      switch (typeof(T).Name)
      {
        case "Order":
          response = _client.OrdersList();
          break;
        case "Product":
          response = _client.StoreProducts();
          break;
        case "ProductGroup":
          response = _client.StoreProductsGroups();
          break;
        case "PaymentType":
          response = _client.PaymentTypes();
          break;
        case "DeliveryType":
          response = _client.DeliveryTypes();
          break;
        case "User":
          response = _client.Users();
          break;
        case "Customer":
          response = _client.CustomersList();
          break;
        case "Offer":
          response = _client.StoreProducts();
          break;
        default: throw new NotImplementedException("Метод API не реализован");
      }
      return (Response<T>)response;
    }

    public List<Product>? GetProducts()
    {
      List<Product>? products = new List<Product>();
      products = GetResponse<Product>()?.Array?.ToList();
      var groups = GetResponse<ProductGroup>()?.Array?.ToList();
      products?.ForEach(p =>
      {
        if (p.groups != null)
          foreach (var group in p.groups)
          {
            group.name = groups.FirstOrDefault(g => g.id == group.id).name;
          }
      });

      return products;
    }

    public async Task<(string link, string id)> OrderCreate(CreateOrderObject createOrderObject, string host, bool isDevelopment = false)
    {
      RetailCRMCore.Models.Order order = new RetailCRMCore.Models.Order();

      order.createdAt = createOrderObject.createdAt;
      order.externalId = Guid.NewGuid().ToString();
      order.lastName = createOrderObject.lastName;
      order.firstName = createOrderObject.firstName;
      order.patronymic = createOrderObject.patronymic;
      order.email = createOrderObject.email;
      order.phone = createOrderObject.phone;
      order.items = createOrderObject.items.ToArray();
      order.summ = createOrderObject.price;
      order.delivery = createOrderObject.delivery;
      //order.toPaySumm = createOrderObject.price + (int)createOrderObject.delivery.cost;
      //order.totalSumm = createOrderObject.price + (int)createOrderObject.delivery.cost;
      order.customerComment = createOrderObject.comment;
      order.customer = createOrderObject.customer;
      order.managerId = createOrderObject.managerId;
      order.anyPhone = createOrderObject.phone;
      order.anyEmail = createOrderObject.email;

      CalculateDelivery(order);

      var json = _client.OrdersCreate(order).GetRawResponse();

      var jObject = JObject.Parse(json);

      JToken property = null;
      jObject.TryGetValue("order", out property);
      try
      {
        var payment = (property as dynamic).payments[0];
        string link = await CreatePayment((int)payment.id, host);
        string id = (property as dynamic).externalId;
        return (link, id);
      }
      catch (Exception exc)
      {
        throw exc;
      }
    }

    //undone: не дописано
    public Task CalculateDelivery(Order order)
    {
      return Task.Run(async () =>
      {
        var obj = JObject.Parse(_client.IntegrationsSettingGet("dalli-service").GetRawResponse()).Property("integrationModule").Value.ToString();
        var dalliIntegrationModule = JsonConvert.DeserializeObject<IntegrationModule>(obj);

        var url = _url + $"/api/v5/integration-modules/calculate?apiKey={_key}";

        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Host = "yakutsa.retailcrm.ru";
        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        var json = "deliveryTypeCodes=" + JsonConvert.SerializeObject(new string[] { "dalli-service" }) + "&order=" + System.Text.Json.JsonSerializer.Serialize(order);
        client.DefaultRequestHeaders
        .Accept
        .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        requestMessage.Content = new StringContent(json,
                                   Encoding.UTF8,
                                   "application/x-www-form-urlencoded");
        HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
        string result = await responseMessage.Content.ReadAsStringAsync();
      });
    }

    //public Task<Response<DeliveryType>> DeliveryTypesAsync()
    //{
    //  return Task.Run<Response<DeliveryType>>(() =>
    //  {
    //    return GetResponse<DeliveryType>();
    //  });
    //}

    //public async Task<Address> ParseAddress(string address)
    //{
    //  Address result = new();
    //  var token = "aa9b411a0851eb8344a4fe5fc9cfc272a994c6ab";
    //  var secret = "15178f7ea73ba5e799adde3745bae8d9dc5de767";
    //  var api = new CleanClientAsync(token, secret);
    //  var response = await api.Clean<Object>(address);
    //  result.countryIso = response.country_iso_code;
    //  result.text = String.IsNullOrEmpty(response.result) ? address : response.result;
    //  result.streetType = response.street_type_full;
    //  result.cityType = response.city_type_full;
    //  result.city = response.city;
    //  result.index = response.postal_code;
    //  result.building = response.house;
    //  result.street = response.street;
    //  result.flat = response.flat;
    //  result.region = response.region_with_type;

    //  try
    //  {
    //    result.floor = response.floor != null ? int.Parse(response.floor) : null;
    //    result.block = response.entrance != null ? int.Parse(response.entrance) : null;
    //  }
    //  catch (Exception exc)
    //  {
    //    Debug.WriteLine(exc.Message);
    //  }


    //  result.metro = response.metro?.FirstOrDefault()?.name;
    //  return result;
    //}

    public class Rootobject
    {
      public bool success { get; set; }
      public int id { get; set; }
      public Order order { get; set; }
    }

    public class CreateOrderObject
    {
      private DateTime _createdAt;
      public int externalId { get; set; }
      public DateTime createdAt { get => _createdAt; set => _createdAt = value; }
      public string lastName { get; set; }
      public string firstName { get; set; }
      public string email { get; set; }
      public string phone { get; set; }
      public List<OrderProduct> items { get; set; } = new List<OrderProduct>();
      public Address address { get; set; }
      public string deliveryType { get; set; }
      public string paymentType { get; set; }
      public string comment { get; set; }
      public double price { get; internal set; }
      public string patronymic { get; internal set; }
      public int managerId { get; set; }
      public Customer? customer { get; set; }
      public Delivery? delivery { get; set; }
    }

    public class Response<T>
    {
      public bool Success { get; set; }
      public Pagination? Pagination { get; set; }
      public T[]? Array { get; set; }

      public static implicit operator Response<T>(RetailCRMCore.Response response)
      {
        var options = new JsonSerializerOptions
        {
          PropertyNamingPolicy = new ArrayNamingProlicy<T>(),
          AllowTrailingCommas = true,
          PropertyNameCaseInsensitive = true,

        };
        options.Converters.Add(new JsonDateTimeConverter());

        var result = new Response<T>();

        if (typeof(T).Name == "DeliveryType" || typeof(T).Name == "PaymentType")
        {
          Regex regex = new Regex("(?<!,){\"name.+?}|{\"isDynamicCostCalculation.+?}");
          var matches = regex.Matches(response.GetRawResponse());

          if (matches.Count == 0) return result;

          int index = 0;

          foreach (Match match in matches)
          {
            result.Array ??= new T[matches.Count];
            try
            {
              var obj = System.Text.Json.JsonSerializer.Deserialize<T>(match.Value);

              result.Array[index++] = obj!;
            }
            catch
            {
              var obj = System.Text.Json.JsonSerializer.Deserialize<T>(match.Value + "}");
              result.Array[index++] = obj!;
              Debug.WriteLine(match.Value);
            }
          }
          result.Success = true;
        }
        else if (typeof(T).Name == "Offer")
        {
          Regex regex = new Regex("(?<=\"offers\":)\\[.+?\\](?=,\"updatedAt\")");
          List<T>? offers = new List<T>();
          var matches = regex.Matches(response.GetRawResponse());
          //JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
          //{

          //ContractResolver = new PortalContractResolver(),
          //SerializationBinder = serializationBinder
          // };

          foreach (Match match in matches)
          {
            List<T>? offer = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(match.Value);//, jsonSerializerSettings);
            offers.AddRange(offer);
          }
          result.Array = offers.ToArray();
          result.Success = true;
        }
        else
        {
          try
          {
            result = System.Text.Json.JsonSerializer.Deserialize<Response<T>>(
             response.GetRawResponse()!
              .Replace("productGroup", "productGroups")
              .Replace(@"\", "")
              .Replace("u0022", "'"),
              options
              )!;
          }
          catch (Exception exc)
          {
            throw exc;
          }
        }
        return result;
      }
    }
  }
}
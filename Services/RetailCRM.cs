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

namespace yakutsa.Services
{
  public class CreateInvoice
  {
    public int paymentId { get; set; }
    public string returnUrl { get; set; }
  }
  public class ApiCreateInvoiceRequest
  {
    public CreateInvoice createInvoice { get; set; }
  }

  public partial class RetailCRM
  {
    const string _key = "h0NsTuUjjscl7JG5SEk6NZPJPuw4dryy";
    const string _url = "https://yakutsa.retailcrm.ru";
    Client _client;

    public RetailCRM()
    {
      _client = new Client(_url, _key);
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
      client.DefaultRequestHeaders
      .Accept
      .Add(new MediaTypeWithQualityHeaderValue("application/json"));

      var serializerOptions = new JsonSerializerOptions
      {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
      };
      //serializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;


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

    class OrderResponse
    {
      public bool success { get; set; }
      public Order order { get; set; }
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

    public async Task<Response<T>> GetResponseAsync<T>() => await Task.Run(() => GetResponse<T>());
    public async Task<(string link, string id)> OrderCreate(CreateOrderObject createOrderObject, string host, bool isDevelopment = false)
    {
      RetailCRMCore.Models.Order order = new RetailCRMCore.Models.Order();

      try
      {
        order.createdAt = createOrderObject.createdAt;
        order.externalId = Guid.NewGuid().ToString();
        order.lastName = createOrderObject.lastName;
        order.firstName = createOrderObject.firstName;
        order.patronymic = createOrderObject.patronymic;
        order.email = createOrderObject.email;
        order.phone = createOrderObject.phone;
        order.items = createOrderObject.items.ToArray();
        order.summ = createOrderObject.price;
        order.delivery = new Delivery { address = createOrderObject.address, code = createOrderObject.deliveryType };
        order.toPaySumm = createOrderObject.price;
        order.totalSumm = createOrderObject.price;
        order.customerComment = createOrderObject.comment;
        order.customer = createOrderObject.customer;
        order.managerId = createOrderObject.managerId;
        order.anyPhone = createOrderObject.phone;
        order.anyEmail = createOrderObject.email;
        var json = _client.OrdersCreate(order).GetRawResponse();

        var options = new JsonSerializerOptions
        {
          WriteIndented = true,
          PropertyNameCaseInsensitive = true,
        };
        options.Converters.Add(new JsonDateTimeConverter());
        Rootobject result = System.Text.Json.JsonSerializer.Deserialize<Rootobject>(json: json, options: options);
        var resultOrder = result?.order;
        try
        {
          string link = await CreatePayment(resultOrder.payments.LastOrDefault().id, host);
          string id = resultOrder.externalId;
          return (link, id);
        }
        catch (Exception exc)
        {
          throw exc;
        }

      }
      catch (Exception exc)
      {
        Debug.WriteLine(exc.Message);
        return default;
      }
    }
    public async Task<Address> ParseAddress(string address)
    {
      Address result = new();
      var token = "aa9b411a0851eb8344a4fe5fc9cfc272a994c6ab";
      var secret = "15178f7ea73ba5e799adde3745bae8d9dc5de767";
      var api = new CleanClientAsync(token, secret);
      var response = await api.Clean<Dadata.Model.Address>(address);
      result.countryIso = response.country_iso_code;
      result.text = String.IsNullOrEmpty(response.result) ? address : response.result;
      result.streetType = response.street_type_full;
      result.cityType = response.city_type_full;
      result.city = response.city;
      result.index = response.postal_code;
      result.building = response.house;
      result.street = response.street;
      result.flat = response.flat;
      result.floor = response.floor != null ? int.Parse(response.floor) : null;
      result.region = response.region_with_type;
      result.block = response.entrance != null ? int.Parse(response.entrance) : null;
      result.metro = response.metro?.FirstOrDefault()?.name;
      return result;
    }

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
          //JObject keyValuePairs = JObject.Parse(response.GetRawResponse());
          //var jobj = JObject.Parse(response.GetRawResponse());
          //try
          //{
          //  var props = keyValuePairs.SelectTokens("$..offers[*].properties", true).Where(t => t.Count() != 0).ToArray();
          //  var json = "[";

          //  int index = 0;
          //  foreach (JToken jToken in props)
          //  {
          //    json += jToken.ToString();
          //    index++;
          //    json += index != props.Length ? "," : "]";
          //  }


          //  //var dynamicProps = JsonConvert.DeserializeObject<dynamic>(props?.ToString());

          //  var tempProps = JsonConvert.DeserializeObject(json) as dynamic[];
          //}
          //catch (Exception)
          //{
          //  throw;
          //}

          ////JToken jToken = props


          Regex regex = new Regex("(?<=\"offers\":)\\[.+?\\](?=,\"updatedAt\")");
          List<T>? offers = new List<T>();
          var matches = regex.Matches(response.GetRawResponse());


          //PortalSerializationBinder serializationBinder = new PortalSerializationBinder();
          //serializationBinder.BindToName(typeof(object));

          //serializationBinder.BindToType(typeof(Offer).Assembly.FullName, typeof(Offer).FullName);

          JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
          {
            //ContractResolver = new PortalContractResolver(),
            //SerializationBinder = serializationBinder
          };

          foreach (Match match in matches)
          {
            List<T>? offer = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(match.Value, jsonSerializerSettings);
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

  public class PortalSerializationBinder : DefaultSerializationBinder
  {
    public override void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
    {
      base.BindToName(serializedType, out assemblyName, out typeName);
    }

    public override Type BindToType(string? assemblyName, string typeName)
    {
      Type result = base.BindToType(assemblyName, typeName);
      return result;
    }
  }

  public class PortalContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
  {
    public override JsonContract ResolveContract(Type type)
    {
      var contract = base.ResolveContract(type);
      return contract;
    }

    protected override string ResolveDictionaryKey(string dictionaryKey)
    {
      if (dictionaryKey == null) return null;

      return base.ResolveDictionaryKey(dictionaryKey);
    }

    protected override Newtonsoft.Json.Serialization.JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
      var prop = base.CreateProperty(member, memberSerialization);
      var temp = this.ResolvePropertyName("isNumeric");


      if (!prop.Writable)
      {
        var property = member as PropertyInfo;
        if (property != null)
        {
          var hasPrivateSetter = property.GetSetMethod(true) != null;
          prop.Writable = hasPrivateSetter;
        }
      }

      return prop;
    }

    protected override List<MemberInfo> GetSerializableMembers(Type objectType)
    {
      return base.GetSerializableMembers(objectType);
    }

    protected override IList<Newtonsoft.Json.Serialization.JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
      return base.CreateProperties(type, memberSerialization);
    }
  }
}
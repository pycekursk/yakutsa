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
          throw exp;
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
      return await this.CreateInvoice(createInvoice);
    }

    public Response<T> GetResponse<T>()
    {
      Response response = null;
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
        order.manager = createOrderObject.manager;
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

        string link = await CreatePayment(resultOrder.payments.LastOrDefault().id, host);
        string id = resultOrder.externalId;
        return (link, id);
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

      //HttpClient httpClient = new HttpClient();
      //httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", "aa9b411a0851eb8344a4fe5fc9cfc272a994c6ab");
      //httpClient.DefaultRequestHeaders.Add("X-Secret", "15178f7ea73ba5e799adde3745bae8d9dc5de767");
      //string cleanAddressUrl = "https://cleaner.dadata.ru/api/v1/clean/address";
      //string suggestionsUrl = "https://suggestions.dadata.ru/suggestions/api/4_1/rs/suggest/address";
      //HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, suggestionsUrl);

      //var json = System.Text.Json.JsonSerializer.Serialize(new { query = address });
      //requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");
      //HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
      //var responseString = await response.Content.ReadAsStringAsync();
      //var responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseString).suggestions[0].data;

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
      public User manager { get; set; }
      public Customer customer { get; set; }
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
          //WriteIndented = true,
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
            Debug.WriteLine(exc.Message);
          }
        }
        return result;
      }
    }
  }
}
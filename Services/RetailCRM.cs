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
using System.Xml.Serialization;
using yakutsa.Services.Dalli;
using RetailCRMCore.Models.Base;
using yakutsa.Services.Dalli.Models;

namespace yakutsa.Services
{
    public class CreateInvoice
    {
        public int paymentId { get; set; }
        public string returnUrl { get; set; }
        public int amount { get; set; }
    }

    public partial class RetailCRM
    {
        const string _key = "h0NsTuUjjscl7JG5SEk6NZPJPuw4dryy";
        const string _url = "https://yakutsa.retailcrm.ru";
        Client _client;
        //Sdek.ApiClient _sdek;
        List<IDeliveryModule> deliveryModules { get; set; } = new();

        public RetailCRM()
        {
            _client = new Client(_url, _key);
            deliveryModules.Add(new yakutsa.Services.Dalli.ApiClient { Code = "dalli-service" });
        }

        public string GetOrdersJson()
        {
            return _client.OrdersList().GetRawResponse();
        }

        public string GetPaymentTypesJson()
        {
            return _client.PaymentTypes().GetRawResponse();
        }

        public string GetDeliveryTypesJson()
        {
            return _client.DeliveryTypes().GetRawResponse();
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

        public Order OrderUpdate(Order order)
        {
            _client.OrderUpdate(order);
            return order;
        }

        public async Task<string> CreatePayment(int id, string host)
        {


            var createInvoice = new CreateInvoice
            {
                paymentId = id,
                returnUrl = $"https://{host}/PaymentReturn",
                amount = 4000,
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

        public Task<string> GetDeliveryTariffs(Address address, Cart cart, string code = "dalli-service")
        {
            return Task.Run<string>(() =>
            {
                string result = String.Empty;
                //HttpClient httpClient = new HttpClient();
                //httpClient.DefaultRequestHeaders
                //.Accept
                //.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                //var requestXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><deliverycost><auth token=\"cd94b1b2049e7e35db64ec756bc49073\"></auth><partner>SDEK</partner><townto>Курск</townto><oblname>Курская</oblname><weight>0.8</weight><price>7000</price><inshprice>7000</inshprice><cashservices>NO</cashservices><length>50</length><width>50</width><height>50</height><output>x2</output></deliverycost>";
                //var stringContent = new StringContent(requestXml, Encoding.UTF8, "application/xml");
                //var response = await httpClient.PostAsync("https://api.dalli-service.com/v1/", stringContent);
                //result = await response.Content.ReadAsStringAsync();

                //var deliveryModule = this.deliveryModules.FirstOrDefault(dm => dm.Code == code);
                //var pvzList = (deliveryModule as Dalli.ApiClient).GetPVZList(address);

                return result;
            });
        }

        public Task<List<DeliveryCost>> DeliveryCalculate(Address address, Cart cart, string code = "dalli-service")
        {
            return Task.Run<List<yakutsa.Services.Dalli.Models.DeliveryCost>>(() =>
          {
              var deliveryModule = this.deliveryModules.FirstOrDefault(dm => dm.Code == code);
              var pvzList = (deliveryModule as Dalli.ApiClient).Calculate(address, cart);
              return pvzList;
          });
        }

        //undone
        //public Task<List<Pvzlist>> GetPVZList(Address address, string code = "dalli-service")
        //{
        //    return Task.Run<List<Pvzlist>>(() =>
        //    {
        //        var deliveryModule = this.deliveryModules.FirstOrDefault(dm => dm.Code == code);
        //        var pvzList = (deliveryModule as Dalli.ApiClient).GetPVZList(address);
        //        return pvzList;
        //    });
        //}

        public Order? GetOrder(string number)
        {
            var filter = new Dictionary<string, object>();
            filter.Add("numbers", new object[] { number });
            var response = _client.OrdersList(filter);
            try
            {
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject(response.GetRawResponse()) as JObject;
                var orders = obj?.GetValue("orders") as JArray;

                if (orders == null)
                    throw new Exception("Ошибка получения обьекта заказа");

                if (orders.Children().Count() == 0)
                    throw new NullReferenceException("Заказ не найден");

                var ord = orders[0];
                var json = orders?.ToString();
                var order = Newtonsoft.Json.JsonConvert.DeserializeObject<Order>(ord.ToString());


                obj = (JObject?)(Newtonsoft.Json.JsonConvert.DeserializeObject(_client.User((int)order?.managerId).GetRawResponse()) as JObject)?.GetValue("user");
                var manager = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(obj?.ToString());
                order.manager = manager;

                //obj = (JObject?)(((JObject?)(Newtonsoft.Json.JsonConvert.DeserializeObject(_client.DeliveryTypes().GetRawResponse()) as JObject)?.GetValue("deliveryTypes"))?.GetValue(order.delivery.code));


                filter = new Dictionary<string, object>();
                object[] filterValues = new object[order.items.Count()];
                for (int i = 0; i < filterValues.Length; i++)
                {
                    filterValues[i] = order.items[i].offer.id;
                }
                filter.Add("offerIds", filterValues);


                var jArray = (JArray?)(Newtonsoft.Json.JsonConvert.DeserializeObject(_client.StoreProducts(filter).GetRawResponse()) as JObject)?.GetValue("products");
                var products = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Product>>(jArray?.ToString());

                foreach (OrderProduct orderProduct in order.items)
                {
                    orderProduct.offer.product = products?.FirstOrDefault(prod => prod.offers?.FirstOrDefault(offr => offr.id == orderProduct.offer.id) != null);
                    orderProduct.summ = orderProduct.quantity * orderProduct.initialPrice;
                }
                return order;
            }
            catch (Exception exc)
            {
                throw (exc);
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

        //public Task<Response<T>> GetResponseAsync<T>()
        //{
        //    return Task.Run(GetResponse<T>);
        //}

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



        //public async Task<string> GetDeliveryTypes()
        //{
        //  string result = String.Empty;
        //  HttpClient httpClient = new HttpClient();
        //  httpClient.DefaultRequestHeaders
        //  .Accept
        //  .Add(new MediaTypeWithQualityHeaderValue("application/xml"));
        //  var someXmlString = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><services><auth token=\"cd94b1b2049e7e35db64ec756bc49073\"></auth></services>";
        //  var stringContent = new StringContent(someXmlString, Encoding.UTF8, "application/xml");
        //  var response = await httpClient.PostAsync("https://api.dalli-service.com/v1/", stringContent);
        //  result = await response.Content.ReadAsStringAsync();
        //  return result;
        //}


        public async Task<(string link, string id)> OrderCreate(CreateOrderObject createOrderObject, string host, bool isDevelopment = false)
        {
            createOrderObject.externalId = Guid.NewGuid().ToString();

            var json = _client.OrdersCreate(createOrderObject, "YAKUTSA.RU").GetRawResponse();

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


        public class Rootobject
        {
            public bool success { get; set; }
            public int id { get; set; }
            public Order order { get; set; }
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


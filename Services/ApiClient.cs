using Microsoft.AspNetCore.Http.Extensions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RetailCRMCore.Models;
using RetailCRMCore.Versions.V5;

using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

using yakutsa.Extensions;

namespace yakutsa.Services.Sdek
{
  public class ApiClient
  {
    public IntegrationModule IntegrationModule { get; set; }
    public Models.OAuth OAuth
    {
      get
      {
        var span = DateTime.Now.Subtract(oAuth.Created);
        if (span.Milliseconds >= oAuth.expires_in)
          GetOAuth().ContinueWith(t => oAuth = t.Result).Wait();
        return oAuth;
      }
      set
      {
        oAuth = value;
      }
    }

    public string client_secret { get; set; }
    public string client_id { get; set; }
    string urlReal = "https://api.cdek.ru/v2/";
    string urlDemo = "https://api.edu.cdek.ru/v2/";

    Timer timer;
    HttpClient _httpClient = new HttpClient();
    Client _client;
    private Models.OAuth oAuth;
    

    public Models.ApiAction[] ApiActions { get; set; } = new Models.ApiAction[] {
      new Models.ApiAction
      {
        Name = "OAuth",
        Path = "oauth/token?parameters"
      },
      new Models.ApiAction
      {
        Name = "Deliverypoints",
        Path = "deliverypoints"
      },
      new Models.ApiAction
      {
        Name = "CalculationByTariffCode",
        Path = "calculator/tariff"
      },
      new Models.ApiAction
      {
        Name = "Delivery",
        Path = "delivery"
      },
      new Models.ApiAction
      {
        Name = "Cities",
        Path = "location/cities"
      },
      new Models.ApiAction
      {
        Name = "Regions",
        Path = "location/regions"
      },
      new Models.ApiAction
      {
        Name = "CalculationByTariffList",
        Path = "calculator/tarifflist"
      }
    };

    public Enums.WorkMode WorkMode { get; set; }

    public ApiClient(string code, Client client)
    {
      _client = client;
      var obj = JObject.Parse(_client.IntegrationsSettingGet(code).GetRawResponse()).Property("integrationModule").Value.ToString();
      IntegrationModule = JsonConvert.DeserializeObject<IntegrationModule>(obj);

      //IntegrationModule.apiKey = "HFkSzd2CAuRPHcvGysiHEb8gXjEXcZZz";
      //IntegrationModule.clientId = "5d69b605-1d40-4763-a8cb-0ce7ea3d2aa9";
      //IntegrationModule.integrations.delivery.settings.shipmentPoints = IntegrationModule.integrations.delivery.settings.shipmentPoints.Where(sp => !string.IsNullOrEmpty(sp.shipmentPointLabel)).ToArray();
      //var resp = _client.IntegrationsSettingsEdit(IntegrationModule).GetRawResponse();
    }

    public ApiClient(string clientId, string clientSecret, Enums.WorkMode workMode)
    {
      this.client_id = clientId;
      this.client_secret = clientSecret;
      WorkMode = workMode;
      if (oAuth == null) Authorization(client_id, client_secret);
    }

    public void Authorization(string client_id, string client_secret)
    {
      GetOAuth().ContinueWith(t => OAuth = t.Result).Wait();
      _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + OAuth.access_token);
    }

    async void TimerCallback(object obj)
    {
      OAuth = await GetOAuth();
    }

    Task<Models.OAuth> GetOAuth()
    {
      return Task.Run<Models.OAuth>(async () =>
      {
        string grant_type = "client_credentials";
        var action = ApiActions.FirstOrDefault(a => a.Name == "OAuth").Path;
        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, this.WorkMode == Enums.WorkMode.Demo ? urlDemo + action : urlReal + action);
        _httpClient.DefaultRequestHeaders
        .Accept
        .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        requestMessage.Content = new StringContent($"{nameof(client_id)}={client_id}&{nameof(client_secret)}={client_secret}&{nameof(grant_type)}={grant_type}",
                                   Encoding.UTF8,
                                   "application/x-www-form-urlencoded");

        HttpResponseMessage responseMessage = await _httpClient.SendAsync(requestMessage);

        string responseJson = await responseMessage.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<Models.OAuth>(responseJson);
      });
    }

    public Task<Models.TariffsCollection> Calculate(Models.CalculationOptions calculationOptions, int? code = null)
    {
      return Task.Run<Models.TariffsCollection>(async () =>
      {
        string action = code != null ? "CalculationByTariffCode" : "CalculationByTariffList";
        action = ApiActions.FirstOrDefault(a => a.Name == action).Path;
        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, (this.WorkMode == Enums.WorkMode.Demo ? urlDemo : urlReal) + action);

        string requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(calculationOptions, new ConverterOptions());

        requestMessage.Content = new StringContent(requestJson,
                                   Encoding.UTF8,
                                   "application/json");

        HttpResponseMessage responseMessage = await _httpClient.SendAsync(requestMessage);
        string responseJson = await responseMessage.Content.ReadAsStringAsync();
        Models.TariffsCollection result = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.TariffsCollection>(responseJson);
        return result;
      });
    }

    public Task<List<Models.Location>> Cities(Models.AreaRequestOptions requestOptions)
    {
      return Task.Run<List<Models.Location>>(async () =>
      {
        var type = requestOptions.GetType();
        var props = type.GetProperties();
        Dictionary<string, string> values = new Dictionary<string, string>();
        foreach (PropertyInfo prop in props)
        {
          var value = requestOptions.GetPropertyValue(prop.Name)?.ToString();
          if (value == null) continue;
          values.Add(prop.Name.ToString(), value);
        }
        QueryBuilder query = new QueryBuilder(values);
        string action = ApiActions.FirstOrDefault(a => a.Name == "Cities").Path;
        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, (this.WorkMode == Enums.WorkMode.Demo ? urlDemo : urlReal) + action + $"/{query}");
        HttpResponseMessage responseMessage = await _httpClient.SendAsync(requestMessage);
        string responseJson = await responseMessage.Content.ReadAsStringAsync();
        List<Models.Location> cities = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.Location>>(responseJson);
        return cities;
      });
    }

    public Task<List<Models.Location>> Regions()
    {
      return Task.Run<List<Models.Location>>(async () =>
      {
        string action = ApiActions.FirstOrDefault(a => a.Name == "Regions").Path;
        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, (this.WorkMode == Enums.WorkMode.Demo ? urlDemo : urlReal) + action);
        HttpResponseMessage responseMessage = await _httpClient.SendAsync(requestMessage);
        string responseJson = await responseMessage.Content.ReadAsStringAsync();
        List<Models.Location> regions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.Location>>(responseJson);
        return regions;
      });
    }
  }

  public class ConverterOptions : JsonSerializerSettings
  {
    public ConverterOptions()
    {
      this.DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'sszz00";
    }
  }
}

namespace yakutsa.Services.Sdek.Models
{
  public class ApiAction
  {
    public string Name { get; set; }
    public string Path { get; set; }
  }

  public class OAuth
  {
    public string access_token { get; set; }
    public string token_type { get; set; }
    public int expires_in { get; set; }
    public string scope { get; set; }
    public string jti { get; set; }
    public DateTime Created { get; set; }

    public OAuth() => Created = DateTime.Now;

    public override string ToString() => base.ToString().ToLower();
  }

  public class Location
  {
    public int code { get; set; }
    public string city { get; set; }
    public string country_code { get; set; }
    public string country { get; set; }
    public string region { get; set; }
    public int region_code { get; set; }
    public string sub_region { get; set; }
    public string[] postal_codes { get; set; }
    public float longitude { get; set; }
    public float latitude { get; set; }
    public string time_zone { get; set; }


  }

  public class CalculationOptions
  {
    public int code { get; set; }
    public string type { get; set; }
    public DateTime date { get; set; }
    public string currency { get; set; }
    public string lang { get; set; }
    public Location from_location { get; set; }
    public Location to_location { get; set; }
    public Package[] packages { get; set; }
  }

  public class AreaRequestOptions
  {
    public string country_codes { get; set; }
    public int? region_code { get; set; }
    public int? code { get; set; }
    public string kladr_region_code { get; set; }
    public string fias_region_guid { get; set; }
    public string kladr_code { get; set; }
    public string fias_guid { get; set; }
    public string postal_code { get; set; }
    public string city { get; set; }
    public int? size { get; set; }
    public int? page { get; set; }
    public string lang { get; set; }
  }

  public class Package
  {
    public int height { get; set; }
    public int length { get; set; }
    public int weight { get; set; }
    public int width { get; set; }
  }

  public class TariffsCollection
  {
    [JsonProperty("tariff_codes")]
    public Tariff[] Tariffs { get; set; }
  }

  public class Error
  {
    [Display(Name = "Код")]
    public string code { get; set; }

    [Display(Name = "Описание")]
    public string message { get; set; }
  }

  public class Status
  {
    [Display(Name = "Код статуса")]
    public string code { get; set; }

    [Display(Name = "Статус")]
    public string name { get; set; }

    [Display(Name = "Дата и время")]
    public DateTime date_time { get; set; }

    [Display(Name = "Дополнительный код")]
    public string reason_code { get; set; }

    [Display(Name = "Место")]
    public string city { get; set; }
  }

  public class Request
  {
    public string request_uuid { get; set; }
    public string type { get; set; }
    public DateTime date_time { get; set; }
    public string state { get; set; }
    public Error[] errors { get; set; }
  }
}

namespace yakutsa.Services.Sdek.Enums
{
  public enum WorkMode
  {
    Real,
    Demo = 0
  }

  public enum Currency
  {
    Ruble = 1,
    Tenge = 2,
    Dollar = 3,
    Euro = 4,
    Pound_sterling = 5,
    Belarusian_ruble = 7,
    Yuan = 6,
    Hryvnia = 8,
    Kyrgyzstani_som = 9,
    Armenian_dram = 10,
    Turkish_lira = 11,
    Won = 13,
    Dirham = 14,
    Thai_baht = 12,
    Sum = 15,
    Tugrik = 16,
    Zloty = 17,
    Manat = 18,
    Lari = 19,
    Japanese_yen = 55,
    Unknown = 0
  }
}
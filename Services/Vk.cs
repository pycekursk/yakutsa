
using System.Text;

using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;

namespace yakutsa.Services
{
  public enum Scopes
  {
    market = 0,
  }

  public enum ResponseType
  {
    code,
    token
  }

  public class Vk
  {
    static Vk? _instance;
    private static object syncRoot = new Object();

    const long _applicationId = 8158232;
    const long _groupId = -188269001;

    public string WallJson { get; set; }

    public string Code { get => code; private set => code = value; }
    public string Access_token { get => access_token; set => access_token = value; }
    public string client_secret = "sSaO5nEblYPYbv44r5r8";
    public string _redirect_uri = @"https://yakutsa.ru/Admin/Vk/Code";

    const string _serviceToken = "d5812709d5812709d5812709dfd5fd5b11dd581d5812709b7f25c86441788da50ab5927";
    const string _groupToken = "0c863021b012ce3ee4f8417dc0dbf8e377ccfdd681a1e8ce4de60c8fa940685909f05ad9af024b0aa37e1";


    public VkApi Client = new VkApi();
    private string access_token;
    private string code;

    public Vk()
    {
      Client.Authorize(new ApiAuthParams
      {
        AccessToken = "507f821fb7688395d37e67836ca6edaf388cfd8d5a6c4da03fa381f40decc6b0f8fc488b8296d0a36ad5a",//_groupToken,// "507f821fb7688395d37e67836ca6edaf388cfd8d5a6c4da03fa381f40decc6b0f8fc488b8296d0a36ad5a",
        Settings = Settings.All
      });

      var products = Client.Markets.Get(_groupId);
    }

    public string GetWall()
    {
      var wall = Client.Wall.Get(new VkNet.Model.RequestParams.WallGetParams { OwnerId = _groupId });
      return Newtonsoft.Json.JsonConvert.SerializeObject(wall);
    }

    string CodeRequest()
    {
      string result = string.Empty;
      Task.Run(async () =>
      {
        HttpClient httpClient = new HttpClient();
        var response = await httpClient.GetAsync($"https://oauth.vk.com/oauth/authorize?client_id={_applicationId}&display=page&redirect_uri={_redirect_uri}&scope=market");
        result = await response.Content.ReadAsStringAsync();
      }).ContinueWith(t => t).Wait();
      return result;
    }

    public static Vk Initialize()
    {
      if (_instance == null)
        lock (syncRoot)
        {
          if (_instance == null)
          {
            _instance = new Vk();
            _instance.Code = _instance.CodeRequest();
          }
        }

      return _instance;
    }

    public string? GetComments()
    {
      string result;
      try
      {
        var comments = Client.Markets.GetComments(new VkNet.Model.RequestParams.Market.MarketGetCommentsParams { OwnerId = 188269001 });
        result = Newtonsoft.Json.JsonConvert.SerializeObject(comments);
      }
      catch (Exception exc)
      {
        result = exc.Message;
      }
      return result;
    }

    //public string GetProducts()
    //{
    //  var groups = _client.Markets.Get();
    //  return Newtonsoft.Json.JsonConvert.SerializeObject(groups);
    //}
  }
}
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace yakutsa.Services
{
  public class Vk
  {
    const string _login = "89207048884";
    const string _password = "6m7sd38L";
    const string _privateKey = "A8NiDURC0c0Vrf7zfJ3Q";
    public void Initialize()
    {
     // GetToken(callbackUrl);

      var api = new VkApi();


      api.Authorize(new ApiAuthParams
      {
        ApplicationId = 8127205,
        Login = _login,
        Password = _password,
        Settings = Settings.Market
      });
      //var res = api.Groups.Get(new GroupsGetParams());
    }

    void GetToken(string callBackUrl)
    {
      HttpClient httpClient = new HttpClient();
      var resp = httpClient.GetAsync($"https://oauth.vk.com/access_token?client_id={8127205}&client_secret={_privateKey}&redirect_uri={callBackUrl}");
    }
  }
}

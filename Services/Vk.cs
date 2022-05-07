
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        private int id;
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get => id; set => id = value == 0 ? id : value; }

        [NotMapped]
        static Vk? _instance;

        [NotMapped]
        private static object syncRoot = new Object();

        public long ApplicationId { get; set; } = 8158232;
        public long GroupId { get; set; } = -188269001;

        public string? WallJson { get; set; }
        public string? Code { get => code; private set => code = value; }
        public string? AccessToken { get => accessToken; set => accessToken = value; }
        public string? ClientSecret { get => clientSecret; set => clientSecret = value; }
        public string? RedirectUrl { get => redirectUrl; set => redirectUrl = value; }
        public string? ServiceToken { get; set; } = "d5812709d5812709d5812709dfd5fd5b11dd581d5812709b7f25c86441788da50ab5927";
        public string? GroupToken { get; set; } = "0c863021b012ce3ee4f8417dc0dbf8e377ccfdd681a1e8ce4de60c8fa940685909f05ad9af024b0aa37e1";

        [NotMapped]
        public VkApi Client = new VkApi();

        private string accessToken;
        private string code;
        private string clientSecret;
        private string redirectUrl = @"https://yakutsa.ru/Admin/Vk/Code";

        public Vk()
        {
            Client.Authorize(new ApiAuthParams
            {
                AccessToken = GroupToken,// "507f821fb7688395d37e67836ca6edaf388cfd8d5a6c4da03fa381f40decc6b0f8fc488b8296d0a36ad5a",
                Settings = Settings.All
            });

            //var products = Client.Markets.Get(_groupId);
        }

        public string GetWall()
        {
            string result = string.Empty;
            try
            {
                var wall = Client.Wall.Get(new VkNet.Model.RequestParams.WallGetParams { OwnerId = GroupId });
                result = Newtonsoft.Json.JsonConvert.SerializeObject(wall);
            }
            catch (Exception exc)
            {
                return exc.Message;
            }
            return result;
        }

        string CodeRequest()
        {
            string result = string.Empty;
            Task.Run(async () =>
            {
                HttpClient httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"https://oauth.vk.com/oauth/authorize?client_id={ApplicationId}&display=page&redirect_uri={RedirectUrl}&scope=market");
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
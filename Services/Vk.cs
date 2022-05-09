
using RetailCRMCore.Models;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;

using yakutsa.Data;

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

        [Display(Name = "Id приложения")]
        public long ApplicationId { get; set; }

        [Display(Name = "Id группы")]
        public long GroupId { get; set; }

        [Action("CodeRequest")]
        [Display(Name = "Код ответа")]
        public string? Code { get => code; private set => code = value; }

        [Display(Name = "Ключ доступа")]
        public string? AccessToken { get => accessToken; set => accessToken = value; }

        [Display(Name = "Защищённый ключ")]
        public string? ClientSecret { get => clientSecret; set => clientSecret = value; }

        [Display(Name = "Адрес ответа")]
        public string? RedirectUrl { get => redirectUrl; set => redirectUrl = value; }

        [Display(Name = "Сервисный ключ")]
        public string? ServiceToken { get; set; }

        [Display(Name = "Ключ группы")]
        public string? GroupToken { get; set; }

        [NotMapped]
        public VkApi Client = new VkApi();

        private string? accessToken;
        private string? code;
        private string? clientSecret;
        private string? redirectUrl;

        public Vk()
        {

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

        public string CodeRequest()
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
                    using (var ctx = new ApplicationDbContext(new Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext>()))
                    {
                        _instance = ctx.Vk.FirstOrDefault();

                        if (_instance == null)
                        {
                            _instance = new Vk();
                            ctx.Vk.Add(_instance);
                            ctx.SaveChanges();
                            _instance = ctx.Vk.First();
                        }
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
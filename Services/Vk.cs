
using Microsoft.EntityFrameworkCore.Metadata.Internal;

using RetailCRMCore.Models;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.RegularExpressions;

using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;

using yakutsa.Data;
using yakutsa.Extensions;
using yakutsa.Models;

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

    public enum OAuthType
    {
        Group = 3,
        User = 2,
        Unknown = 1
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
        public ulong ApplicationId { get; set; }

        [Display(Name = "Id группы")]
        public long GroupId { get; set; }

        [Display(Name = "Id пользователя")]
        public long UserId { get; set; }

        [Display(Name = "email")]
        public string Login { get; set; }

        [Action("CodeRequest")]
        [Display(Name = "Код ответа")]
        public string? Code { get => code; private set => code = value; }

        [NotMapped]
        [Action("GetComments")]
        [Display(Name = "Комментарии")]
        public List<ProductComments> ProductsComments { get => productsComments; set => productsComments = value; }

        [Display(Name = "Ключ группы")]
        public string? GroupAccessToken { get => groupAccessToken; set => groupAccessToken = value; }

        [Display(Name = "Ключ пользователя")]
        public string? UserAccessToken { get => userAccessToken; set => userAccessToken = value; }

        [Display(Name = "Защищённый ключ")]
        public string? ClientSecret { get => clientSecret; set => clientSecret = value; }

        [Display(Name = "Адрес ответа")]
        public string? RedirectUrl { get => redirectUrl; set => redirectUrl = value; }

        [Display(Name = "Сервисный ключ")]
        public string? ServiceToken { get; set; }

        [Display(Name = "Подключено")]
        [Action("Connect")]
        public bool Connected { get; internal set; }

        [Display(Name = "Тип ключа")]
        OAuthType OAuthType { get; set; }

        [NotMapped]
        public VkApi Client = new VkApi();

        private string? groupAccessToken;
        private string? code;
        private string? clientSecret;
        private string? redirectUrl;
        private List<ProductComments> productsComments;
        private string? userAccessToken;

        public Vk()
        {

        }

        public bool Connect()
        {
            OAuthType = OAuthType.User;
            ApiAuthParams authParams = new()
            {
                Settings = Settings.All
            };

            if (OAuthType == OAuthType.User)
            {
                authParams.UserId = UserId;
                authParams.AccessToken = UserAccessToken;
                authParams.Login = Login;
            }

            Client.Authorize(authParams);

            Connected = Client.IsAuthorized;


            return Connected;
        }

        public string GetWall()
        {
            if (!Connected) Connect();
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
                //var response = await httpClient.GetAsync($"https://oauth.vk.com/oauth/authorize?client_id={ApplicationId}&display=page&redirect_uri={RedirectUrl}&scope=market");
                var response = await httpClient.GetAsync("https://oauth.vk.com/authorize?client_id=8158232&display=page&redirect_uri=https://oauth.vk.com/blank.html&group_ids=188269001&scope=messages&response_type=code&v=5.131");
                result = await response.Content.ReadAsStringAsync();
            }).ContinueWith(t => t).Wait();
            return result;
        }

        public List<ProductComments> GetComments()
        {
            if (!Connected) Connect();
            var products = Client.Markets.Get(GroupId);
            foreach (var product in products)
            {
                Client.Markets.GetComments(new VkNet.Model.RequestParams.Market.MarketGetCommentsParams { OwnerId = GroupId, ItemId = product.Id.Value })
                    .ToList()
                    .ForEach(c => ProductsComments.Add(new ProductComments { Product = product, Comments = c.Comments.ToList() }));
            }
            return ProductsComments;
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
    }

    public class ProductComments : BaseModel
    {
        public Market? Product { get; set; }
        public List<Comment>? Comments { get; set; }
    }
}
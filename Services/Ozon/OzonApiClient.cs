using yakutsa.Data;
using yakutsa.Models;
using yakutsa.Services.Ozon.Models;


using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using System.Net.Http.Headers;
using System.Text;

using Attribute = yakutsa.Services.Ozon.Models.Attribute;
using RetailCRMCore.Models;

namespace yakutsa.Services.Ozon
{
    public class OzonApiClient
    {
        static OzonApiClient? instance;
        static object syncRoot = new object();
        ITWHttpClient httpClient;

        OzonSettings? OzonSettings;


        public static OzonApiClient Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance != null) return instance;

                        using (var ctx = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>()))
                        {
                            var ozonSettings = ctx.OzonSettings.FirstOrDefault();
                            instance = new OzonApiClient(ozonSettings!);
                        }
                    }
                }
                return instance;
            }
        }

        OzonApiClient(OzonSettings ozonSettings)
        {
            OzonSettings = ozonSettings;

            string apiKey = ozonSettings.ApiKey;
            string clientId = ozonSettings.ClientId;

            using (var ctx = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>()))
            {
                OzonSettings = ctx.OzonSettings.FirstOrDefault();
                OzonSettings = OzonSettings ?? new OzonSettings();
            }

            if (apiKey == null) throw new ArgumentNullException(nameof(apiKey));
            if (clientId == null) throw new ArgumentNullException(nameof(clientId));

            httpClient = new ITWHttpClient();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Client-Id", clientId);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Api-Key", apiKey);
        }

        public async Task<List<OzonCategory>?> GetCategories()
        {
            string url = "https://api-seller.ozon.ru/v2/category/tree";
            var requestData = new CategoryRequestData { Language = "DEFAULT" };
            var response = await httpClient.PostAsync(url, JsonContent.Create(requestData));
            var json = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<Ozon.Models.CategoriesResponseData>(json);
            var categories = responseData?.Categories;

            //if (categories != null)
            //  using (var ctx = new ApplicationDbContext())
            //  {
            //    ctx.OzonCategories.RemoveRange(ctx.OzonCategories);
            //    ctx.OzonCategories.AddRange(categories);
            //    await ctx.SaveChangesAsync();
            //  }

            return categories;
        }

        public async Task<OzonCategoryAttributesResponseData?> GetCategoryAttributes(IEnumerable<OzonCategory> ozonCategory, OzonCategoryAttributeType ozonCategoryAttributeType = OzonCategoryAttributeType.All)
        {
            if (ozonCategory != null && ozonCategory.Count() != 0)
                return await GetCategoryAttributes(ozonCategory.Select(c => c.CategoryId));
            else return default;
        }

        public async Task<OzonCategoryAttributesResponseData?> GetCategoryAttributes(IEnumerable<long> ozonCategoryId, OzonCategoryAttributeType ozonCategoryAttributeType = OzonCategoryAttributeType.All)
        {
            string url = "https://api-seller.ozon.ru/v3/category/attribute";
            var requestData = new { category_id = ozonCategoryId, attribute_type = ozonCategoryAttributeType.ToString().ToUpper(), language = "DEFAULT" };
            var response = await httpClient.PostAsync(url, JsonContent.Create(requestData));
            var json = await response.Content.ReadAsStringAsync();
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<OzonCategoryAttributesResponseData>(json);
            return result;
        }

        public async Task GetAttributeValue(long categoryId, long attributeId)
        {
            string url = "https://api-seller.ozon.ru/v2/category/attribute/values";
            var requestData = new { attribute_id = attributeId, category_id = categoryId, language = "DEFAULT", last_value_id = 0, limit = 5000 };
            var jsonContent = JsonContent.Create(requestData);
            var response = await httpClient.PostAsync(url, jsonContent);
            var json = await response.Content.ReadAsStringAsync();
            // var result = Newtonsoft.Json.JsonConvert.DeserializeObject<OzonCategoryAttributesResponseData>(json);
        }


        public async Task<OzonProductsResponseData?> GetProducts()
        {
            string url = "https://api-seller.ozon.ru/v2/product/list";
            var requestData = new { Limit = 1000 };
            var response = await httpClient.PostAsync(url, JsonContent.Create(requestData));
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<OzonProductsResponseData?>(json);
            return data;
        }

        public async Task<OzonProductInfoResponseData?> GetProductInfo(OzonProduct ozonProduct)
        {
            string url = "https://api-seller.ozon.ru/v2/product/info";
            var requestData = new { product_id = ozonProduct.ProductId };
            var response = await httpClient.PostAsync(url, JsonContent.Create(requestData));
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<OzonProductInfoResponseData?>(json);
            return data;
        }

        public async Task<LimitInfoResponseData?> GetLimitInfo()
        {
            string url = "https://api-seller.ozon.ru/v2/product/info/limit";
            var requestData = new { };
            var response = await httpClient.PostAsync(url, JsonContent.Create(requestData));
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<LimitInfoResponseData?>(json);
            return data;
        }

        public async Task<ProductImportResponseData?> ProductImport(List<Product> products, long categoryId)
        {
            string url = "https://api-seller.ozon.ru/v2/product/import";

            //var attributes = await GetCategoryAttributes(new long[] { categoryId }, OzonCategoryAttributeType.All);
            //await GetAttributeValue(categoryId, 4295);
            //foreach (var item in attributes.CategoriesWithAttributes)
            //{
            //  item.Attributes.ForEach(async attr =>
            //  {
            //    await GetAttributeValue(item.CategoryId, attr.Id);
            //  });
            //}

            var requestData = new OzonProductImportData();

            foreach (Product product in products)
            {
                if (product == null) continue;
                var images = product.images?.ToList();
                foreach (var offer in product.offers)
                {
                    OzonProductImportItem ozonProductInfo = new OzonProductImportItem();

                    if (images?.Count > 0)
                    {
                        ozonProductInfo.Images = images.Where(i => i.Size == ImageSize.l).Select(i => i.Url).ToList()!;
                    }

                    Attribute brandAttribute = new Attribute { ComplexId = 0, Id = 31, Values = new List<Value> { new Value { ValueValue = "YAKUTSA" } } };
                    Attribute nameAttribute = new Attribute { ComplexId = 0, Id = 4180, Values = new List<Value> { new Value { ValueValue = product.name } } };
                    Attribute descriptionAttribute = new Attribute { ComplexId = 0, Id = 4191, Values = new List<Value> { new Value { ValueValue = product.description } } };
                    Attribute typeAttribute = new Attribute { ComplexId = 0, Id = 8229, Values = new List<Value> { new Value { DictionaryValueId = 93253 } } };
                    Attribute countryAttribute = new Attribute { ComplexId = 0, Id = 4389, Values = new List<Value> { new Value { DictionaryValueId = 90295 } } };
                    Attribute seasonAttribute = new Attribute { ComplexId = 0, Id = 4495, Values = new List<Value> { new Value { DictionaryValueId = 30938 } } };
                    Attribute targetAttribute = new Attribute { ComplexId = 0, Id = 9163, Values = new List<Value> { new Value { DictionaryValueId = 22880 }, new Value { DictionaryValueId = 22881 } } };
                    Attribute colorAttribute = new Attribute { ComplexId = 0, Id = 10096, Values = new List<Value> { new Value { DictionaryValueId = 61574 } } };
                    Attribute productArticleAttribute = new Attribute { ComplexId = 0, Id = 8292, Values = new List<Value> { new Value { ValueValue = product.article } } };
                    Attribute printTypeAttribute = new Attribute { ComplexId = 0, Id = 9437, Values = new List<Value> { new Value { DictionaryValueId = 970671978 }, new Value { DictionaryValueId = 970671979 } } };
                    Attribute articleTypeAttribute = new Attribute { ComplexId = 0, Id = 9024, Values = new List<Value> { new Value { ValueValue = offer.article } } };
                    Attribute materialDetailsAttribute = new Attribute { ComplexId = 0, Id = 4604, Values = new List<Value> { new Value { ValueValue = offer?.Material } } };
                    Attribute packageTypeAttribute = new Attribute { ComplexId = 0, Id = 4300 };
                    //Attribute materialAttribute = new Attribute { ComplexId = 0, Id = 4496, Values = new List<Value> { new Value { DictionaryValueId = 62174 }, new Value { DictionaryValueId = 62040 } } };

                    var group = product.groups?.First(g => g.name.ToLower() == "hoodie" || g.name.ToLower() == "t-shirt" || g.name.ToLower() == "joggers");

                    if (group != null)
                        switch (group.name.ToLower())
                        {
                            case "hoodie":
                                packageTypeAttribute.Values = new List<Value> { new Value { DictionaryValueId = 44410 } };
                                break;
                            default:
                                packageTypeAttribute.Values = new List<Value> { new Value { DictionaryValueId = 44414 } };
                                break;
                        }

                    Attribute sizeAttribute = new Attribute { ComplexId = 0, Id = 4295 };

                    switch (offer?.Size)
                    {
                        case Size.XS:
                            sizeAttribute.Values = new List<Value> { new Value { DictionaryValueId = 35535 }, new Value { DictionaryValueId = 35545 } };
                            break;
                        case Size.S:
                            sizeAttribute.Values = new List<Value> { new Value { DictionaryValueId = 35545 }, new Value { DictionaryValueId = 35428 } };
                            break;
                        case Size.M:
                            sizeAttribute.Values = new List<Value> { new Value { DictionaryValueId = 35429 }, new Value { DictionaryValueId = 35430 } };
                            break;
                        case Size.L:
                            sizeAttribute.Values = new List<Value> { new Value { DictionaryValueId = 35431 } };
                            break;
                        case Size.XL:
                            sizeAttribute.Values = new List<Value> { new Value { DictionaryValueId = 35432 } };
                            break;
                        default:
                            break;
                    }

                    ozonProductInfo.Attributes = new List<Attribute> {
                        brandAttribute,
                        nameAttribute,
                        descriptionAttribute,
                        typeAttribute,
                        countryAttribute,
                        seasonAttribute,
                        targetAttribute,
                        colorAttribute,
                        productArticleAttribute,
                        printTypeAttribute,
                        materialDetailsAttribute,
                        packageTypeAttribute,
                        sizeAttribute,
                        articleTypeAttribute
                    };

                    ozonProductInfo.CategoryId = categoryId;
                    ozonProductInfo.CurrencyCode = "RUB";

                    ozonProductInfo.WeightUnit = "g";
                    ozonProductInfo.DimensionUnit = "mm";
                    ozonProductInfo.Vat = "0.00";
                    ozonProductInfo.Name = product.name;
                    ozonProductInfo.Price = offer?.price.ToString();
                    ozonProductInfo.OfferId = offer!.article;

                    ozonProductInfo.Weight = offer.weight;
                    ozonProductInfo.Height = offer.height * 10;
                    ozonProductInfo.Depth = offer.length * 10;
                    ozonProductInfo.Width = offer.width * 10;

                    requestData.OzonProductsImportItems.Add(ozonProductInfo);
                }
            }

            var json = JsonConvert.SerializeObject(requestData);
            var buffer = System.Text.Encoding.UTF8.GetBytes(json);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await httpClient.PostAsync(url, byteContent);
            var responseJson = await response.Content.ReadAsStringAsync();




            Console.WriteLine(json);
            Console.WriteLine(responseJson);


            var data = JsonConvert.DeserializeObject<ProductImportResponseData?>(responseJson);
            return data;
        }

        //public void SyncWithContext(ApplicationDbContext context)
        //{
        //  OzonProductsResponseData? response = null;
        //  GetProducts().ContinueWith(r => response = r.Result).Wait();
        //  if (response == null || response.Result == null) return;

        //  //using (var ctx = new ApplicationDbContext())
        //  //{
        //  var products = context.Products?.Include(p => p.OzonProduct).ToList();
        //  if (products == null) return;

        //  response.Result.Products.ForEach(op =>
        //  {
        //    var product = products.FirstOrDefault(p => p.Code == op.OfferId || p.VendorCode == op.OfferId);
        //    if (product != null && product.OzonProduct == null)
        //    {
        //      op.PortalProductId = product.Id;
        //      context.OzonProducts.Add(op);
        //    }
        //  });

        //  context.SaveChanges();
        //  //}
        //}
    }
}

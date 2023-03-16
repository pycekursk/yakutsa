using Newtonsoft.Json;

namespace yakutsa.Services.Ozon.Models
{

  public class OzonProductsResponseData
  {
    [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
    public OzonProductsResponseResult Result { get; set; }
  }



  public class OzonProductsResponseResult
  {
    [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
    public List<OzonProduct> Products { get; set; }

    [JsonProperty("total", NullValueHandling = NullValueHandling.Ignore)]
    public long? Total { get; set; }

    [JsonProperty("last_id", NullValueHandling = NullValueHandling.Ignore)]
    public string LastId { get; set; }
  }
}

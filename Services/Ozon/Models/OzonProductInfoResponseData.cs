using Newtonsoft.Json;

namespace yakutsa.Services.Ozon.Models
{
  public class OzonProductInfoResponseData
  {
    [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
    public OzonProductInfo OzonProductInfo { get; set; }
  }
}

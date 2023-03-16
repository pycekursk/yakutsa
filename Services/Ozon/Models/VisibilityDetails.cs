using Newtonsoft.Json;

namespace yakutsa.Services.Ozon.Models
{
  public class VisibilityDetails
  {
    [JsonProperty("has_price", NullValueHandling = NullValueHandling.Ignore)]
    public bool? HasPrice { get; set; }

    [JsonProperty("has_stock", NullValueHandling = NullValueHandling.Ignore)]
    public bool? HasStock { get; set; }

    [JsonProperty("active_product", NullValueHandling = NullValueHandling.Ignore)]
    public bool? ActiveProduct { get; set; }
  }
}

using Newtonsoft.Json;

namespace yakutsa.Services.Ozon.Models
{
  public class Commission
  {
    [JsonProperty("percent", NullValueHandling = NullValueHandling.Ignore)]
    public long? Percent { get; set; }

    [JsonProperty("min_value", NullValueHandling = NullValueHandling.Ignore)]
    public long? MinValue { get; set; }

    [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
    public long? Value { get; set; }

    [JsonProperty("sale_schema", NullValueHandling = NullValueHandling.Ignore)]
    public string SaleSchema { get; set; }

    [JsonProperty("delivery_amount", NullValueHandling = NullValueHandling.Ignore)]
    public long? DeliveryAmount { get; set; }

    [JsonProperty("return_amount", NullValueHandling = NullValueHandling.Ignore)]
    public long? ReturnAmount { get; set; }
  }
}

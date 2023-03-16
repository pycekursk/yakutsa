using Newtonsoft.Json;

namespace yakutsa.Services.Ozon.Models
{
  public class OzonAttributeValueRequestData
  {
    [JsonProperty("attribute_id", NullValueHandling = NullValueHandling.Ignore)]
    public long? AttributeId { get; set; }

    [JsonProperty("category_id", NullValueHandling = NullValueHandling.Ignore)]
    public long? CategoryId { get; set; }

    [JsonProperty("language", NullValueHandling = NullValueHandling.Ignore)]
    public string Language { get; set; }

    [JsonProperty("last_value_id", NullValueHandling = NullValueHandling.Ignore)]
    public long? LastValueId { get; set; }

    [JsonProperty("limit", NullValueHandling = NullValueHandling.Ignore)]
    public long? Limit { get; set; }
  }
}

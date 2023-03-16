using Newtonsoft.Json;

namespace yakutsa.Services.Ozon.Models
{
  public class Source
  {
    [JsonProperty("is_enabled", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IsEnabled { get; set; }

    [JsonProperty("sku", NullValueHandling = NullValueHandling.Ignore)]
    public long? Sku { get; set; }

    [JsonProperty("source", NullValueHandling = NullValueHandling.Ignore)]
    public string SourceSource { get; set; }
  }
}

using Newtonsoft.Json;

namespace yakutsa.Services.Ozon.Models
{
  public class Stocks
  {
    [JsonProperty("coming", NullValueHandling = NullValueHandling.Ignore)]
    public long? Coming { get; set; }

    [JsonProperty("present", NullValueHandling = NullValueHandling.Ignore)]
    public long? Present { get; set; }

    [JsonProperty("reserved", NullValueHandling = NullValueHandling.Ignore)]
    public long? Reserved { get; set; }
  }
}

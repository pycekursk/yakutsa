using Newtonsoft.Json;

namespace yakutsa.Services.Ozon.Models
{
  public class LimitInfoResponseData
  {
    [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
    public Result Result { get; set; }

    [JsonProperty("total", NullValueHandling = NullValueHandling.Ignore)]
    public Total Total { get; set; }



  }

  public partial class Result
  {
    [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
    public long? Value { get; set; }

    [JsonProperty("remaining", NullValueHandling = NullValueHandling.Ignore)]
    public long? Remaining { get; set; }

    [JsonProperty("reset_at", NullValueHandling = NullValueHandling.Ignore)]
    public string ResetAt { get; set; }
  }

  public class Total
  {
    [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
    public long? Value { get; set; }

    [JsonProperty("remaining", NullValueHandling = NullValueHandling.Ignore)]
    public long? Remaining { get; set; }
  }
}



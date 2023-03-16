using Newtonsoft.Json;

namespace yakutsa.Services.Ozon.Models
{
  public class CategoryRequestData
  {
    [JsonProperty("category_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public long CategoryId { get; set; }

    [JsonProperty("language", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string Language { get; set; } = "DEFAULT";
  }
}

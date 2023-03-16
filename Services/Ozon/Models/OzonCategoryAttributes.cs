using Newtonsoft.Json;

namespace yakutsa.Services.Ozon.Models
{
  public class OzonCategoryAttributes
  {
    [JsonProperty("category_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public long CategoryId { get; set; }

    [JsonProperty("attributes", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public List<OzonCategoryAttribute> Attributes { get; set; }
  }
}



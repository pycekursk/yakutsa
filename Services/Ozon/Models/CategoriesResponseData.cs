using Newtonsoft.Json;

namespace yakutsa.Services.Ozon.Models
{
  public class CategoriesResponseData
  {
    [JsonProperty("result")]
    public List<OzonCategory> Categories { get; set; }
  }
}

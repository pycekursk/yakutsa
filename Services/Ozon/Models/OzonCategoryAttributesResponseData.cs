
using Newtonsoft.Json;

namespace yakutsa.Services.Ozon.Models
{
  public class OzonCategoryAttributesResponseData
  {
    [JsonProperty("result")]
    public List<OzonCategoryAttributes> CategoriesWithAttributes { get; set; }
  }
}


using Newtonsoft.Json;

namespace yakutsa.Services.Ozon.Models
{
  public class OzonProductImportData
  {
    [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
    public List<OzonProductImportItem> OzonProductsImportItems { get; set; } = new List<OzonProductImportItem>();
  }
}

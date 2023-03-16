using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;

namespace yakutsa.Services.Ozon.Models
{
  public class OzonCategory
  {
    [JsonProperty("category_id"), Key]
    public long CategoryId { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("children")]
    public List<OzonCategory> Categories { get; set; }
  }
}

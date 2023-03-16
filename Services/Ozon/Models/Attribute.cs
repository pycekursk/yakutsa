
using Newtonsoft.Json;

namespace yakutsa.Services.Ozon.Models
{
  public class Attribute
  {
    [JsonProperty("complex_id", NullValueHandling = NullValueHandling.Ignore)]
    public long? ComplexId { get; set; }

    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public long? Id { get; set; }

    [JsonProperty("values", NullValueHandling = NullValueHandling.Ignore)]
    public List<Value> Values { get; set; }
  }
}

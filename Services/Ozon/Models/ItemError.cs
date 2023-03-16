using Newtonsoft.Json;

namespace yakutsa.Services.Ozon.Models
{
  public class ItemError
  {
    [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
    public string Code { get; set; }

    [JsonProperty("field", NullValueHandling = NullValueHandling.Ignore)]
    public string Field { get; set; }

    [JsonProperty("attribute_id", NullValueHandling = NullValueHandling.Ignore)]
    public long? AttributeId { get; set; }

    [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
    public string State { get; set; }

    [JsonProperty("level", NullValueHandling = NullValueHandling.Ignore)]
    public string Level { get; set; }

    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }

    [JsonProperty("optional_description_elements", NullValueHandling = NullValueHandling.Ignore)]
    public OptionalDescriptionElements OptionalDescriptionElements { get; set; }

    [JsonProperty("attribute_name", NullValueHandling = NullValueHandling.Ignore)]
    public string AttributeName { get; set; }
  }
}

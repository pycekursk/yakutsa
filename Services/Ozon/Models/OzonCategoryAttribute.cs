using Newtonsoft.Json;

namespace yakutsa.Services.Ozon.Models
{
  public class OzonCategoryAttribute
  {
    [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public long Id { get; set; }

    [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string Name { get; set; }

    [JsonProperty("description", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string Description { get; set; }

    [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string Type { get; set; }

    [JsonProperty("is_collection", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool IsCollection { get; set; }

    [JsonProperty("is_required", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool IsRequired { get; set; }

    [JsonProperty("group_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public long GroupId { get; set; }

    [JsonProperty("group_name", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string GroupName { get; set; }

    [JsonProperty("dictionary_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public long DictionaryId { get; set; }
  }
}



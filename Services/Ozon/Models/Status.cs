using Newtonsoft.Json;

namespace yakutsa.Services.Ozon.Models
{
  public class Status
  {
    [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
    public string State { get; set; }

    [JsonProperty("state_failed", NullValueHandling = NullValueHandling.Ignore)]
    public string StateFailed { get; set; }

    [JsonProperty("moderate_status", NullValueHandling = NullValueHandling.Ignore)]
    public string ModerateStatus { get; set; }

    [JsonProperty("decline_reasons", NullValueHandling = NullValueHandling.Ignore)]
    public List<string> DeclineReasons { get; set; }

    [JsonProperty("validation_state", NullValueHandling = NullValueHandling.Ignore)]
    public string ValidationState { get; set; }

    [JsonProperty("state_name", NullValueHandling = NullValueHandling.Ignore)]
    public string StateName { get; set; }

    [JsonProperty("state_description", NullValueHandling = NullValueHandling.Ignore)]
    public string StateDescription { get; set; }

    [JsonProperty("is_failed", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IsFailed { get; set; }

    [JsonProperty("is_created", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IsCreated { get; set; }

    [JsonProperty("state_tooltip", NullValueHandling = NullValueHandling.Ignore)]
    public string StateTooltip { get; set; }

    [JsonProperty("item_errors", NullValueHandling = NullValueHandling.Ignore)]
    public List<ItemError> ItemErrors { get; set; }

    [JsonProperty("state_updated_at", NullValueHandling = NullValueHandling.Ignore)]
    public string StateUpdatedAt { get; set; }
  }
}

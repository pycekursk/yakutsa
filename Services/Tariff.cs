using Newtonsoft.Json;

namespace yakutsa.Services.Sdek.Models
{
  public partial class Tariff
  {
    [JsonProperty("tariff_code", NullValueHandling = NullValueHandling.Ignore)]
    public long? TariffCodeTariffCode { get; set; }

    [JsonProperty("tariff_name", NullValueHandling = NullValueHandling.Ignore)]
    public string TariffName { get; set; }

    [JsonProperty("tariff_description")]
    public string TariffDescription { get; set; }

    [JsonProperty("delivery_mode", NullValueHandling = NullValueHandling.Ignore)]
    public long? DeliveryMode { get; set; }

    [JsonProperty("delivery_sum", NullValueHandling = NullValueHandling.Ignore)]
    public long? DeliverySum { get; set; }

    [JsonProperty("period_min", NullValueHandling = NullValueHandling.Ignore)]
    public long? PeriodMin { get; set; }

    [JsonProperty("period_max", NullValueHandling = NullValueHandling.Ignore)]
    public long? PeriodMax { get; set; }

    [JsonProperty("calendar_min", NullValueHandling = NullValueHandling.Ignore)]
    public long? CalendarMin { get; set; }

    [JsonProperty("calendar_max", NullValueHandling = NullValueHandling.Ignore)]
    public long? CalendarMax { get; set; }
  }
}

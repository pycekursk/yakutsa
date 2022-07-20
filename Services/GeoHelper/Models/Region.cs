using Newtonsoft.Json;

using yakutsa.Services.GeoHelper.Models.Base;

namespace yakutsa.Services.GeoHelper.Models
{
    public class Region : BaseObject
    {
        public int timezoneOffset { get; set; }
        public string? countryIso { get; set; }
        public string? timezone { get; set; }
        [JsonProperty("countryId")]
        public override int parentId { get; set; }
    }
}




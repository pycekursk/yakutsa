using Newtonsoft.Json;

using yakutsa.Services.GeoHelper.Models.Base;

namespace yakutsa.Services.GeoHelper.Models
{
    public class Street : BaseObject
    {
        public string? postCode { get; set; }

        [JsonProperty("cityId")]
        public override int parentId { get; set; }
    }
}

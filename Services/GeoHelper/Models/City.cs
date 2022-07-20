using Newtonsoft.Json;

using yakutsa.Services.GeoHelper.Models.Base;

namespace yakutsa.Services.GeoHelper.Models
{
    public class City : BaseObject
    {
        public int population { get; set; }

        [JsonProperty("regionId")]
        public override int parentId { get; set; }
    }

}

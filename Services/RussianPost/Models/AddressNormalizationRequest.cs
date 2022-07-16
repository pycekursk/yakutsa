using Newtonsoft.Json;

namespace yakutsa.Services.RussianPost.Models
{
    public class AddressNormalizationRequest
    {
        public string id { get; set; }

        [JsonProperty("original-address")]
        public string originaladdress { get; set; }
    }
}
using Newtonsoft.Json;

namespace yakutsa.Services.RussianPost.Models
{
    public class AddressNormalizationResponse
    {
        [JsonProperty("address-type")]
        public string addresstype { get; set; }
        [JsonProperty("address-guid")]
        public string addressguid { get; set; }
        public string area { get; set; }
        public string building { get; set; }
        public string corpus { get; set; }
        public string hotel { get; set; }
        public string house { get; set; }
        public string id { get; set; }
        public string index { get; set; }
        public string letter { get; set; }
        public string location { get; set; }
        [JsonProperty("num-address-type")]
        public string numaddresstype { get; set; }
        [JsonProperty("original-address")]
        public string originaladdress { get; set; }
        public string place { get; set; }
        [JsonProperty("quality-code")]
        public string qualitycode { get; set; }
        public string region { get; set; }
        [JsonProperty("region-guid")]
        public string regionGuid { get; set; }
        public string room { get; set; }
        public string slash { get; set; }
        public string street { get; set; }
        [JsonProperty("validation-code")]
        public string validationcode { get; set; }
    }

}


using Newtonsoft.Json;

namespace yakutsa.Services.RussianPost.Models
{
    public class DeliveryCostRequest
    {
        [JsonProperty("completeness-checking")]
        public bool completenesschecking { get; set; }
        public bool courier { get; set; }
        [JsonProperty("declared-value")]
        public int declaredvalue { get; set; }
        public Dimension dimension { get; set; }
        [JsonProperty("entries-type")]
        public string entriestype { get; set; }
        public bool fragile { get; set; }
        [JsonProperty("index-from")]
        public string indexfrom { get; set; }
        [JsonProperty("index-to")]
        public string indexto { get; set; }
        [JsonProperty("mail-category")]
        public string mailcategory { get; set; }
        [JsonProperty("mail-direct")]
        public int maildirect { get; set; }
        [JsonProperty("mail-type")]
        public string mailtype { get; set; }
        public int mass { get; set; }
        [JsonProperty("notice-payment-method")]
        public string noticepaymentmethod { get; set; }
        [JsonProperty("payment-method")]
        public string paymentmethod { get; set; }
        [JsonProperty("sms-notice-recipient")]
        public int smsnoticerecipient { get; set; }
        [JsonProperty("transport-type")]
        public string transporttype { get; set; }
        [JsonProperty("with-order-of-notice")]
        public bool withorderofnotice { get; set; }
        [JsonProperty("with-simple-notice")]
        public bool withsimplenotice { get; set; }
    }

    
}

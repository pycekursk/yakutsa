using Newtonsoft.Json;

namespace yakutsa.Services.RussianPost.Models
{
    public class DeliveryCostResponse
    {
        [JsonProperty(PropertyName = "avia-rate")]
        public AviaRate aviarate { get; set; }

        [JsonProperty(PropertyName = "completeness-checking-rate")]
        public CompletenessCheckingRate completenesscheckingrate { get; set; }

        [JsonProperty(PropertyName = "delivery-time")]
        public DeliveryTime deliverytime { get; set; }

        [JsonProperty(PropertyName = "fragile-rate")]
        public FragileRate fragilerate { get; set; }

        [JsonProperty(PropertyName = "ground-rate")]
        public GroundRate groundrate { get; set; }

        [JsonProperty(PropertyName = "insurance-rate")]
        public InsuranceRate insurancerate { get; set; }

        [JsonProperty(PropertyName = "notice-payment-method")]
        public string noticepaymentmethod { get; set; }

        [JsonProperty(PropertyName = "notice-rate")]
        public NoticeRate noticerate { get; set; }

        [JsonProperty(PropertyName = "oversize-rate")]
        public OversizeRate oversizerate { get; set; }

        [JsonProperty(PropertyName = "payment-method")]
        public string paymentmethod { get; set; }

        [JsonProperty(PropertyName = "sms-notice-recipient-rate")]
        public SmsNoticeRecipientRate smsnoticerecipientrate { get; set; }

        [JsonProperty(PropertyName = "total-rate")]
        public int totalrate { get; set; }

        [JsonProperty(PropertyName = "total-vat")]
        public int totalvat { get; set; }
    }
}
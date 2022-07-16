namespace yakutsa.Services.RussianPost.Models
{
    public class DeliveryTime
    {
        [Newtonsoft.Json.JsonProperty("max-days")]
        public int maxdays { get; set; }

        [Newtonsoft.Json.JsonProperty("min-days")]
        public int mindays { get; set; }
    }


}

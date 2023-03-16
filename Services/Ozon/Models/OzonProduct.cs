using yakutsa.Models;

using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace yakutsa.Services.Ozon.Models
{
    public class OzonProduct
    {
        [JsonProperty("product_id", NullValueHandling = NullValueHandling.Ignore), Key]
        public long? ProductId { get; set; }

        [JsonProperty("offer_id", NullValueHandling = NullValueHandling.Ignore)]
        public string OfferId { get; set; }

        [JsonProperty("is_fbo_visible", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsFboVisible { get; set; }

        [JsonProperty("is_fbs_visible", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsFbsVisible { get; set; }

        [JsonProperty("archived", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Archived { get; set; }

        [JsonProperty("is_discounted", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsDiscounted { get; set; }


        [ForeignKey("Product")]
        public int PortalProductId { get; set; }


        //public Product Product { get; set; }
    }
}

using Newtonsoft.Json;

namespace yakutsa.Services.Ozon.Models
{
  public class OzonProductInfo
  {
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public long? Id { get; set; }

    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }

    [JsonProperty("offer_id", NullValueHandling = NullValueHandling.Ignore)]
    public string OfferId { get; set; }

    [JsonProperty("barcode", NullValueHandling = NullValueHandling.Ignore)]
    public string Barcode { get; set; }

    [JsonProperty("buybox_price", NullValueHandling = NullValueHandling.Ignore)]
    public string BuyboxPrice { get; set; }

    [JsonProperty("category_id", NullValueHandling = NullValueHandling.Ignore)]
    public long? CategoryId { get; set; }

    [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
    public string CreatedAt { get; set; }

    [JsonProperty("images", NullValueHandling = NullValueHandling.Ignore)]
    public List<object> Images { get; set; }

    [JsonProperty("marketing_price", NullValueHandling = NullValueHandling.Ignore)]
    public string MarketingPrice { get; set; }

    [JsonProperty("min_ozon_price", NullValueHandling = NullValueHandling.Ignore)]
    public string MinOzonPrice { get; set; }

    [JsonProperty("old_price", NullValueHandling = NullValueHandling.Ignore)]
    public string OldPrice { get; set; }

    [JsonProperty("premium_price", NullValueHandling = NullValueHandling.Ignore)]
    public string PremiumPrice { get; set; }

    [JsonProperty("price", NullValueHandling = NullValueHandling.Ignore)]
    public string Price { get; set; }

    [JsonProperty("recommended_price", NullValueHandling = NullValueHandling.Ignore)]
    public string RecommendedPrice { get; set; }

    [JsonProperty("min_price", NullValueHandling = NullValueHandling.Ignore)]
    public string MinPrice { get; set; }

    [JsonProperty("sources", NullValueHandling = NullValueHandling.Ignore)]
    public List<Source> Sources { get; set; }

    [JsonProperty("stocks", NullValueHandling = NullValueHandling.Ignore)]
    public Stocks Stocks { get; set; }

    [JsonProperty("errors", NullValueHandling = NullValueHandling.Ignore)]
    public List<object> Errors { get; set; }

    [JsonProperty("vat", NullValueHandling = NullValueHandling.Ignore)]
    public string Vat { get; set; }

    [JsonProperty("visible", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Visible { get; set; }

    [JsonProperty("visibility_details", NullValueHandling = NullValueHandling.Ignore)]
    public VisibilityDetails VisibilityDetails { get; set; }

    [JsonProperty("price_index", NullValueHandling = NullValueHandling.Ignore)]
    public string PriceIndex { get; set; }

    [JsonProperty("commissions", NullValueHandling = NullValueHandling.Ignore)]
    public List<Commission> Commissions { get; set; }

    [JsonProperty("volume_weight", NullValueHandling = NullValueHandling.Ignore)]
    public long? VolumeWeight { get; set; }

    [JsonProperty("is_prepayment", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IsPrepayment { get; set; }

    [JsonProperty("is_prepayment_allowed", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IsPrepaymentAllowed { get; set; }

    [JsonProperty("images360", NullValueHandling = NullValueHandling.Ignore)]
    public List<object> Images360 { get; set; }

    [JsonProperty("color_image", NullValueHandling = NullValueHandling.Ignore)]
    public string ColorImage { get; set; }

    [JsonProperty("primary_image", NullValueHandling = NullValueHandling.Ignore)]
    public Uri PrimaryImage { get; set; }

    [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
    public Status Status { get; set; }

    [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
    public string State { get; set; }

    [JsonProperty("service_type", NullValueHandling = NullValueHandling.Ignore)]
    public string ServiceType { get; set; }

    [JsonProperty("fbo_sku", NullValueHandling = NullValueHandling.Ignore)]
    public long? FboSku { get; set; }

    [JsonProperty("fbs_sku", NullValueHandling = NullValueHandling.Ignore)]
    public long? FbsSku { get; set; }

    [JsonProperty("currency_code", NullValueHandling = NullValueHandling.Ignore)]
    public string CurrencyCode { get; set; }

    [JsonProperty("is_kgt", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IsKgt { get; set; }

    [JsonProperty("discounted_stocks", NullValueHandling = NullValueHandling.Ignore)]
    public Stocks DiscountedStocks { get; set; }

    [JsonProperty("is_discounted", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IsDiscounted { get; set; }

    [JsonProperty("has_discounted_item", NullValueHandling = NullValueHandling.Ignore)]
    public bool? HasDiscountedItem { get; set; }

    [JsonProperty("barcodes", NullValueHandling = NullValueHandling.Ignore)]
    public List<object> Barcodes { get; set; }

    [JsonProperty("updated_at", NullValueHandling = NullValueHandling.Ignore)]
    public string UpdatedAt { get; set; }
  }
}

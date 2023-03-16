
using Newtonsoft.Json;

namespace yakutsa.Services.Ozon.Models
{
  public class OzonProductImportItem
  {
    [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
    public List<Attribute> Attributes { get; set; }

    [JsonProperty("barcode", NullValueHandling = NullValueHandling.Ignore)]
    public string Barcode { get; set; }

    [JsonProperty("category_id", NullValueHandling = NullValueHandling.Ignore)]
    public long? CategoryId { get; set; }

    [JsonProperty("color_image", NullValueHandling = NullValueHandling.Ignore)]
    public string ColorImage { get; set; }

    [JsonProperty("complex_attributes", NullValueHandling = NullValueHandling.Ignore)]
    public List<ComplexAttribute> ComplexAttributes { get; set; }

    [JsonProperty("currency_code", NullValueHandling = NullValueHandling.Ignore)]
    public string CurrencyCode { get; set; }

    [JsonProperty("depth", NullValueHandling = NullValueHandling.Ignore)]
    public long? Depth { get; set; }

    [JsonProperty("dimension_unit", NullValueHandling = NullValueHandling.Ignore)]
    public string DimensionUnit { get; set; }

    [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
    public long? Height { get; set; }

    [JsonProperty("images", NullValueHandling = NullValueHandling.Ignore)]
    public List<string> Images { get; set; }

    [JsonProperty("images360", NullValueHandling = NullValueHandling.Ignore)]
    public List<string> Images360 { get; set; }

    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }

    [JsonProperty("offer_id", NullValueHandling = NullValueHandling.Ignore)]
    public string OfferId { get; set; }

    [JsonProperty("old_price", NullValueHandling = NullValueHandling.Ignore)]
    public string? OldPrice { get; set; }

    [JsonProperty("pdf_list", NullValueHandling = NullValueHandling.Ignore)]
    public List<object> PdfList { get; set; }

    [JsonProperty("premium_price", NullValueHandling = NullValueHandling.Ignore)]
    public string? PremiumPrice { get; set; }

    [JsonProperty("price", NullValueHandling = NullValueHandling.Ignore)]
    public string? Price { get; set; }

    [JsonProperty("primary_image", NullValueHandling = NullValueHandling.Ignore)]
    public string PrimaryImage { get; set; }

    [JsonProperty("vat", NullValueHandling = NullValueHandling.Ignore)]
    public string Vat { get; set; }

    [JsonProperty("weight", NullValueHandling = NullValueHandling.Ignore)]
    public long? Weight { get; set; }

    [JsonProperty("weight_unit", NullValueHandling = NullValueHandling.Ignore)]
    public string WeightUnit { get; set; }

    [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
    public long? Width { get; set; }
  }


  public class ComplexAttribute
  {
    [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
    public List<Attribute> Attributes { get; set; }
  }
}

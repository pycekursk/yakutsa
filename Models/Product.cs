using System.Text.Json.Serialization;

using yakutsa.Services;

using static yakutsa.Services.RetailCRM;

namespace yakutsa.Models
{
  public class Product
  {
    public int minPrice { get; set; }
    public int maxPrice { get; set; }
    public int id { get; set; }
    public string article { get; set; }
    public string name { get; set; }
    public string imageUrl { get; set; }

    [JsonIgnore]
    public string? resizedImageHex { get; set; }

    public Group[] groups { get; set; }
    public Offer[] offers { get; set; }
    public string updatedAt { get; set; }
    public bool active { get; set; }
    public int quantity { get; set; }
    public bool markable { get; set; }
    [JsonIgnore]
    public string? modelPath { get; set; }
  }
}





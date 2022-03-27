using RetailCRMCore.Models;

using System.Text.Json.Serialization;

using yakutsa.Services;

namespace yakutsa.Models
{
  public class CartProduct
  {
    public Product? Product { get; set; }
    public Offer? Offer { get; set; }
    public int Count { get; set; }
    public Variant Variant { get; set; }
    public double Price { get { double price = 0; price = Math.Round((double)(Offer!.price * Count), 2); return price; } }
  }

  public enum Variant
  {
    Normal = 1,
    Oversize = 2,
    NA = 0
  }
}





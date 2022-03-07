using System.Text.Json.Serialization;

using yakutsa.Services;

namespace yakutsa.Models
{
  public class CartProduct
  {
    public Product? Product { get; set; }
    public Offer? Offer { get; set; }
    public int Count { get; set; }

    public decimal Price { get { decimal price = 0; price = Math.Round((decimal)(Offer!.price * Count), 2); return price; } }
  }
}





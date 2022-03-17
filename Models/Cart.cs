using RetailCRMCore.Models;

using System.Text.Json.Serialization;

namespace yakutsa.Models
{
  public class Cart
  {
    [JsonIgnore]
    public HttpContext _httpContext;

    public Cart(HttpContext httpContext)
    {
      _httpContext = httpContext;
    }

    public double Price
    {
      get
      {
        double result = 0;
        CartProducts.ForEach(x => result += x.Price);
        return result;
      }
    }

    public List<CartProduct> CartProducts { get; set; } = new List<CartProduct>();

    public int Count { get { int count = 0; CartProducts.ForEach(cp => count += cp.Count); return count; } }



    public List<CartProduct> Add(Product product, Offer offer, int count)
    {
      var cartProduct = CartProducts.FirstOrDefault(cp => cp.Offer.id == offer.id);

      if (cartProduct != null) cartProduct.Count += count;
      else CartProducts.Add(new CartProduct { Product = product, Offer = offer, Count = count });

      _httpContext.Session.SetString("cart", System.Text.Json.JsonSerializer.Serialize(this));
      return CartProducts;
    }

    public List<CartProduct> ChangeCount(int productId, int offerId, int count = 0)
    {
      if (count == 0) CartProducts.Remove(CartProducts.FirstOrDefault(o => o.Product.id == productId && o.Offer.id == offerId)!);
      else
      {
        var cardProduct = CartProducts.FirstOrDefault(cp => cp.Product.id == productId && cp.Offer.id == offerId);
        cardProduct!.Count = count;
      }
      _httpContext.Session.SetString("cart", System.Text.Json.JsonSerializer.Serialize(this));
      return CartProducts;
    }
  }
}
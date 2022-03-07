using yakutsa.Services;

using static yakutsa.Services.RetailCRM;

namespace yakutsa.Models
{
  public class Offer
  {
    public string name { get; set; }
    public int price { get; set; }
    public string[] images { get; set; }
    public int id { get; set; }
    public string article { get; set; }
    public Price[] prices { get; set; }
    public string vatRate { get; set; }
    public int quantity { get; set; }
    public bool active { get; set; }
    public Unit unit { get; set; }
  }

}





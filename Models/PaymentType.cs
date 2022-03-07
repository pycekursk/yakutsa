public class PaymentType
{
  public string name { get; set; }
  public string code { get; set; }
  public bool active { get; set; }
  public bool defaultForCrm { get; set; }
  public bool defaultForApi { get; set; }
  public string[] deliveryTypes { get; set; }
  public string[] paymentStatuses { get; set; }
}

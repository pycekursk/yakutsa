namespace yakutsa.Models
{
  public class DeliveryType
  {
    public bool isDynamicCostCalculation { get; set; }
    public bool isAutoCostCalculation { get; set; }
    public bool isAutoNetCostCalculation { get; set; }
    public bool isCostDependsOnRegionAndWeightAndSum { get; set; }
    public bool isCostDependsOnDateTime { get; set; }
    public string name { get; set; }
    public string code { get; set; }
    public bool active { get; set; }
    public int defaultCost { get; set; }
    public int defaultNetCost { get; set; }
    public string[] paymentTypes { get; set; }
    public string integrationCode { get; set; }
    public object[] deliveryServices { get; set; }
    public bool defaultForCrm { get; set; }

  }
}
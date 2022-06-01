using System.Xml.Serialization;

namespace yakutsa.Services.Dalli.Models
{
  [XmlRoot(ElementName = "services")]
  public class Services
  {

    [XmlElement(ElementName = "service")]
    public List<Service> Service { get; set; }

    [XmlAttribute(AttributeName = "count")]
    public int Count { get; set; }

    [XmlText]
    public string Text { get; set; }
  }

}

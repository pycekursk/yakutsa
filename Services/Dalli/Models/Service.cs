using System.Xml.Serialization;

namespace yakutsa.Services.Dalli.Models
{
  [XmlRoot(ElementName = "service")]
  public class Service
  {

    [XmlElement(ElementName = "code")]
    public int Code { get; set; }

    [XmlElement(ElementName = "name")]
    public string Name { get; set; }
  }

}

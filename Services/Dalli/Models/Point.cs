using RetailCRMCore.Models.Base;

using System.Xml.Serialization;

namespace yakutsa.Services.Dalli.Models
{
  [XmlRoot(ElementName = "point")]
  public class Point : IPVZPoint
  {
    [XmlElement(ElementName = "address")]
    public string Address { get; set; }

    [XmlElement(ElementName = "town")]
    public string Town { get; set; }

    [XmlElement(ElementName = "phone")]
    public string Phone { get; set; }

    [XmlElement(ElementName = "worktime")]
    public string Worktime { get; set; }

    [XmlElement(ElementName = "name")]
    public string Name { get; set; }

    [XmlElement(ElementName = "description")]
    public string Description { get; set; }

    [XmlAttribute(AttributeName = "code")]
    public string Code { get; set; }

    [XmlText]
    public string Text { get; set; }
  }
}

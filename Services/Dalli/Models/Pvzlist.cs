using System.Xml.Serialization;

namespace yakutsa.Services.Dalli.Models
{
  [XmlRoot(ElementName = "pvzlist")]
  public class Pvzlist
  {
    [XmlElement(ElementName = "point")]
    public List<Point> Point { get; set; }

    [XmlAttribute(AttributeName = "partner")]
    public string Partner { get; set; }

    [XmlAttribute(AttributeName = "count")]
    public int Count { get; set; }
  }
}

using System.Xml.Serialization;

using yakutsa.Extensions;

namespace yakutsa.Services.Dalli.Models
{
    [XmlRoot(ElementName = "deliverycost")]
    public class DeliveryCost
    {
        [XmlElement(ElementName = "price")]
        public List<PartnerCalculationResult>? Price { get; set; }

        [XmlAttribute(AttributeName = "partner")]
        public string? Partner { get; set; }

        [XmlAttribute(AttributeName = "townto")]
        public string? Townto { get; set; }

        [XmlAttribute(AttributeName = "zone")]
        public string? Zone { get; set; }

        public string? Code { get; set; }

        public string FriendlyName { get => Partner.GetDisplayName(); }
    }
}

using Newtonsoft.Json.Converters;

using System.ComponentModel.DataAnnotations;

using System.Xml.Serialization;

using yakutsa.Extensions;
using yakutsa.Services.Dalli.Enums;

namespace yakutsa.Services.Dalli.Models
{
    [XmlRoot(ElementName = "price")]
    public class PartnerCalculationResult
    {
        [XmlAttribute(AttributeName = "service")]
        public int Service { get; set; }

        [Display(Name = "Цена")]
        [XmlAttribute(AttributeName = "price")]
        public int Price { get; set; }

        [Display(Name = "Период доставки")]
        [XmlAttribute(AttributeName = "delivery_period")]
        public int DeliveryPeriod { get; set; }

        [XmlAttribute(AttributeName = "msg")]
        public string Msg { get; set; }

        [XmlAttribute(AttributeName = "zone")]
        public string Zone { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public int Type { get; set; }

        [Display(Name = "Срок (без учета дня забора)")]
        public string InfoText { get => $"{DeliveryPeriod} раб. дн. ({Msg})"; }

        public DeliveryServices ServiceType { get => (DeliveryServices)Service; }

        public RupostTypes RupostType { get => (RupostTypes)Type; }

        public string FriendlyName { get => ServiceType.GetDisplayName(); }

        public string RupostFriendlyName { get => RupostType.GetDisplayName(); }
    }
}

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
        public string InfoText { get => DeliveryPeriod == 0 ? String.Empty : $"{DeliveryPeriod + 1} раб. дн."; }

        public string LabelText { get => labelText ??= String.Empty; set => labelText = value; }

        public DeliveryServices ServiceType { get => (DeliveryServices)Service; }

        public RupostTypes RupostType { get => (RupostTypes)Type; }

        string friendlyName;
        private string labelText;

        public string FriendlyName { get => friendlyName ??= ServiceType.GetDisplayName(); set => friendlyName = value; }

        public string RupostFriendlyName { get => RupostType.GetDisplayName(); }

        public List<Dalli.Models.Point>? PVZList { get; set; }
    }
}

using yakutsa.Data;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Identity;

//using Newtonsoft.Json;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using Newtonsoft.Json;

namespace yakutsa.Models
{
  public class AppUser : IdentityUser
  {
    private string? address;
    private AddressObject? addressObject;

    [DisplayName("Имя")]
    public string? Name { get; set; }

    [DisplayName("Адрес")]
    public string? Address { get => address; set => address = SetAddress(value); }

    public AddressObject? AddressObject
    {
      get
      {
        if (addressObject == null && !string.IsNullOrEmpty(address))
          AddressObject.ParseJsonAsync(address).ContinueWith(t => addressObject = t.Result).Wait();
        return addressObject;
      }
      set => addressObject = value;
    }

    [DisplayName("Роль")]
    public IdentityRole? Role { get; set; }

    //[DisplayName("Заказы")]
    //public List<Order>? Orders { get; set; }

    [DisplayName("Зарегистрирован")]
    public DateTime Registered { get; set; }

    [DisplayName("Последний визит")]
    public DateTime LastVisit { get; set; }

    //[DisplayName("Тип цены")]
    //public PriceModel PriceModel { get; set; }

    private string SetAddress(string address)
    {
      if (string.IsNullOrEmpty(address)) return default;
      AddressObject.ParseJsonAsync(address).ContinueWith(t => AddressObject = t.Result).Wait();
      return address;
    }
  }

  public class AddressObject : BaseModel
  {
    private string? region;

    [NotMapped]
    public HtmlString? AddressHtmlString { get => new HtmlString(Name); }

    [JsonProperty("locality")]
    public string? City { get; set; }

    [JsonProperty("province")]
    public string? Region
    {
      get => region;
      set
      {
        if (!string.IsNullOrEmpty(value) && (value.ToLower().Contains("область") || value.ToLower().Contains("обл.")))
          region = value;
      }
    }

    [JsonProperty("house")]
    public string? House { get; set; }

    [JsonProperty("street")]
    public string? Street { get; set; }

    [JsonProperty("entrance")]
    public string? Entrance { get; set; }

    [JsonProperty("flat")]
    public string? Flat { get; set; }

    [JsonProperty("floor")]
    public string? Floor { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }

    public static async Task<AddressObject> ParseAsync(string input)
    {
      WebRequest request = WebRequest.Create($"https://geocode-maps.yandex.ru/1.x/?apikey=76c45ff6-6d94-4c71-9cca-10f9ff24ad30&geocode={input}");
      request.ContentType = "application/json";
      var result = string.Empty;
      WebResponse response = await request.GetResponseAsync();
      AddressObject address = new();
      using (Stream stream = response.GetResponseStream())
      {
        using (StreamReader reader = new StreamReader(stream))
        {
          try
          {
            var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(ymaps));
            var geoResult = (ymaps)xmlSerializer.Deserialize(reader);
            var city = geoResult?.GeoObjectCollection.featureMember.GeoObject.metaDataProperty.GeocoderMetaData.AddressDetails.Country.AdministrativeArea.SubAdministrativeArea.Locality.LocalityName;
            var region = geoResult?.GeoObjectCollection.featureMember.GeoObject.metaDataProperty.GeocoderMetaData.AddressDetails.Country.AdministrativeArea.AdministrativeAreaName;

            address.City = city;
            address.Region = region;
          }
          catch (Exception)
          {

          }

        }
      }
      return address;
    }


    public static async Task<AddressObject> ParseJsonAsync(string input)
    {
      WebRequest request = WebRequest.Create($"https://geocode-maps.yandex.ru/1.x/?apikey=76c45ff6-6d94-4c71-9cca-10f9ff24ad30&format=json&geocode={input}");
      request.ContentType = "application/json";
      var result = string.Empty;
      WebResponse response = await request.GetResponseAsync();
      AddressObject address = new();
      using (Stream stream = response.GetResponseStream())
      {
        using (StreamReader reader = new StreamReader(stream))
        {
          result = await reader.ReadToEndAsync();
        }
      }

      var geoobject = (Newtonsoft.Json.JsonConvert.DeserializeObject(result) as dynamic)?.response.GeoObjectCollection.featureMember[0].GeoObject;
      var obj = geoobject?.metaDataProperty.GeocoderMetaData.Address.Components;
      address.Name = input;
      if (obj?.Count != 0)
      {
        var point = geoobject["Point"]["pos"].ToString().Split(" ");

        address.Latitude = point[1];
        address.Longitude = point[0];

        foreach (var prop in obj)
        {
          switch (prop.kind.ToString())
          {
            case "locality":
              address.City = prop.name;
              break;
            case "province":
              address.Region = prop.name;
              break;
            case "street":
              address.Street = prop.name;
              break;
            case "house":
              address.House = prop.name;
              break;
            case "entrance":
              address.Entrance = prop.name;
              break;
            case "other":
              if (prop.name.ToString().Contains("этаж"))
                address.Floor = prop.name;
              else if (prop.name.ToString().Contains("кв"))
                address.Flat = prop.name;
              break;
            default: break;
          }
        }
      }

      return address;
    }
  }

  #region GeocoderSupport
  // Примечание. Для запуска созданного кода может потребоваться NET Framework версии 4.5 или более поздней версии и .NET Core или Standard версии 2.0 или более поздней.
  /// <remarks/>
  [System.SerializableAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://maps.yandex.ru/ymaps/1.x")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://maps.yandex.ru/ymaps/1.x", IsNullable = false)]
  public partial class ymaps
  {

    private ymapsGeoObjectCollection geoObjectCollectionField;

    /// <remarks/>
    public ymapsGeoObjectCollection GeoObjectCollection
    {
      get
      {
        return this.geoObjectCollectionField;
      }
      set
      {
        this.geoObjectCollectionField = value;
      }
    }
  }

  /// <remarks/>
  [System.SerializableAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://maps.yandex.ru/ymaps/1.x")]
  public partial class ymapsGeoObjectCollection
  {

    private metaDataProperty metaDataPropertyField;

    private featureMember featureMemberField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.opengis.net/gml")]
    public metaDataProperty metaDataProperty
    {
      get
      {
        return this.metaDataPropertyField;
      }
      set
      {
        this.metaDataPropertyField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.opengis.net/gml")]
    public featureMember featureMember
    {
      get
      {
        return this.featureMemberField;
      }
      set
      {
        this.featureMemberField = value;
      }
    }
  }

  /// <remarks/>
  [System.SerializableAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.opengis.net/gml")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.opengis.net/gml", IsNullable = false)]
  public partial class metaDataProperty
  {

    private GeocoderMetaData geocoderMetaDataField;

    private GeocoderResponseMetaData geocoderResponseMetaDataField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://maps.yandex.ru/geocoder/1.x")]
    public GeocoderMetaData GeocoderMetaData
    {
      get
      {
        return this.geocoderMetaDataField;
      }
      set
      {
        this.geocoderMetaDataField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://maps.yandex.ru/geocoder/1.x")]
    public GeocoderResponseMetaData GeocoderResponseMetaData
    {
      get
      {
        return this.geocoderResponseMetaDataField;
      }
      set
      {
        this.geocoderResponseMetaDataField = value;
      }
    }
  }

  /// <remarks/>
  [System.SerializableAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://maps.yandex.ru/geocoder/1.x")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://maps.yandex.ru/geocoder/1.x", IsNullable = false)]
  public partial class GeocoderMetaData
  {

    private string kindField;

    private string textField;

    private string precisionField;

    private XmlAddress addressField;

    private AddressDetails addressDetailsField;

    /// <remarks/>
    public string kind
    {
      get
      {
        return this.kindField;
      }
      set
      {
        this.kindField = value;
      }
    }

    /// <remarks/>
    public string text
    {
      get
      {
        return this.textField;
      }
      set
      {
        this.textField = value;
      }
    }

    /// <remarks/>
    public string precision
    {
      get
      {
        return this.precisionField;
      }
      set
      {
        this.precisionField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://maps.yandex.ru/address/1.x", ElementName = "Address")]
    public XmlAddress Address
    {
      get
      {
        return this.addressField;
      }
      set
      {
        this.addressField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "urn:oasis:names:tc:ciq:xsdschema:xAL:2.0")]
    public AddressDetails AddressDetails
    {
      get
      {
        return this.addressDetailsField;
      }
      set
      {
        this.addressDetailsField = value;
      }
    }
  }

  /// <remarks/>
  [System.SerializableAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://maps.yandex.ru/address/1.x")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://maps.yandex.ru/address/1.x", IsNullable = false)]
  public partial class XmlAddress
  {

    private string country_codeField;

    private string formattedField;

    private AddressComponent[] componentField;

    /// <remarks/>
    public string country_code
    {
      get
      {
        return this.country_codeField;
      }
      set
      {
        this.country_codeField = value;
      }
    }

    /// <remarks/>
    public string formatted
    {
      get
      {
        return this.formattedField;
      }
      set
      {
        this.formattedField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Component")]
    public AddressComponent[] Component
    {
      get
      {
        return this.componentField;
      }
      set
      {
        this.componentField = value;
      }
    }
  }

  /// <remarks/>
  [System.SerializableAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://maps.yandex.ru/address/1.x")]
  public partial class AddressComponent
  {

    private string kindField;

    private string nameField;

    /// <remarks/>
    public string kind
    {
      get
      {
        return this.kindField;
      }
      set
      {
        this.kindField = value;
      }
    }

    /// <remarks/>
    public string name
    {
      get
      {
        return this.nameField;
      }
      set
      {
        this.nameField = value;
      }
    }
  }

  /// <remarks/>
  [System.SerializableAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:ciq:xsdschema:xAL:2.0")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:oasis:names:tc:ciq:xsdschema:xAL:2.0", IsNullable = false)]
  public partial class AddressDetails
  {

    private AddressDetailsCountry countryField;

    /// <remarks/>
    public AddressDetailsCountry Country
    {
      get
      {
        return this.countryField;
      }
      set
      {
        this.countryField = value;
      }
    }
  }

  /// <remarks/>
  [System.SerializableAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:ciq:xsdschema:xAL:2.0")]
  public partial class AddressDetailsCountry
  {

    private string addressLineField;

    private string countryNameCodeField;

    private string countryNameField;

    private AddressDetailsCountryAdministrativeArea administrativeAreaField;

    /// <remarks/>
    public string AddressLine
    {
      get
      {
        return this.addressLineField;
      }
      set
      {
        this.addressLineField = value;
      }
    }

    /// <remarks/>
    public string CountryNameCode
    {
      get
      {
        return this.countryNameCodeField;
      }
      set
      {
        this.countryNameCodeField = value;
      }
    }

    /// <remarks/>
    public string CountryName
    {
      get
      {
        return this.countryNameField;
      }
      set
      {
        this.countryNameField = value;
      }
    }

    /// <remarks/>
    public AddressDetailsCountryAdministrativeArea AdministrativeArea
    {
      get
      {
        return this.administrativeAreaField;
      }
      set
      {
        this.administrativeAreaField = value;
      }
    }
  }

  /// <remarks/>
  [System.SerializableAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:ciq:xsdschema:xAL:2.0")]
  public partial class AddressDetailsCountryAdministrativeArea
  {

    private string administrativeAreaNameField;

    private AddressDetailsCountryAdministrativeAreaSubAdministrativeArea subAdministrativeAreaField;

    /// <remarks/>
    public string AdministrativeAreaName
    {
      get
      {
        return this.administrativeAreaNameField;
      }
      set
      {
        this.administrativeAreaNameField = value;
      }
    }

    /// <remarks/>
    public AddressDetailsCountryAdministrativeAreaSubAdministrativeArea SubAdministrativeArea
    {
      get
      {
        return this.subAdministrativeAreaField;
      }
      set
      {
        this.subAdministrativeAreaField = value;
      }
    }
  }

  /// <remarks/>
  [System.SerializableAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:ciq:xsdschema:xAL:2.0")]
  public partial class AddressDetailsCountryAdministrativeAreaSubAdministrativeArea
  {

    private string subAdministrativeAreaNameField;

    private AddressDetailsCountryAdministrativeAreaSubAdministrativeAreaLocality localityField;

    /// <remarks/>
    public string SubAdministrativeAreaName
    {
      get
      {
        return this.subAdministrativeAreaNameField;
      }
      set
      {
        this.subAdministrativeAreaNameField = value;
      }
    }

    /// <remarks/>
    public AddressDetailsCountryAdministrativeAreaSubAdministrativeAreaLocality Locality
    {
      get
      {
        return this.localityField;
      }
      set
      {
        this.localityField = value;
      }
    }
  }

  /// <remarks/>
  [System.SerializableAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:ciq:xsdschema:xAL:2.0")]
  public partial class AddressDetailsCountryAdministrativeAreaSubAdministrativeAreaLocality
  {

    private string localityNameField;

    /// <remarks/>
    public string LocalityName
    {
      get
      {
        return this.localityNameField;
      }
      set
      {
        this.localityNameField = value;
      }
    }
  }

  /// <remarks/>
  [System.SerializableAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://maps.yandex.ru/geocoder/1.x")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://maps.yandex.ru/geocoder/1.x", IsNullable = false)]
  public partial class GeocoderResponseMetaData
  {

    private string requestField;

    private byte foundField;

    private byte resultsField;

    /// <remarks/>
    public string request
    {
      get
      {
        return this.requestField;
      }
      set
      {
        this.requestField = value;
      }
    }

    /// <remarks/>
    public byte found
    {
      get
      {
        return this.foundField;
      }
      set
      {
        this.foundField = value;
      }
    }

    /// <remarks/>
    public byte results
    {
      get
      {
        return this.resultsField;
      }
      set
      {
        this.resultsField = value;
      }
    }
  }

  /// <remarks/>
  [System.SerializableAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.opengis.net/gml")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.opengis.net/gml", IsNullable = false)]
  public partial class featureMember
  {

    private GeoObject geoObjectField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://maps.yandex.ru/ymaps/1.x")]
    public GeoObject GeoObject
    {
      get
      {
        return this.geoObjectField;
      }
      set
      {
        this.geoObjectField = value;
      }
    }
  }

  /// <remarks/>
  [System.SerializableAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://maps.yandex.ru/ymaps/1.x")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://maps.yandex.ru/ymaps/1.x", IsNullable = false)]
  public partial class GeoObject
  {

    private metaDataProperty metaDataPropertyField;

    private string descriptionField;

    private string nameField;

    private boundedBy boundedByField;

    private Point pointField;

    private byte idField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.opengis.net/gml")]
    public metaDataProperty metaDataProperty
    {
      get
      {
        return this.metaDataPropertyField;
      }
      set
      {
        this.metaDataPropertyField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.opengis.net/gml")]
    public string description
    {
      get
      {
        return this.descriptionField;
      }
      set
      {
        this.descriptionField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.opengis.net/gml")]
    public string name
    {
      get
      {
        return this.nameField;
      }
      set
      {
        this.nameField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.opengis.net/gml")]
    public boundedBy boundedBy
    {
      get
      {
        return this.boundedByField;
      }
      set
      {
        this.boundedByField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.opengis.net/gml")]
    public Point Point
    {
      get
      {
        return this.pointField;
      }
      set
      {
        this.pointField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.opengis.net/gml")]
    public byte id
    {
      get
      {
        return this.idField;
      }
      set
      {
        this.idField = value;
      }
    }
  }

  /// <remarks/>
  [System.SerializableAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.opengis.net/gml")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.opengis.net/gml", IsNullable = false)]
  public partial class boundedBy
  {

    private boundedByEnvelope envelopeField;

    /// <remarks/>
    public boundedByEnvelope Envelope
    {
      get
      {
        return this.envelopeField;
      }
      set
      {
        this.envelopeField = value;
      }
    }
  }

  /// <remarks/>
  [System.SerializableAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.opengis.net/gml")]
  public partial class boundedByEnvelope
  {

    private string lowerCornerField;

    private string upperCornerField;

    /// <remarks/>
    public string lowerCorner
    {
      get
      {
        return this.lowerCornerField;
      }
      set
      {
        this.lowerCornerField = value;
      }
    }

    /// <remarks/>
    public string upperCorner
    {
      get
      {
        return this.upperCornerField;
      }
      set
      {
        this.upperCornerField = value;
      }
    }
  }

  /// <remarks/>
  [System.SerializableAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.opengis.net/gml")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.opengis.net/gml", IsNullable = false)]
  public partial class Point
  {

    private string posField;

    /// <remarks/>
    public string pos
    {
      get
      {
        return this.posField;
      }
      set
      {
        this.posField = value;
      }
    }
  }

  #endregion
}

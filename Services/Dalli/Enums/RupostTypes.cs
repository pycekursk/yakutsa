using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System.ComponentModel.DataAnnotations;

namespace yakutsa.Services.Dalli.Enums
{
  [JsonConverter(typeof(StringEnumConverter))]
  public enum RupostTypes
  {
    [Display(Name = "Посылка онлайн")]
    PACKAGE_ONLINE = 1,

    [Display(Name = "Курьер онлайн")]
    COURIER_ONLINE = 2,

    [Display(Name = "Посылка нестандартная")]
    PACKAGE_UNIQUE = 3,

    [Display(Name = "Посылка 1-го класса")]
    PACKAGE_FIRST_CLASS = 4,

    UNKNOWN = 0
  }
}

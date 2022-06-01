using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System.ComponentModel.DataAnnotations;

namespace yakutsa.Services.Dalli.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DeliveryServices
    {
        [Display(Name = "Обычная Dalli-Service МСК")]
        SIMPLE_MOSCOW = 1,

        [Display(Name = "Экспресс Dalli-Service МСК")]
        EXPRESS_MOSCOW = 2,

        [Display(Name = "ПВЗ Dalli-Service МСК")]
        PVZ_MOSCOW = 4,

        PICKUP_IM = 5,
        RETURN_GOODS = 7,
        RETURN_MONEY = 8,

        [Display(Name = "ПВЗ СДЭК")]
        PVZ_SDEK = 9,

        [Display(Name = "Курьерская СДЭК")]
        COURIER_SDEK = 10,

        [Display(Name = "Обычная Dalli-Service СПБ")]
        SIMPLE_SPB = 11,


        [Display(Name = "ПВЗ Dalli-Service СПБ")]
        PVZ_SPB = 12,

        [Display(Name = "ПВЗ BOXBERRY")]
        PVZ_BOXBERRY = 13,

        PICKUP_RECEIVER = 14,

        [Display(Name = "ПВЗ PickPoint")]
        PVZ_PICKPOINT = 15,

        EXPRESS_PICKUP = 16,
        FIRST_MILE_PLUS = 17,

        [Display(Name = "5POST Пятерочка")]
        _5POST = 20,

        [Display(Name = "Почта России")]
        RUPOST = 19,

        UNKNOWN = 0
    }

   
}

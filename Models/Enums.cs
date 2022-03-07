using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.ComponentModel.DataAnnotations;

namespace yakutsa.Models
{
  public static class Enums
  {
    public enum ProductSubCategory
    {
      Unknown = 0,
    }

    public enum ProductCategory
    {
      Unknown = 0,
      Ground = 1,
      Air = 2,
    }

    public enum ItemState
    {
      Active = 0,
      Archive = 1
    }

    public enum ClientType
    {
      Unknown = 0,
      Personal = 1,
      Organization = 2
    }

    public enum PaymentState
    {
      [Display(Name = "Ожидает оплаты")]
      WaitPay = 0,

      [Display(Name = "Оплачен")]
      Payed = 1
    }

    public enum OrderState
    {
      [Display(Name = "Черновик")]
      Unknown = 0,

      [Display(Name = "Создан")]
      Created = 1,

      [Display(Name = "В работе")]
      InProgress = 2,

      [Display(Name = "Завершен")]
      Complete = 3,

      [Display(Name = "Отменен")]
      Canceled = 4
    }

    public enum DeliveryType
    {
      Unknown = 0,

      [Display(Name = "Доставка")]
      Delivery = 1,

      [Display(Name = "Самовывоз")]
      Pickup = 2
    }

    public enum PaymentType
    {
      [Display(Name = "Наличные")]
      Cash = 0,

      [Display(Name = "Онлайн")]
      Online = 1
    }

    public enum OrderByDirection
    {
      Ascend = 0,
      Descend = 1
    }

    public enum PriceModel
    {
      [Display(Name = "Нет")]
      Unknown = 0,

      [Display(Name = "Розница")]
      Standart = 1,

      [Display(Name = "Акция")]
      Event = 6,

      [Display(Name = "Розница акция")]
      StandartEvent = 7,

      [Display(Name = "Закупка")]
      Base = 3,

      [Display(Name = "Опт")]
      Partners = 5
    }

    public enum MessageType
    {
      main = 0,
      danger = 1,
      success = 2
    }
  }
}

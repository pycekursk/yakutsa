using Newtonsoft.Json;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using yakutsa.Data;

namespace yakutsa.Models
{
    public class HtmlDisplayAttribute : Attribute
    {
        public bool IsVisible { get; set; }
        public HtmlDisplayAttribute(bool isVisible)
        {
            IsVisible = isVisible;
        }
    }



    public class Loyalty : BaseModel
    {
        [HtmlDisplay(false)]
        public override string? Name { get; set; }
        public List<PromoCode>? PromoCodes { get; set; } = new List<PromoCode>();

        public List<PromoCode>? GenerateCodes(string codeText, int count, PromoCodeType promoCodeType, double value)
        {
            var codes = new List<PromoCode>();

            for (int i = 0; i < count; i++)
            {
                var code = new PromoCode() { CodeText = codeText, PromoCodeType = promoCodeType, Value = value };
                codes.Add(code);
            }

            PromoCodes?.AddRange(codes);

            using (var ctx = new ApplicationDbContext(new Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext>()))
            {
                ctx.Loyalty.Update(this);
                ctx.SaveChanges();
            }

            return PromoCodes;
        }
    }

    public class PromoCode : BaseModel
    {
        private bool isUsed;

        [RetailCRMCore.Models.TableAttribute("Код")]
        public string? CodeText { get; set; }

        [RetailCRMCore.Models.TableAttribute("Скидка (руб/%)")]
        public double Value { get; set; }

        [RetailCRMCore.Models.TableAttribute("Тип скидки")]
        public PromoCodeType PromoCodeType { get; set; } = PromoCodeType.Fixed;

        public bool IsUsed { get => isUsed; set { isUsed = value; if (value) { IsActive = false; UsedAt = DateTime.Now; } } }

        [RetailCRMCore.Models.TableAttribute("Активность")]
        public bool IsActive { get; set; }

        public DateTime? UsedAt { get; set; }

        [JsonIgnore]
        public Loyalty? Loyalty { get; set; }

        [ForeignKey("Loyalty")]
        public int ParentId { get; set; }
    }

    public enum PromoCodeType
    {
        [Display(Name = "Процент")]
        Dynamic,
        [Display(Name = "Фиксированная")]
        Fixed
    }
}

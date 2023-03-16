using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using yakutsa.Models;
using System.ComponentModel.DataAnnotations;

namespace yakutsa.Services.Ozon
{
    public class OzonSettings : BaseModel
    {
        [HtmlDisplay(false)]
        [DisplayName("Name")]
        public override string? Name { get => base.Name; set => base.Name = value; }

        public string ApiKey { get; set; }
        public string ClientId { get; set; }
    }
}

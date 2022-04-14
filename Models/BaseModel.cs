using yakutsa.Data;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace yakutsa.Models
{
  public class BaseModel
  {
    private int id;
    private string? name;

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [DisplayName("Id")]
    public virtual int Id { get => id; set => id = value == 0 ? id : value; }

    [Display(Name = "Имя")]
    public virtual string? Name { get => name; set => name = value; }

    public BaseModel() { }

    public string? GetName(string name)
    {
      var props = this.GetType().GetProperties();
      var prop = props.Where(p => p.Name == name).FirstOrDefault();
      if (prop == null) return null;
      object[] attrs = prop.GetCustomAttributes(false);
      foreach (DisplayNameAttribute attr in attrs)
      {
        return attr.DisplayName;
      }
      return name;
    }
  }
}

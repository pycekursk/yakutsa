namespace yakutsa.Services.GeoHelper.Models
{
    public class Localitytype
    {
        public string code { get; set; }
        public string name { get; set; }
        public Localizednamesshort localizedNamesShort { get; set; }
        public Localizednames localizedNames { get; set; }
    }
}

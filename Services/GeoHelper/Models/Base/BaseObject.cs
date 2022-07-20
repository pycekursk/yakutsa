namespace yakutsa.Services.GeoHelper.Models.Base
{
    public class BaseObject
    {
        public int id { get; set; }
        public string name { get; set; }
        public Codes codes { get; set; }
        public virtual int parentId { get; set; }
        public Localitytype localityType { get; set; }
        public Externalids externalIds { get; set; }
        public Localizednames localizedNames { get; set; }
    }
}

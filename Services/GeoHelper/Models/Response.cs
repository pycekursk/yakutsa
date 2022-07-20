namespace yakutsa.Services.GeoHelper.Models
{
    public class Response<T>
    {
        public bool success { get; set; }
        public string? language { get; set; }
        public T[]? result { get; set; }
        public Pagination? pagination { get; set; }

        public Response<T> GetResponse()
        {
            Response<T> result = new Response<T>();

            throw new NotImplementedException();

            //return result;
        }
    }

    public class Pagination
    {
        public int limit { get; set; }
        public int totalCount { get; set; }
        public int currentPage { get; set; }
        public int totalPageCount { get; set; }
    }
}

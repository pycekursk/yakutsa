
using Newtonsoft.Json;

namespace yakutsa.Services.Ozon.Models
{
  public class ProductImportResponseData
  {
    [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
    public Result Result { get; set; }
  }

  public partial class Result
  {
    [JsonProperty("task_id", NullValueHandling = NullValueHandling.Ignore)]
    public long TaskId { get; set; }
  }

}

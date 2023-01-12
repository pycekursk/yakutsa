namespace yakutsa.Services.OzonSellerApi
{
  public class OzonClient
  {
    HttpClient _client;
    public string ApiKey { get; private set; }
    public string ClientId { get; private set; }

#nullable disable
    public OzonClient(string clientId, string apiKey)
    {
      if (apiKey == null) throw new ArgumentNullException(nameof(apiKey));
      if (clientId == null) throw new ArgumentNullException(nameof(clientId));
      _client = new HttpClient();
      var result = _client.DefaultRequestHeaders.TryAddWithoutValidation("Client-Id", clientId);
      if (result) ClientId = clientId;
      result = _client.DefaultRequestHeaders.TryAddWithoutValidation("Api-Key", apiKey);
      if (result) ApiKey = apiKey;
    }
  }
}

namespace yakutsa.Services
{
    public class ITWHttpClient : HttpClient
    {
        public delegate void RequestEvent();
        public event RequestEvent RequestEventHandler;

        public ITWHttpClient()
        {

        }

        public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestEventHandler?.Invoke();
            return base.SendAsync(request, cancellationToken);
        }

        public override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestEventHandler?.Invoke();
            return base.Send(request, cancellationToken);
        }
    }
}

using Flurl.Http.Configuration;

namespace CSharpFileUpload.SupportClass
{
    internal class OHttpClientFactory : DefaultHttpClientFactory
    {
        // override to customize how HttpClient is created/configured
        public override HttpClient CreateHttpClient(HttpMessageHandler handler)
        {
            var socketsHandler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                PooledConnectionIdleTimeout = TimeSpan.FromSeconds(30),
                MaxConnectionsPerServer = 3,
                AllowAutoRedirect=true,
              
                
            };
            return new HttpClient(socketsHandler);

        }

        //// override to customize how HttpMessageHandler is created/configured
        //public override HttpMessageHandler CreateMessageHandler()
        //{

        //}
    }
}

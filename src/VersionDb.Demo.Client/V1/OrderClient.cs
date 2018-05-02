using System.Net.Http;
using VersionDb.Client;

namespace VersionDb.Demo.V1
{
    public class OrderClient : SimpleVersionDbClient<Order>, IOrderClient
    {
        public OrderClient(HttpClient httpClient) : base(httpClient, 
            OrderVersions.CanonicalName, OrderVersions.V1)
        {
        }
    }
}
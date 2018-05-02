using System.Net.Http;
using VersionDb.Client;

namespace VersionDb.Demo.V2
{
    public class OrderClient : SimpleVersionDbClient<Order>, IOrderClient
    {
        public OrderClient(HttpClient httpClient) : base(httpClient, 
            OrderVersions.CanonicalName, OrderVersions.V2)
        {
            
        }
    }
}
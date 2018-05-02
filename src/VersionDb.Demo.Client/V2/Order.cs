using System;
using System.Net.Http;
using VersionDb.Client;

namespace VersionDb.Demo.V2
{
    public class Order
    {
        public string Id { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public PaymentInfo Payments { get; set; }
    }

    public interface IOrderClient : IVersionDbClient<Order> {}

    public class OrderClient : SimpleVersionDbClient<Order>, IOrderClient
    {
        public OrderClient(HttpClient httpClient, string typeName, string versionName) : base(httpClient, typeName, versionName)
        {
        }
    }
}

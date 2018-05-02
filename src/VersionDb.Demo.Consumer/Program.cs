using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VersionDb.Client;

namespace VersionDb.Demo.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            string hostname = "http://localhost:5000";
            
            Task.Run(() => Watcher(hostname));

            
            V2.IOrderClient orderClient = new V2.OrderClient(
                new HttpClient()
                {
                    BaseAddress = new Uri(hostname)
                });
            
            while ( true ) 
            {
                orderClient.Post("cat", new V2.Order
                {
                    Id = "cat",
                    CreatedDate = DateTime.Now,
                    Payments = new V2.PaymentInfo
                    {
                        Amount = 10.0,
                        Date = DateTime.Now,
                        Method = "VISA"
                    }
                });

                Thread.Sleep(1500);
            }
        }

        private static void Watcher(string hostname)
        {
            V1.IOrderClient orderClient = new V1.OrderClient(
                new HttpClient()
                {
                    BaseAddress = new Uri(hostname)
                });

            foreach (Change<V1.Order> change in orderClient.Watch("cat"))
            {
                if (change.Value == null) continue; // Ignore deletions in this demo

                DateTimeOffset createdDate = change.Value.PaymentDetails.Single().Date;
                TimeSpan latency = DateTimeOffset.Now - createdDate;

                Console.WriteLine($"{createdDate:o} - Latency: {latency.TotalMilliseconds}ms");
            }
        }
    }
}

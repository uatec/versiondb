using System;

namespace VersionDb.V1
{
    public class Order
    {
        public string Id { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public PaymentDetail[] PaymentDetails { get; set; }
        public double PaymentTotal { get; set; }
    }
}

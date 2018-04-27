using System;

namespace VersionDb.V2
{
    public class Order
    {
        public string Id { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public PaymentInfo Payments { get; set; }
    }
}

using System;

namespace VersionDb.V1
{
    public class PaymentDetail 
    {
        public string Method { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}

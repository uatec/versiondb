﻿using System;

namespace VersionDb.V2
{
    public class PaymentInfo 
    {
        public string Method { get; set; }
        public DateTimeOffset Date { get; set; }
        public double Amount { get; set; }
    }
}

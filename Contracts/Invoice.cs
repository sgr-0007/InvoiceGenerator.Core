
using System;
using System.Collections.Generic;

namespace InvoiceGenerator.Core.Contracts
{
    public class Invoice
    {
        public required string Number          { get; set; }
        public required DateTime IssuedDate    { get; set; }
        public required DateTime DueDate       { get; set; }

        public required Address SellerAddress   { get; set; }
        public required Address CustomerAddress { get; set; }

        public List<LineItem> LineItems { get; set; } = new List<LineItem>();
    }
}
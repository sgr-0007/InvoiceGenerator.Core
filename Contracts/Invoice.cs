
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

        public required List<LineItem> LineItems { get; set; } = new();

        // A dictionary for any custom fields the user wants to add
        public Dictionary<string, string> CustomFields { get; set; } = new();
    }
}
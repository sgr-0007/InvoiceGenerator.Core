using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PdfReporting.Core.Contracts
{
    public class Address
    {
        public required string CompanyName { get; set; }
        public required string Street { get; set; }
        public required string City { get; set; }
        public required string State { get; set; }
        public required string Email { get; set; }
    }
}
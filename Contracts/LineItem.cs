using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PdfReporting.Core.Contracts
{
    public class LineItem
    {

        public string Name { get; set; }

        public decimal Price { get; set; }

        public decimal Quantity { get; set; }
    }
}
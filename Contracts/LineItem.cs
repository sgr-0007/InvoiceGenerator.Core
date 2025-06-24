using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InvoiceGenerator.Core.Contracts
{
    /// <summary>
    /// Represents a line item in an invoice
    /// </summary>
    public class LineItem
    {
        /// <summary>
        /// Gets or sets the name or description of the item
        /// </summary>
        [Required(ErrorMessage = "Item name is required")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the price per unit of the item
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the quantity of the item
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }
        
        /// <summary>
        /// Gets the total price for this line item (Price * Quantity)
        /// </summary>
        public decimal Total => Price * Quantity;
    }
}
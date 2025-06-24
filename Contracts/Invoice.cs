
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace InvoiceGenerator.Core.Contracts
{
    /// <summary>
    /// Represents an invoice document with seller, customer, and line item information
    /// </summary>
    public class Invoice
    {
        /// <summary>
        /// Gets or sets the invoice number
        /// </summary>
        [Required(ErrorMessage = "Invoice number is required")]
        public required string Number { get; set; }

        /// <summary>
        /// Gets or sets the date when the invoice was issued
        /// </summary>
        [Required(ErrorMessage = "Issue date is required")]
        public required DateTime IssuedDate { get; set; }

        /// <summary>
        /// Gets or sets the due date for payment
        /// </summary>
        [Required(ErrorMessage = "Due date is required")]
        public required DateTime DueDate { get; set; }

        /// <summary>
        /// Gets or sets the seller's address and contact information
        /// </summary>
        [Required(ErrorMessage = "Seller address is required")]
        public required Address SellerAddress { get; set; }

        /// <summary>
        /// Gets or sets the customer's address and contact information
        /// </summary>
        [Required(ErrorMessage = "Customer address is required")]
        public required Address CustomerAddress { get; set; }

        /// <summary>
        /// Gets or sets the list of line items in the invoice
        /// </summary>
        [Required(ErrorMessage = "At least one line item is required")]
        [MinLength(1, ErrorMessage = "At least one line item is required")]
        public required List<LineItem> LineItems { get; set; } = new();

        /// <summary>
        /// Gets or sets a dictionary of custom fields for the invoice
        /// </summary>
        public Dictionary<string, string> CustomFields { get; set; } = new();

        /// <summary>
        /// Gets the subtotal amount of the invoice (sum of all line items)
        /// </summary>
        public decimal Subtotal => LineItems.Sum(item => item.Total);

        /// <summary>
        /// Gets or sets the tax rate as a percentage (e.g., 7.5 for 7.5%)
        /// </summary>
        [Range(0, 100, ErrorMessage = "Tax rate must be between 0 and 100")]
        public decimal TaxRate { get; set; } = 0;

        /// <summary>
        /// Gets the tax amount based on the subtotal and tax rate
        /// </summary>
        public decimal TaxAmount => Subtotal * (TaxRate / 100);

        /// <summary>
        /// Gets the total amount of the invoice including tax
        /// </summary>
        public decimal Total => Subtotal + TaxAmount;

        /// <summary>
        /// Gets or sets any notes to be displayed on the invoice
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Gets or sets the currency code (e.g., USD, EUR)
        /// </summary>
        public string CurrencyCode { get; set; } = "USD";
    }
}
using Microsoft.ML.Data;

namespace InvoiceGenerator.Core.ML.Models
{
    /// <summary>
    /// Represents the input data for invoice layout optimization
    /// </summary>
    public class InvoiceLayoutData
    {
        /// <summary>
        /// Number of line items in the invoice
        /// </summary>
        [LoadColumn(0)]
        public float LineItemCount { get; set; }

        /// <summary>
        /// Total amount of the invoice
        /// </summary>
        [LoadColumn(1)]
        public float InvoiceTotal { get; set; }

        /// <summary>
        /// Number of custom fields in the invoice
        /// </summary>
        [LoadColumn(2)]
        public float CustomFieldCount { get; set; }

        /// <summary>
        /// Length of the notes section
        /// </summary>
        [LoadColumn(3)]
        public float NotesLength { get; set; }

        /// <summary>
        /// Whether the invoice is for a business (true) or individual (false)
        /// </summary>
        [LoadColumn(4)]
        public bool IsBusinessCustomer { get; set; }

        /// <summary>
        /// The currency code used in the invoice
        /// </summary>
        [LoadColumn(5)]
        public string CurrencyCode { get; set; } = "USD";
        
        /// <summary>
        /// The layout template to use for this invoice (label column)
        /// </summary>
        [LoadColumn(8)]
        public string LayoutTemplate { get; set; } = "Standard";
    }
}

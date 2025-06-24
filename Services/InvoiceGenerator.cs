using InvoiceGenerator.Core.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

#nullable enable

namespace InvoiceGenerator.Core.Services
{
    /// <summary>
    /// Implementation of IInvoiceGenerator that generates PDF invoices using Razor templates
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the InvoicePdfGenerator class
    /// </remarks>
    /// <param name="razorRenderer">The razor view renderer service</param>
    /// <param name="pdfGenerator">The PDF generator service</param>
    /// <param name="logger">Optional logger for diagnostic information</param>
    public class InvoicePdfGenerator(
        IRazorViewToStringRenderer razorRenderer,
        IPdfGenerator pdfGenerator,
        ILogger<InvoicePdfGenerator>? logger = null) : IInvoiceGenerator
    {
        private readonly IRazorViewToStringRenderer _razorRenderer = razorRenderer ?? throw new ArgumentNullException(nameof(razorRenderer));
        private readonly IPdfGenerator _pdfGenerator = pdfGenerator ?? throw new ArgumentNullException(nameof(pdfGenerator));
        private readonly ILogger<InvoicePdfGenerator>? _logger = logger;
        private string _templatePath = "Views/InvoiceReport.cshtml";

        /// <summary>
        /// Gets or sets the path to the Razor template used for invoice generation
        /// </summary>
        public string TemplatePath
        {
            get => _templatePath;
            set => _templatePath = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Generates a PDF invoice from the provided invoice data
        /// </summary>
        /// <param name="invoice">The invoice data to generate a PDF for</param>
        /// <returns>The generated PDF as a byte array</returns>
        /// <exception cref="ArgumentNullException">Thrown when invoice is null</exception>
        /// <exception cref="ValidationException">Thrown when invoice data is invalid</exception>
        public async Task<byte[]> GenerateAsync(Invoice invoice)
        {
            if (invoice == null)
            {
                throw new ArgumentNullException(nameof(invoice));
            }

            // Validate the invoice data
            ValidateInvoice(invoice);

            try
            {
                _logger?.LogInformation("Generating invoice PDF for invoice number {InvoiceNumber}", invoice.Number);

                // Render the Razor view to HTML
                string html = await _razorRenderer.RenderViewToStringAsync(_templatePath, invoice);
                
                // Generate the PDF from the HTML
                byte[] pdfData = _pdfGenerator.GeneratePdf(html);
                
                _logger?.LogInformation("Successfully generated PDF for invoice {InvoiceNumber}, size: {Size} bytes", 
                    invoice.Number, pdfData.Length);
                    
                return pdfData;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating PDF for invoice {InvoiceNumber}", invoice.Number);
                throw;
            }
        }

        /// <summary>
        /// Validates the invoice data
        /// </summary>
        /// <param name="invoice">The invoice to validate</param>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        private static void ValidateInvoice(Invoice invoice)
        {
            // Ensure due date is not before issue date
            if (invoice.DueDate < invoice.IssuedDate)
            {
                throw new ValidationException("Due date cannot be earlier than issue date");
            }

            // Ensure there is at least one line item
            if (invoice.LineItems == null || invoice.LineItems.Count == 0)
            {
                throw new ValidationException("Invoice must have at least one line item");
            }

            // Ensure all required addresses are present
            if (invoice.SellerAddress == null)
            {
                throw new ValidationException("Seller address is required");
            }

            if (invoice.CustomerAddress == null)
            {
                throw new ValidationException("Customer address is required");
            }
        }
    }
}

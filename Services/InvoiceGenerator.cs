using InvoiceGenerator.Core.Contracts;
using InvoiceGenerator.Core.ML.Models;
using InvoiceGenerator.Core.ML.Services;
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
    /// <param name="layoutOptimizer">Optional layout optimizer for ML-based layout optimization</param>
    /// <param name="logger">Optional logger for diagnostic information</param>
    public class InvoicePdfGenerator(
        IRazorViewToStringRenderer razorRenderer,
        IPdfGenerator pdfGenerator,
        IInvoiceLayoutOptimizer? layoutOptimizer = null,
        ILogger<InvoicePdfGenerator>? logger = null) : IInvoiceGenerator
    {
        private readonly IRazorViewToStringRenderer _razorRenderer = razorRenderer ?? throw new ArgumentNullException(nameof(razorRenderer));
        private readonly IPdfGenerator _pdfGenerator = pdfGenerator ?? throw new ArgumentNullException(nameof(pdfGenerator));
        private readonly IInvoiceLayoutOptimizer? _layoutOptimizer = layoutOptimizer;
        private readonly ILogger<InvoicePdfGenerator>? _logger = logger;
        private string _templatePath = "Views/InvoiceReport.cshtml";
        private bool _useSmartLayout = true;

        /// <summary>
        /// Gets or sets the path to the Razor template used for invoice generation
        /// </summary>
        public string TemplatePath
        {
            get => _templatePath;
            set => _templatePath = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets whether to use ML-based smart layout optimization
        /// </summary>
        public bool UseSmartLayout
        {
            get => _useSmartLayout;
            set => _useSmartLayout = value;
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
            ArgumentNullException.ThrowIfNull(invoice);

            // Validate the invoice data
            ValidateInvoice(invoice);

            try
            {
                _logger?.LogInformation("Generating invoice PDF for invoice number {InvoiceNumber}", invoice.Number);

                // Apply ML-based layout optimization if enabled and available
                InvoiceLayoutOptions? layoutOptions = null;
                if (_useSmartLayout && _layoutOptimizer != null)
                {
                    _logger?.LogInformation("Applying ML-based layout optimization for invoice {InvoiceNumber}", invoice.Number);
                    var prediction = _layoutOptimizer.PredictOptimalLayout(invoice);
                    layoutOptions = InvoiceLayoutOptions.FromPrediction(prediction);
                    
                    // Select template based on ML prediction if applicable
                    if (!string.IsNullOrEmpty(layoutOptions.LayoutTemplate) && layoutOptions.LayoutTemplate != "Standard")
                    {
                        _templatePath = $"Views/InvoiceReport.{layoutOptions.LayoutTemplate}.cshtml";
                        _logger?.LogInformation("Using ML-recommended template: {TemplatePath}", _templatePath);
                    }
                }

                // Create a view model that includes both invoice data and layout options
                var viewModel = new
                {
                    Invoice = invoice,
                    Layout = layoutOptions ?? new InvoiceLayoutOptions()
                };

                // Render the Razor view to HTML
                string html = await _razorRenderer.RenderViewToStringAsync(_templatePath, viewModel);
                
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

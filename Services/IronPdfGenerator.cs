using IronPdf;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

#nullable enable

namespace InvoiceGenerator.Core.Services
{
    /// <summary>
    /// Exception thrown when PDF generation fails
    /// </summary>
    public class PdfGenerationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfGenerationException"/> class
        /// </summary>
        public PdfGenerationException(string message) : base(message)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfGenerationException"/> class
        /// </summary>
        public PdfGenerationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Implementation of IPdfGenerator using IronPdf library
    /// </summary>
    public class IronPdfGenerator : IPdfGenerator
    {
        private readonly ChromePdfRenderer _renderer;
        private readonly ILogger<IronPdfGenerator>? _logger;
        
        /// <summary>
        /// Gets or sets the page size for the PDF
        /// </summary>
        public string PageSize { get; set; } = "A4";

        /// <summary>
        /// Gets or sets the orientation for the PDF
        /// </summary>
        public string Orientation { get; set; } = "Portrait";
        
        /// <summary>
        /// Gets or sets the margins for generated PDFs in millimeters
        /// </summary>
        public int MarginMm { get; set; } = 20;
        
        /// <summary>
        /// Gets or sets the title for generated PDFs
        /// </summary>
        public string? DocumentTitle { get; set; }

        /// <summary>
        /// Initializes a new instance of the IronPdfGenerator class
        /// </summary>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public IronPdfGenerator(ILogger<IronPdfGenerator>? logger = null)
        {
            _renderer = new ChromePdfRenderer();
            _logger = logger;
            
            // Set default renderer options
            _renderer.RenderingOptions.PaperSize = ParsePageSize(PageSize);
            _renderer.RenderingOptions.PaperOrientation = ParseOrientation(Orientation);
            _renderer.RenderingOptions.MarginTop = MarginMm;
            _renderer.RenderingOptions.MarginBottom = MarginMm;
            _renderer.RenderingOptions.MarginLeft = MarginMm;
            _renderer.RenderingOptions.MarginRight = MarginMm;
            _renderer.RenderingOptions.CreatePdfFormsFromHtml = true;
            _renderer.RenderingOptions.CssMediaType = IronPdf.Rendering.PdfCssMediaType.Print;
        }

        /// <summary>
        /// Generates a PDF document from HTML content
        /// </summary>
        /// <param name="html">The HTML content to convert to PDF</param>
        /// <returns>The generated PDF as a byte array</returns>
        /// <exception cref="ArgumentNullException">Thrown when html is null or empty</exception>
        /// <exception cref="Exception">Thrown when PDF generation fails</exception>
        public byte[] GeneratePdf(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                throw new ArgumentNullException(nameof(html), "HTML content cannot be null or empty");
            }

            try
            {
                _logger?.LogInformation("Generating PDF with IronPdf");
                
                // Configure the renderer
                // Convert string page size to IronPdf enum
                _renderer.RenderingOptions.PaperSize = ParsePageSize(PageSize);
                _renderer.RenderingOptions.PaperOrientation = ParseOrientation(Orientation);
                _renderer.RenderingOptions.MarginTop = MarginMm;
                _renderer.RenderingOptions.MarginBottom = MarginMm;
                _renderer.RenderingOptions.MarginLeft = MarginMm;
                _renderer.RenderingOptions.MarginRight = MarginMm;
                
                if (!string.IsNullOrEmpty(DocumentTitle))
                {
                    _renderer.RenderingOptions.Title = DocumentTitle;
                }
                
                // Generate the PDF
                using var doc = _renderer.RenderHtmlAsPdf(html);
                
                _logger?.LogInformation("PDF generated successfully, size: {Size} bytes", doc.BinaryData.Length);
                return doc.BinaryData;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating PDF with IronPdf");
                throw new Exception("Failed to generate PDF document", ex);
            }
        }
        
        /// <summary>
        /// Generates a PDF document from HTML content and saves it to a file
        /// </summary>
        /// <param name="html">The HTML content to convert to PDF</param>
        /// <param name="outputPath">The file path where the PDF should be saved</param>
        /// <returns>The path to the saved PDF file</returns>
        /// <exception cref="ArgumentNullException">Thrown when html or outputPath is null or empty</exception>
        /// <exception cref="PdfGenerationException">Thrown when PDF generation or saving fails</exception>
        public string GeneratePdfToFile(string html, string outputPath)
        {
            if (string.IsNullOrEmpty(outputPath))
            {
                throw new ArgumentNullException(nameof(outputPath), "Output path cannot be null or empty");
            }
            
            try
            {
                _logger?.LogInformation("Generating PDF with IronPdf and saving to {OutputPath}", outputPath);
                
                // Generate the PDF
                using var doc = _renderer.RenderHtmlAsPdf(html);
                
                // Save to file
                doc.SaveAs(outputPath);
                
                _logger?.LogInformation("PDF saved successfully to {OutputPath}", outputPath);
                return outputPath;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving PDF to {OutputPath}: {ErrorMessage}", outputPath, ex.Message);
                throw new PdfGenerationException($"Error saving PDF to {outputPath}", ex);
            }
        }
        
        // SavePdfToFile method has been merged with GeneratePdfToFile
        
        /// <summary>
        /// Converts a string page size to IronPdf PdfPaperSize enum
        /// </summary>
        /// <param name="pageSize">String representation of page size</param>
        /// <returns>IronPdf PdfPaperSize enum value</returns>
        private IronPdf.Rendering.PdfPaperSize ParsePageSize(string pageSize)
        {
            if (string.IsNullOrEmpty(pageSize))
            {
                return IronPdf.Rendering.PdfPaperSize.A4; // Default
            }
            
            switch (pageSize.ToUpperInvariant())
            {
                case "A4":
                    return IronPdf.Rendering.PdfPaperSize.A4;
                case "LETTER":
                    return IronPdf.Rendering.PdfPaperSize.Letter;
                case "LEGAL":
                    return IronPdf.Rendering.PdfPaperSize.Legal;
                default:
                    _logger?.LogWarning("Unsupported page size: {PageSize}, using A4 as default", pageSize);
                    return IronPdf.Rendering.PdfPaperSize.A4;
            }
        }
        
        /// <summary>
        /// Converts a string orientation to IronPdf PdfPaperOrientation enum
        /// </summary>
        /// <param name="orientation">String representation of orientation</param>
        /// <returns>IronPdf PdfPaperOrientation enum value</returns>
        private IronPdf.Rendering.PdfPaperOrientation ParseOrientation(string orientation)
        {
            if (string.IsNullOrEmpty(orientation))
            {
                return IronPdf.Rendering.PdfPaperOrientation.Portrait; // Default
            }
            
            return orientation.ToUpperInvariant() == "LANDSCAPE" 
                ? IronPdf.Rendering.PdfPaperOrientation.Landscape 
                : IronPdf.Rendering.PdfPaperOrientation.Portrait;
        }
    }
}

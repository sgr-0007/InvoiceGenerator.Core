using InvoiceGenerator.Core.Contracts;
using System.Threading.Tasks;

namespace InvoiceGenerator.Core.Services
{
    public class InvoicePdfGenerator : IInvoiceGenerator
    {
        private readonly IRazorViewToStringRenderer _razorRenderer;
        private readonly IPdfGenerator _pdfGenerator;

        public InvoicePdfGenerator(IRazorViewToStringRenderer razorRenderer, IPdfGenerator pdfGenerator)
        {
            _razorRenderer = razorRenderer;
            _pdfGenerator = pdfGenerator;
        }

        public async Task<byte[]> GenerateAsync(Invoice invoice)
        {
            // Render the Razor view to HTML
            string html = await _razorRenderer.RenderViewToStringAsync("Views/InvoiceReport.cshtml", invoice);
            
            // Generate the PDF from the HTML
            return _pdfGenerator.GeneratePdf(html);
        }
    }
}

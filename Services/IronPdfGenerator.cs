using IronPdf;

namespace InvoiceGenerator.Core.Services
{
    public class IronPdfGenerator : IPdfGenerator
    {
        private readonly ChromePdfRenderer _renderer;

        public IronPdfGenerator()
        {
            _renderer = new ChromePdfRenderer();
        }

        public byte[] GeneratePdf(string html)
        {
            using var doc = _renderer.RenderHtmlAsPdf(html);
            return doc.BinaryData;
        }
    }
}

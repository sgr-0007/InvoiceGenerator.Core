

namespace InvoiceGenerator.Core.Services
{
    public interface IPdfGenerator
    {
        byte[] GeneratePdf(string html);
    }
}
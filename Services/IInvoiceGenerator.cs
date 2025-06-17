using InvoiceGenerator.Core.Contracts;
using System.Threading.Tasks;

namespace InvoiceGenerator.Core.Services
{
    public interface IInvoiceGenerator
    {
        Task<byte[]> GenerateAsync(Invoice invoice);
    }
}

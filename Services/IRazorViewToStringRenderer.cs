
using System.Threading.Tasks;

namespace InvoiceGenerator.Core.Services
{
    public interface IRazorViewToStringRenderer
    {
                Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);

    }
}
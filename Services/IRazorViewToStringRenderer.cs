
using System.Threading.Tasks;

namespace PdfReporting.Core.Services
{
    public interface IRazorViewToStringRenderer
    {
                Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);

    }
}
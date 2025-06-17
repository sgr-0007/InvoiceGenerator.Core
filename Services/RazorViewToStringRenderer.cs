using System.Threading.Tasks;
using Razor.Templating.Core;

namespace InvoiceGenerator.Core.Services
{
    public class RazorViewToStringRenderer : IRazorViewToStringRenderer
    {
        public Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
        {
            return RazorTemplateEngine.RenderAsync(viewName, model);
        }
    }
}
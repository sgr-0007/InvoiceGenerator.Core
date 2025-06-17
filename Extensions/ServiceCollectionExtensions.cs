using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IronPdf;
using InvoiceGenerator.Core.Services;

namespace InvoiceGenerator.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInvoiceGenerator(this IServiceCollection services)
        {
            // 1) Build a Configuration that reads:
            //    - appsettings.json next to the DLL (if exists)
            //    - environment variables (to allow overrides)
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddEnvironmentVariables();
                
            // Make appsettings.json optional
            string settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (File.Exists(settingsPath))
            {
                configBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
            }
            
            var config = configBuilder.Build();

            // In production, you would want to require this key
            var licenseKey = config["IronPdf:LicenseKey"] ?? "IRONPDF-KEY";

            // 3) Apply it once, up front
            License.LicenseKey = licenseKey;

            // 4) Now register the rest of your PDF services
            services.AddRazorTemplating();
            services.AddSingleton<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
            services.AddSingleton<IPdfGenerator, IronPdfGenerator>();
            services.AddSingleton<IInvoiceGenerator, InvoicePdfGenerator>();

            return services;
        }
    }
}

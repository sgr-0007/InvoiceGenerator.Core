using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IronPdf;
using PdfReporting.Core.Services;

namespace PdfReporting.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPdfReporting(this IServiceCollection services)
        {
            // 1) Build a Configuration that reads:
            //    - appsettings.json next to the DLL
            //    - environment variables (to allow overrides)
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            // 2) Pull out the IronPdf key (or throw if missing)
            var licenseKey = config["IronPdf:LicenseKey"]
                             ?? throw new InvalidOperationException(
                                 "Missing IronPdf:LicenseKey in appsettings.json or env-var IRONPDF_IronPdf__LicenseKey."
                             );

            // 3) Apply it once, up front
            License.LicenseKey = licenseKey;

            // 4) Now register the rest of your PDF services
            services.AddRazorTemplating();
            services.AddSingleton<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
            services.AddSingleton<IPdfGenerator, IronPdfGenerator>();

            return services;
        }
    }
}

using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IronPdf;
using InvoiceGenerator.Core.Services;


namespace InvoiceGenerator.Core.Extensions
{
    /// <summary>
    /// Extension methods for configuring InvoiceGenerator services in an IServiceCollection
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds and configures InvoiceGenerator services to the specified IServiceCollection
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <returns>The IServiceCollection so that additional calls can be chained</returns>
        public static IServiceCollection AddInvoiceGenerator(this IServiceCollection services)
        {
            return AddInvoiceGenerator(services, options => { });
        }

        /// <summary>
        /// Adds and configures InvoiceGenerator services to the specified IServiceCollection with custom options
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <param name="configureOptions">A delegate to configure the InvoiceGeneratorOptions</param>
        /// <returns>The IServiceCollection so that additional calls can be chained</returns>
        public static IServiceCollection AddInvoiceGenerator(
            this IServiceCollection services,
            Action<InvoiceGeneratorOptions> configureOptions)
        {
            // Create default options
            var options = new InvoiceGeneratorOptions();
            
            // Apply user configuration
            configureOptions(options);
            
            // Build configuration
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(options.ConfigurationBasePath ?? AppContext.BaseDirectory)
                .AddEnvironmentVariables();
                
            // Add appsettings.json if it exists and is enabled
            if (options.UseAppSettings)
            {
                string settingsPath = Path.Combine(options.ConfigurationBasePath ?? AppContext.BaseDirectory, "appsettings.json");
                if (File.Exists(settingsPath))
                {
                    configBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: options.ReloadConfigOnChange);
                }
            }
            
            // Build the configuration
            var config = configBuilder.Build();

            // Get IronPdf license key from configuration or options
            var licenseKey = options.IronPdfLicenseKey ?? config["IronPdf:LicenseKey"];
            
            // Apply license key if provided
            if (!string.IsNullOrEmpty(licenseKey))
            {
                License.LicenseKey = licenseKey;
            }

            // Register services
            services.AddRazorTemplating();
            services.AddSingleton<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
            
            // Configure IronPdf generator
            services.AddScoped<IPdfGenerator>(sp =>
            {
                var logger = sp.GetService<ILogger<IronPdfGenerator>>();
                var generator = new IronPdfGenerator(logger)
                {
                    PageSize = options.DefaultPageSize,
                    Orientation = options.DefaultOrientation,
                    MarginMm = options.DefaultMarginMm,
                    DocumentTitle = options.DefaultDocumentTitle
                };
                return generator;
            });
            
            // Register invoice generator with configuration
            services.AddSingleton<IInvoiceGenerator>(serviceProvider =>
            {
                var razorRenderer = serviceProvider.GetRequiredService<IRazorViewToStringRenderer>();
                var pdfGenerator = serviceProvider.GetRequiredService<IPdfGenerator>();
                var logger = serviceProvider.GetService<ILogger<InvoicePdfGenerator>>();
                
                var invoiceGenerator = new InvoicePdfGenerator(razorRenderer, pdfGenerator, logger);
                
                // Set custom template path if provided
                if (!string.IsNullOrEmpty(options.CustomTemplatePath))
                {
                    invoiceGenerator.TemplatePath = options.CustomTemplatePath;
                }
                
                return invoiceGenerator;
            });

            return services;
        }
    }
    
    /// <summary>
    /// Options for configuring the InvoiceGenerator services
    /// </summary>
    public class InvoiceGeneratorOptions
    {
        /// <summary>
        /// Common page size constants
        /// </summary>
        public static class PageSizes
        {
            public const string A0 = "A0";
            public const string A1 = "A1";
            public const string A2 = "A2";
            public const string A3 = "A3";
            public const string A4 = "A4";
            public const string A5 = "A5";
            public const string A6 = "A6";
            public const string Letter = "Letter";
            public const string Legal = "Legal";
            public const string Tabloid = "Tabloid";
        }
        
        /// <summary>
        /// Page orientation constants
        /// </summary>
        public static class Orientations
        {
            public const string Portrait = "Portrait";
            public const string Landscape = "Landscape";
        }
        
        /// <summary>
        /// Gets or sets the IronPdf license key
        /// </summary>
        public string IronPdfLicenseKey { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets whether to use appsettings.json for configuration
        /// </summary>
        public bool UseAppSettings { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to reload configuration when appsettings.json changes
        /// </summary>
        public bool ReloadConfigOnChange { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the base path for configuration files
        /// </summary>
        public string ConfigurationBasePath { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the custom template path for invoice generation
        /// </summary>
        public string CustomTemplatePath { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the default page size for generated PDFs
        /// </summary>
        public string DefaultPageSize { get; set; } = PageSizes.A4;
        
        /// <summary>
        /// Gets or sets the default page orientation for generated PDFs
        /// </summary>
        public string DefaultOrientation { get; set; } = Orientations.Portrait;
        
        /// <summary>
        /// Gets or sets the default margins for generated PDFs in millimeters
        /// </summary>
        public int DefaultMarginMm { get; set; } = 20;
        
        /// <summary>
        /// Gets or sets the default document title for generated PDFs
        /// </summary>
        public string DefaultDocumentTitle { get; set; } = string.Empty;
    }
}

# Invoice Generator Library

[![NuGet version](https://img.shields.io/nuget/v/InvoiceGenerator.Core.svg?color=blue&label=NuGet)](https://www.nuget.org/packages/InvoiceGenerator.Core)
[![NuGet downloads](https://img.shields.io/nuget/dt/InvoiceGenerator.Core.svg?label=Downloads)](https://www.nuget.org/packages/InvoiceGenerator.Core)

**Version 2.0.2**


A .NET 8 library for generating Invoice PDF reports from Razor templates using IronPdf. This library is designed to be easy to use, providing a simple API for generating complex PDF documents with text, tables, and more.

## Features

- Generate PDF documents from Razor templates (.cshtml)
- Uses IronPdf for HTML-to-PDF conversion
- Dependency injection support for easy integration with ASP.NET Core applications
- Model-based report generation (strongly typed views)
- Support for custom fields in invoices via a flexible dictionary, allowing for easy addition of custom data to invoices
- Cross-platform support (Windows, macOS, Linux)
- Simple, consolidated API with a single entry point for configuration
- Customizable PDF options including page size, orientation, and margins

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later

### Installation

Clone the repository and build the solution:

```sh
git clone <your-repository-url> # Replace with your repo URL
cd <repository-folder>
dotnet build
```

Alternatively, you can install the library via NuGet:

```powershell
Install-Package InvoiceGenerator.Core
```

## Usage

The primary way to use this library is by leveraging dependency injection.

### Complete Example

Here's a complete example of how to use the library to generate an invoice PDF:

```csharp
using Microsoft.Extensions.DependencyInjection;
using InvoiceGenerator.Core.Contracts;
using InvoiceGenerator.Core.Extensions;
using InvoiceGenerator.Core.Services;
using System.IO;
using System;

// 1️⃣ Setup DI for your PDF services
var services = new ServiceCollection()
                .AddInvoiceGenerator() // registers all required services
                .BuildServiceProvider();

// Get the invoice generator service
var invoiceGenerator = services.GetRequiredService<IInvoiceGenerator>();

// 2️⃣ Create your invoice model
var invoice = new Invoice
{
    Number = "INV-001",
    IssuedDate = DateTime.Today,
    DueDate = DateTime.Today.AddDays(30),
    SellerAddress = new Address
    {
        CompanyName = "ACME Corp",
        Street = "123 Main St",
        City = "Metropolis",
        State = "NY",
        Email = "sales@acme.com",
    },
    CustomerAddress = new Address
    {
        CompanyName = "Wayne Enterprises",
        Street = "456 Oak Ave",
        City = "Gotham",
        State = "NJ",
        Email = "wayne@example.com",
    },
    LineItems =
    [
        new() { Name = "Product A", Price = 10.00m, Quantity = 2 },
        new() { Name = "Product B", Price = 25.50m, Quantity = 1 }
    ]
};

// 3️⃣ Add custom fields to the invoice dynamically
invoice.CustomFields["Purchase Order"] = "PO-12345";
invoice.CustomFields["Salesperson"] = "John Doe";

// 4️⃣ Generate the PDF from the invoice
try {
    byte[] pdfBytes = await invoiceGenerator.GenerateAsync(invoice);
    
    // 5️⃣ Write the PDF out
    var outputPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "TestInvoice.pdf"
    );
    await File.WriteAllBytesAsync(outputPath, pdfBytes);
    
    Console.WriteLine($"✅ PDF written to {outputPath}");
} catch (Exception ex) {
    Console.WriteLine($"❌ Error generating PDF: {ex.Message}");
}
```

### Step-by-Step Usage

#### 1. Service Registration

Register the required services in your `Program.cs` or startup class:

```csharp
using Microsoft.Extensions.DependencyInjection;
using InvoiceGenerator.Core.Extensions;

var services = new ServiceCollection()
    // Basic registration with default options
    .AddInvoiceGenerator()
    .BuildServiceProvider();
    
// Or with custom options
var services = new ServiceCollection()
    .AddInvoiceGenerator(options => {
        // Configure PDF options using string constants
        options.DefaultPageSize = InvoiceGeneratorOptions.PageSizes.A4;
        options.DefaultOrientation = InvoiceGeneratorOptions.Orientations.Portrait;
        options.DefaultMarginMm = 15;
        options.DefaultDocumentTitle = "Invoice Generator";
        
        // Optional: Set a custom template path
        // options.CustomTemplatePath = "CustomInvoiceTemplate";
        
        // Optional: Provide an IronPdf license key
        // options.IronPdfLicenseKey = "YOUR-LICENSE-KEY";
    })
    .BuildServiceProvider();
```

#### 2. PDF Generation

##### Using the InvoicePdfGenerator (Recommended)

The library provides a high-level `IInvoiceGenerator` interface and `InvoicePdfGenerator` implementation that simplifies the PDF generation process:

```csharp
using InvoiceGenerator.Core.Services;
using InvoiceGenerator.Core.Contracts;

// Resolve the IInvoiceGenerator service from the DI container
var invoiceGenerator = services.GetRequiredService<IInvoiceGenerator>();

// 1. Create your invoice model
var invoice = new Invoice
{
    Number = "INV-001",
    IssuedDate = DateTime.Today,
    DueDate = DateTime.Today.AddDays(30),
    SellerAddress = new Address
    {
        CompanyName = "ACME Corp",
        Street = "123 Main St",
        City = "Metropolis",
        State = "NY",
        Email = "sales@acme.com",
    },
    CustomerAddress = new Address
    {
        CompanyName = "Wayne Enterprises",
        Street = "456 Oak Ave",
        City = "Gotham",
        State = "NJ",
        Email = "wayne@example.com",
    },
    LineItems =
    [
        new() { Name = "Product A", Price = 10.00m, Quantity = 2 },
        new() { Name = "Product B", Price = 25.50m, Quantity = 1 }
    ]
};

// 2. Add custom fields to the invoice (optional)
invoice.CustomFields["Purchase Order"] = "PO-12345";
invoice.CustomFields["Salesperson"] = "John Doe";
invoice.CustomFields["Terms"] = "Net 30";

// 3. Generate the PDF directly from the invoice model
byte[] pdfBytes = await invoiceGenerator.GenerateAsync(invoice);

// 4. Save the PDF file
var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "MyInvoice.pdf");
await File.WriteAllBytesAsync(outputPath, pdfBytes);

Console.WriteLine($"PDF generated successfully at {outputPath}");
```

##### Using the Lower-Level Components

Alternatively, you can use the lower-level components directly for more control:

```csharp
using InvoiceGenerator.Core.Services;

// Resolve services from the DI container
var razorRenderer = services.GetRequiredService<IRazorViewToStringRenderer>();
var pdfGenerator = services.GetRequiredService<IPdfGenerator>();

// 1. Create a model for your view
var invoiceModel = new Invoice(); // Your data model

// 2. Render the Razor view to an HTML string
string html = await razorRenderer.RenderViewToStringAsync(
    "Views/InvoiceReport.cshtml", 
    invoiceModel
);

// 3. Generate the PDF from the HTML string
byte[] pdfBytes = pdfGenerator.GeneratePdf(html);

// 4. Save the PDF file
var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "MyInvoice.pdf");
await File.WriteAllBytesAsync(outputPath, pdfBytes);

Console.WriteLine($"PDF generated successfully at {outputPath}");
```

## Project Structure

The InvoiceGenerator.Core library is designed with simplicity in mind, using a consolidated approach for configuration and service registration:

- **ServiceCollectionExtensions.cs**: Contains the main extension methods for registering services and the `InvoiceGeneratorOptions` class with all configuration options
- **Contracts/**: Contains the data models like `Invoice`, `Address`, and `LineItem`
- **Services/**: Contains the service implementations for invoice generation and PDF creation
- **Views/**: Contains the Razor templates used for rendering invoices

## Contributing

Contributions are welcome! If you'd like to contribute, please follow these steps:

1.  Fork the repository on GitHub.
2.  Create a new branch for your feature or bug fix.
3.  Make your changes and commit them with a clear message.
4.  Push your branch to your fork.
5.  Open a pull request to the main repository.

## License

This project is licensed under the MIT License. See the `LICENSE` file for more details.
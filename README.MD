# Invoice Generator Library

A .NET 8 library for generating PDF reports from Razor templates using IronPdf. This library is designed to be easy to use, providing a simple API for generating complex PDF documents with text, images, tables, and more.

## Features

- Generate PDF documents from Razor templates (.cshtml)
- Uses IronPdf for HTML-to-PDF conversion
- Dependency injection support for easy integration with ASP.NET Core applications
- Model-based report generation (strongly typed views)
- Cross-platform support (Windows, macOS, Linux)

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

### 1. Service Registration

Register the required services in your `Program.cs` or startup class:

```csharp
using Microsoft.Extensions.DependencyInjection;
using InvoiceGenerator.Core.Extensions;

var services = new ServiceCollection()
    .AddInvoiceGenerator() // Registers IRazorViewToStringRenderer & IPdfGenerator
    .BuildServiceProvider();
```

### 2. PDF Generation

Inject `IRazorViewToStringRenderer` and `IPdfGenerator` to render a Razor view to HTML and then convert it to a PDF.

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

## Contributing

Contributions are welcome! If you'd like to contribute, please follow these steps:

1.  Fork the repository on GitHub.
2.  Create a new branch for your feature or bug fix.
3.  Make your changes and commit them with a clear message.
4.  Push your branch to your fork.
5.  Open a pull request to the main repository.

## License

This project is licensed under the MIT License. See the `LICENSE` file for more details.
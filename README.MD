# Invoice Generator Library

[![NuGet version](https://img.shields.io/nuget/v/InvoiceGenerator.Core.svg?color=blue&label=NuGet)](https://www.nuget.org/packages/InvoiceGenerator.Core)
[![NuGet downloads](https://img.shields.io/nuget/dt/InvoiceGenerator.Core.svg?label=Downloads)](https://www.nuget.org/packages/InvoiceGenerator.Core)

**Version 2.1.3**

### What's New in 2.1.0

- **ML-Based Layout Optimization**: Automatically select the best invoice template based on invoice characteristics
- **Dynamic Template Selection**: Intelligently choose between Standard, Premium, Compact, or Custom templates
- **Smart Layout Options**: ML predictions for font size, spacing, and emphasis based on invoice content
- **Training Pipeline**: Built-in ML.NET training pipeline with synthetic data generation
- **Extensible Design**: Easy to customize with your own templates and training data

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
git clone https://github.com/sgr-0007/InvoiceGenerator.Core.git 
cd InvoiceGenerator.Core
dotnet restore
dotnet clean
dotnet build
```

Alternatively, you can install the library via NuGet:

```powershell
Install-Package InvoiceGenerator.Core
```

## Usage

The primary way to use this library is by leveraging dependency injection.

### Standard Usage

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

Register the required services in your `Program.cs`:

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

## ML-Based Smart Layout

The library includes ML-based invoice layout optimization features that automatically select the best template and layout options based on invoice characteristics.

### 1. Enable ML Features

Enable ML features during service registration:

```csharp
services.AddInvoiceGenerator(options => {
    options.UseSmartLayout = true; // Enable ML-based layout optimization
    options.ConfigurationBasePath = Directory.GetCurrentDirectory();
    
    // Optional: Specify paths to custom ML model and training data
    // options.MlModelPath = "path/to/model.zip";
    // options.TrainingDataPath = "path/to/training_data.csv";
});
```

### 2. Generate Invoices with Smart Layout

Once ML features are enabled, the invoice generator will automatically:
1. Analyze invoice characteristics (line items count, total amount, etc.)
2. Select the optimal template (Standard, Premium, Compact, or Custom)
3. Adjust layout parameters (font size, spacing, emphasis)

#### How Layout Selection Works

The ML system selects layouts based on these invoice characteristics:

- **Line Item Count**: Invoices with many line items (>20) typically use the Compact template for better space utilization
- **Total Amount**: High-value invoices (>$1000) often use the Premium template with enhanced styling
- **Custom Fields**: Invoices with many custom fields may use templates with dedicated sections
- **Content Length**: Text-heavy invoices use layouts with optimized spacing

The multiclass classification model outputs confidence scores for each template type, and the system selects the template with the highest score. Layout parameters are then fine-tuned based on the specific invoice characteristics:

- **Font Size**: Adjusted based on content density
- **Section Spacing**: Optimized for readability
- **Total Amount Emphasis**: Enhanced for high-value invoices
- **Layout Template**: Selected from Standard, Premium, Compact, or Custom

```csharp
// Create different types of invoices
var standardInvoice = new Invoice { /* few line items */ };
var compactInvoice = new Invoice { /* many line items */ };
var premiumInvoice = new Invoice { /* high value */ };

// The library automatically selects the best template for each invoice
byte[] pdfBytes = await invoiceGenerator.GenerateAsync(invoice);
```

### 3. Training Custom ML Models

You can train custom ML models with your own data:

```csharp
// Get the ML optimizer service
var layoutOptimizer = services.GetRequiredService<IInvoiceLayoutOptimizer>();

// Generate synthetic training data (or use your own)
var trainingDataPath = "path/to/training_data.csv";
var trainingGenerator = new TrainingDataGenerator();
trainingGenerator.GenerateTrainingData(trainingDataPath, 500);

// Train the model
var modelPath = "path/to/save/model.zip";
if (layoutOptimizer.TrainModel(trainingDataPath))
{
    // Save the trained model
    layoutOptimizer.SaveModel(modelPath);
}

// Load a previously trained model
layoutOptimizer.LoadModel(modelPath);
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
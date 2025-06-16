# PDF Reporting Library

A .NET 8 library for generating PDF reports from Razor templates using IronPdf.

## Project Structure

This solution consists of two projects:

- **PdfReporting.Core**: Core library containing the PDF generation logic and Razor templating services
- **PdfReporting.Test**: Console application that demonstrates how to use the library

## Features

- Generate PDF documents from Razor templates (.cshtml)
- Uses IronPdf for HTML-to-PDF conversion
- Dependency injection support for easy integration with ASP.NET Core applications
- Model-based report generation (strongly typed views)
- Cross-platform support (Windows, macOS, Linux)

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- NuGet package manager

### Installation

1. Clone this repository
2. Build the solution:

```sh
dotnet build
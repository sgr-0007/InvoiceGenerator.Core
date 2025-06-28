using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using InvoiceGenerator.Core.ML.Models;
using Microsoft.Extensions.Logging;

namespace InvoiceGenerator.Core.ML.Utils
{
    /// <summary>
    /// Utility class for generating training data for invoice layout optimization
    /// </summary>
    public class TrainingDataGenerator
    {
        private readonly ILogger<TrainingDataGenerator>? _logger;
        private readonly Random _random = new Random(42); // Fixed seed for reproducibility

        /// <summary>
        /// Initializes a new instance of the TrainingDataGenerator class
        /// </summary>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public TrainingDataGenerator(ILogger<TrainingDataGenerator>? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Generates sample training data for invoice layout optimization
        /// </summary>
        /// <param name="outputPath">Path where the CSV file should be saved</param>
        /// <param name="sampleCount">Number of samples to generate</param>
        /// <returns>True if the data was generated successfully, false otherwise</returns>
        public bool GenerateTrainingData(string outputPath, int sampleCount = 1000)
        {
            try
            {
                _logger?.LogInformation($"Generating {sampleCount} training samples to {outputPath}");
                
                // Create directory if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);
                
                using var writer = new StreamWriter(outputPath, false, Encoding.UTF8);
                
                // Write CSV header
                writer.WriteLine("LineItemCount,InvoiceTotal,CustomFieldCount,NotesLength,IsBusinessCustomer,CurrencyCode,FontSize,SectionSpacing,TotalEmphasis,LayoutTemplate,Score");
                
                // Generate sample data
                for (int i = 0; i < sampleCount; i++)
                {
                    var sample = GenerateSample();
                    writer.WriteLine(FormatSampleAsCsv(sample));
                }
                
                _logger?.LogInformation($"Successfully generated {sampleCount} training samples");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error generating training data to {outputPath}");
                return false;
            }
        }
        
        /// <summary>
        /// Generates a single training sample
        /// </summary>
        /// <returns>A tuple containing the input features and output values</returns>
        private (InvoiceLayoutData Features, InvoiceLayoutPrediction Prediction) GenerateSample()
        {
            // Generate input features
            var features = new InvoiceLayoutData
            {
                LineItemCount = _random.Next(1, 50),
                InvoiceTotal = (float)(_random.NextDouble() * 10000),
                CustomFieldCount = _random.Next(0, 10),
                NotesLength = _random.Next(0, 500),
                IsBusinessCustomer = _random.NextDouble() > 0.3, // 70% business customers
                CurrencyCode = GetRandomCurrency()
            };
            
            // Generate output values based on input features
            var prediction = new InvoiceLayoutPrediction
            {
                // For many line items, use smaller font size
                FontSize = features.LineItemCount > 20 ? 
                    (float)(9 + _random.NextDouble() * 2) : // 9-11 for many items
                    (float)(11 + _random.NextDouble() * 3), // 11-14 for fewer items
                
                // More spacing for fewer items
                SectionSpacing = features.LineItemCount > 15 ?
                    (float)(1.0 + _random.NextDouble() * 0.5) : // 1.0-1.5 for many items
                    (float)(1.5 + _random.NextDouble() * 1.0), // 1.5-2.5 for fewer items
                
                // Higher emphasis for larger invoice totals
                TotalEmphasis = features.InvoiceTotal > 5000 ?
                    (float)(0.7 + _random.NextDouble() * 0.3) : // 0.7-1.0 for large amounts
                    (float)(0.5 + _random.NextDouble() * 0.3), // 0.5-0.8 for smaller amounts
                
                // Select template based on features
                LayoutTemplate = SelectTemplate(features),
                
                // Random confidence scores as array (in real model this would be calculated)
                Score = [ 
                    (float)(0.7 + _random.NextDouble() * 0.3), // 0.7-1.0 for primary score
                    (float)(0.2 + _random.NextDouble() * 0.3), // 0.2-0.5 for secondary score
                    (float)(0.1 + _random.NextDouble() * 0.2)  // 0.1-0.3 for tertiary score
                ]
            };
            
            return (features, prediction);
        }
        
        /// <summary>
        /// Formats a sample as a CSV line
        /// </summary>
        /// <param name="sample">The sample to format</param>
        /// <returns>A CSV formatted string</returns>
        private string FormatSampleAsCsv((InvoiceLayoutData Features, InvoiceLayoutPrediction Prediction) sample)
        {
            return $"{sample.Features.LineItemCount}," +
                   $"{sample.Features.InvoiceTotal}," +
                   $"{sample.Features.CustomFieldCount}," +
                   $"{sample.Features.NotesLength}," +
                   $"{(sample.Features.IsBusinessCustomer ? "true" : "false")}," +
                   $"{sample.Features.CurrencyCode}," +
                   $"{sample.Prediction.FontSize}," +
                   $"{sample.Prediction.SectionSpacing}," +
                   $"{sample.Prediction.TotalEmphasis}," +
                   $"{sample.Prediction.LayoutTemplate}," +
                   $"{sample.Prediction.Score}";
        }
        
        /// <summary>
        /// Gets a random currency code
        /// </summary>
        /// <returns>A random currency code</returns>
        private string GetRandomCurrency()
        {
            string[] currencies = { "USD", "EUR", "GBP", "JPY", "CAD", "AUD", "CHF", "CNY", "INR" };
            return currencies[_random.Next(currencies.Length)];
        }
        
        /// <summary>
        /// Selects a template based on invoice features
        /// </summary>
        /// <param name="features">The invoice features</param>
        /// <returns>The selected template name</returns>
        private string SelectTemplate(InvoiceLayoutData features)
        {
            // Business logic for template selection
            if (features.LineItemCount > 30)
            {
                return "Compact"; // Compact template for many line items
            }
            
            if (features.InvoiceTotal > 7500)
            {
                return "Premium"; // Premium template for high-value invoices
            }
            
            // Default template
            return "Standard";
        }
    }
}

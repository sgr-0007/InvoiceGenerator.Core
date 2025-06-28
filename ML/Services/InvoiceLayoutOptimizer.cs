using InvoiceGenerator.Core.Contracts;
using InvoiceGenerator.Core.ML.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

#nullable enable

namespace InvoiceGenerator.Core.ML.Services
{
    /// <summary>
    /// Implementation of IInvoiceLayoutOptimizer that uses ML.NET to optimize invoice layouts
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the InvoiceLayoutOptimizer class
    /// </remarks>
    /// <param name="logger">Optional logger for diagnostic information</param>
    public class InvoiceLayoutOptimizer(ILogger<InvoiceLayoutOptimizer>? logger = null) : IInvoiceLayoutOptimizer
    {
        private readonly MLContext _mlContext = new(seed: 42);
        private ITransformer? _trainedModel;
        private readonly ILogger<InvoiceLayoutOptimizer>? _logger = logger;

        /// <inheritdoc/>
        public InvoiceLayoutPrediction PredictOptimalLayout(Invoice invoice)
        {
            if (_trainedModel == null)
            {
                _logger?.LogWarning("Attempting to predict without a trained model. Using default values.");
                return GetDefaultPrediction();
            }
            
            try
            {
                // Convert invoice to feature data
                var inputData = ConvertInvoiceToFeatures(invoice);
                
                // Create prediction engine
                var predictionEngine = _mlContext.Model.CreatePredictionEngine<InvoiceLayoutData, InvoiceLayoutPrediction>(_trainedModel);
                
                // Make prediction
                var prediction = predictionEngine.Predict(inputData);
                
                _logger?.LogInformation($"Generated layout prediction for invoice {invoice.Number} with confidence {prediction.ScoreValue:P2}");
                
                return prediction;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error predicting layout for invoice {invoice.Number}");
                return GetDefaultPrediction();
            }
        }
        
        /// <inheritdoc/>
        public bool TrainModel(string trainingDataPath)
        {
            if (!File.Exists(trainingDataPath))
            {
                _logger?.LogError($"Training data file not found: {trainingDataPath}");
                return false;
            }
            
            try
            {
                // Load data
                IDataView trainingData = _mlContext.Data.LoadFromTextFile<InvoiceLayoutData>(
                    trainingDataPath,
                    hasHeader: true,
                    separatorChar: ',');
                
                // Define data processing pipeline
                var dataProcessPipeline = _mlContext.Transforms.Concatenate(
                    "Features",
                    nameof(InvoiceLayoutData.LineItemCount),
                    nameof(InvoiceLayoutData.InvoiceTotal),
                    nameof(InvoiceLayoutData.CustomFieldCount),
                    nameof(InvoiceLayoutData.NotesLength))
                    .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                    .Append(_mlContext.Transforms.Categorical.OneHotEncoding(
                        outputColumnName: "CurrencyCodeEncoded",
                        inputColumnName: nameof(InvoiceLayoutData.CurrencyCode)));
                
                // Map the LayoutTemplate column to the label column
                var mappingPipeline = _mlContext.Transforms.Conversion.MapValueToKey(
                    outputColumnName: "Label",
                    inputColumnName: nameof(InvoiceLayoutData.LayoutTemplate));
                
                // Define multi-class classification trainer
                var trainer = _mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                    labelColumnName: "Label",
                    featureColumnName: "Features");
                
                // Create training pipeline
                var trainingPipeline = dataProcessPipeline
                    .Append(mappingPipeline)
                    .Append(trainer)
                    .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLayoutTemplate", "PredictedLabel"));
                
                // Train model
                _trainedModel = trainingPipeline.Fit(trainingData);
                
                _logger?.LogInformation("Model training completed successfully");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error training model");
                return false;
            }
        }
        
        /// <inheritdoc/>
        public bool SaveModel(string modelPath)
        {
            if (_trainedModel == null)
            {
                _logger?.LogError("Cannot save model: No trained model available");
                return false;
            }
            
            try
            {
                // Create directory if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(modelPath) ?? string.Empty);
                
                // Save model
                _mlContext.Model.Save(_trainedModel, null, modelPath);
                
                _logger?.LogInformation($"Model saved to {modelPath}");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error saving model to {modelPath}");
                return false;
            }
        }
        
        /// <inheritdoc/>
        public bool LoadModel(string modelPath)
        {
            if (!File.Exists(modelPath))
            {
                _logger?.LogError($"Model file not found: {modelPath}");
                return false;
            }
            
            try
            {
                // Load model
                _trainedModel = _mlContext.Model.Load(modelPath, out _);
                
                _logger?.LogInformation($"Model loaded from {modelPath}");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error loading model from {modelPath}");
                return false;
            }
        }
        
        /// <summary>
        /// Converts an invoice to feature data for ML prediction
        /// </summary>
        /// <param name="invoice">The invoice to convert</param>
        /// <returns>Feature data for ML prediction</returns>
        private InvoiceLayoutData ConvertInvoiceToFeatures(Invoice invoice)
        {
            return new InvoiceLayoutData
            {
                LineItemCount = invoice.LineItems.Count,
                InvoiceTotal = (float)invoice.Total,
                CustomFieldCount = invoice.CustomFields.Count,
                NotesLength = invoice.Notes?.Length ?? 0,
                IsBusinessCustomer = !string.IsNullOrEmpty(invoice.CustomerAddress.CompanyName),
                CurrencyCode = invoice.CurrencyCode
            };
        }
        
        /// <summary>
        /// Returns a default prediction when no model is available
        /// </summary>
        /// <returns>Default layout prediction</returns>
        private InvoiceLayoutPrediction GetDefaultPrediction()
        {
            return new InvoiceLayoutPrediction
            {
                FontSize = 12.0f,
                SectionSpacing = 1.5f,
                TotalEmphasis = 0.8f,
                LayoutTemplate = "Standard",
                Score = new float[] { 0.5f, 0.3f, 0.2f }
            };
        }
    }
}

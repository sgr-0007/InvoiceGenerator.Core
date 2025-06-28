using InvoiceGenerator.Core.Contracts;
using InvoiceGenerator.Core.ML.Models;

namespace InvoiceGenerator.Core.ML.Services
{
    /// <summary>
    /// Service for optimizing invoice layouts using machine learning
    /// </summary>
    public interface IInvoiceLayoutOptimizer
    {
        /// <summary>
        /// Predicts the optimal layout parameters for an invoice
        /// </summary>
        /// <param name="invoice">The invoice to optimize</param>
        /// <returns>Layout prediction with optimal parameters</returns>
        InvoiceLayoutPrediction PredictOptimalLayout(Invoice invoice);
        
        /// <summary>
        /// Trains the layout optimization model with sample data
        /// </summary>
        /// <param name="trainingDataPath">Path to the training data CSV file</param>
        /// <returns>True if training was successful, false otherwise</returns>
        bool TrainModel(string trainingDataPath);
        
        /// <summary>
        /// Saves the trained model to the specified path
        /// </summary>
        /// <param name="modelPath">Path where the model should be saved</param>
        /// <returns>True if the model was saved successfully, false otherwise</returns>
        bool SaveModel(string modelPath);
        
        /// <summary>
        /// Loads a trained model from the specified path
        /// </summary>
        /// <param name="modelPath">Path to the trained model file</param>
        /// <returns>True if the model was loaded successfully, false otherwise</returns>
        bool LoadModel(string modelPath);
    }
}

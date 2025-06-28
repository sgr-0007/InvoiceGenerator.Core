using Microsoft.ML.Data;

namespace InvoiceGenerator.Core.ML.Models
{
    /// <summary>
    /// Represents the output prediction for invoice layout optimization
    /// </summary>
    public class InvoiceLayoutPrediction
    {
        /// <summary>
        /// Predicted optimal font size for the invoice
        /// </summary>
        [ColumnName("FontSize")]
        public float FontSize { get; set; }
        
        /// <summary>
        /// Predicted optimal spacing between sections
        /// </summary>
        [ColumnName("SectionSpacing")]
        public float SectionSpacing { get; set; }
        
        /// <summary>
        /// Predicted optimal emphasis level for the total amount (0-1)
        /// </summary>
        [ColumnName("TotalEmphasis")]
        public float TotalEmphasis { get; set; }
        
        /// <summary>
        /// Predicted optimal layout template to use
        /// </summary>
        [ColumnName("LayoutTemplate")]
        public string LayoutTemplate { get; set; } = "Standard";
        
        /// <summary>
        /// Confidence score for the prediction (0-1)
        /// </summary>
        [ColumnName("Score")]
        public float[] Score { get; set; }
        
        /// <summary>
        /// Gets the confidence score as a single value (for compatibility)
        /// </summary>
        public float ScoreValue => Score != null && Score.Length > 0 ? Score[0] : 0f;
    }
}

namespace InvoiceGenerator.Core.ML.Models
{
    /// <summary>
    /// Represents layout options for invoice generation that can be optimized by ML
    /// </summary>
    public class InvoiceLayoutOptions
    {
        /// <summary>
        /// Gets or sets the font size for the main content
        /// </summary>
        public float FontSize { get; set; } = 12.0f;
        
        /// <summary>
        /// Gets or sets the spacing between sections in the invoice
        /// </summary>
        public float SectionSpacing { get; set; } = 1.5f;
        
        /// <summary>
        /// Gets or sets the emphasis level for the total amount (0-1)
        /// </summary>
        public float TotalEmphasis { get; set; } = 0.8f;
        
        /// <summary>
        /// Gets or sets the layout template to use
        /// </summary>
        public string LayoutTemplate { get; set; } = "Standard";
        
        /// <summary>
        /// Gets or sets whether to use compact layout for many line items
        /// </summary>
        public bool UseCompactLayout { get; set; }
        
        /// <summary>
        /// Gets or sets whether to highlight important fields
        /// </summary>
        public bool HighlightImportantFields { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the color scheme to use
        /// </summary>
        public string ColorScheme { get; set; } = "Default";
        
        /// <summary>
        /// Creates layout options from an ML prediction
        /// </summary>
        /// <param name="prediction">The ML prediction to convert</param>
        /// <returns>Layout options based on the prediction</returns>
        public static InvoiceLayoutOptions FromPrediction(InvoiceLayoutPrediction prediction)
        {
            return new InvoiceLayoutOptions
            {
                FontSize = prediction.FontSize,
                SectionSpacing = prediction.SectionSpacing,
                TotalEmphasis = prediction.TotalEmphasis,
                LayoutTemplate = prediction.LayoutTemplate,
                UseCompactLayout = prediction.FontSize < 11.0f, // Infer compact layout from small font size
                HighlightImportantFields = prediction.TotalEmphasis > 0.7f
            };
        }
    }
}

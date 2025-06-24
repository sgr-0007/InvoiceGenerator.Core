using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace InvoiceGenerator.Core.Contracts
{
    /// <summary>
    /// Represents a physical address and contact information
    /// </summary>
    public class Address
    {
        /// <summary>
        /// Gets or sets the company or organization name
        /// </summary>
        [Required(ErrorMessage = "Company name is required")]
        public required string CompanyName { get; set; }
        
        /// <summary>
        /// Gets or sets the street address
        /// </summary>
        [Required(ErrorMessage = "Street address is required")]
        public required string Street { get; set; }
        
        /// <summary>
        /// Gets or sets the city
        /// </summary>
        [Required(ErrorMessage = "City is required")]
        public required string City { get; set; }
        
        /// <summary>
        /// Gets or sets the state or province
        /// </summary>
        [Required(ErrorMessage = "State is required")]
        public required string State { get; set; }
        
        /// <summary>
        /// Gets or sets the email address
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public required string Email { get; set; }
        
        /// <summary>
        /// Gets or sets the postal code/zip code
        /// </summary>
        public string? PostalCode { get; set; }
        
        /// <summary>
        /// Gets or sets the phone number
        /// </summary>
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? Phone { get; set; }
    }
}
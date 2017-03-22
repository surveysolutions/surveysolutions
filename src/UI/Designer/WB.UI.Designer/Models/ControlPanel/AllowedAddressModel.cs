using System.ComponentModel.DataAnnotations;
using System.Net;

namespace WB.UI.Designer.Models.ControlPanel
{
    public class AllowedAddressModel
    {
        public int Id { get; set; }
        public string Description { get; set; }

        [Required]
        [StringLength(15)]
        [IpAddress]
        public string Address { get; set; }
    }

    public class IpAddressAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var stringValue = value as string;

            if (string.IsNullOrWhiteSpace(stringValue)) { return ValidationResult.Success; }

            IPAddress address;
            if (!IPAddress.TryParse(stringValue, out address))
            {
                return new ValidationResult("IP address cannot be parsed.");
            }

            return ValidationResult.Success;
        }
    }
}
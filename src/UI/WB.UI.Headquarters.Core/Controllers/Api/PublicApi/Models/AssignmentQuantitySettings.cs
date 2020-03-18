using System.ComponentModel.DataAnnotations;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class AssignmentQuantitySettings
    {
        /// <summary>
        /// Assignment Quantity Settings.
        /// </summary>
        [Required]
        public bool CanChangeQuantity { get; set; }
    }
}

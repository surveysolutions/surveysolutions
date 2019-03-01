using System.Runtime.Serialization;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class AssignmentQuantitySettings
    {
        /// <summary>
        /// Assignment Quantity Settings.
        /// </summary>
        [DataMember(IsRequired = true)]
        public bool CanChangeQuantity { get; set; }
    }
}

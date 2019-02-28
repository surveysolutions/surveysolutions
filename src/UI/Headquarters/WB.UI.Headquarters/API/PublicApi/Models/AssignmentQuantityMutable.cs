using System.Runtime.Serialization;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class AssignmentQuantityMutable
    {
        /// <summary>
        /// Is Quantity Mutable.
        /// </summary>
        [DataMember(IsRequired = true)]
        public bool CanChangeQuantity { get; set; }
    }
}

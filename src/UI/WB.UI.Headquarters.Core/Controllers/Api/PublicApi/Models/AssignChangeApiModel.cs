using System;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    /// <summary>
    /// Provide either responsible id or responsible name
    /// </summary>
    public class AssignChangeApiModel
    {
        /// <summary>
        /// New responsible id
        /// </summary>
        public Guid? ResponsibleId { set; get; }
        /// <summary>
        /// New responsible name
        /// </summary>
        public string ResponsibleName { set; get; }
    }
}

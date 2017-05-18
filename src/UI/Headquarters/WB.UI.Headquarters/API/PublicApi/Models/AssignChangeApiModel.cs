using System;
using System.ComponentModel.DataAnnotations;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class AssignChangeApiModel
    {
        /// <summary>
        /// Id of the interview to reassign
        /// </summary>
        [Required]
        public Guid Id { set; get; }
        public Guid? ResponsibleId { set; get; }
        public string ResponsibleName { set; get; }
    }
}

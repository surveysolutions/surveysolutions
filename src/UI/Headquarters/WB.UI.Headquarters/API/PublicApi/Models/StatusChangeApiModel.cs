using System;
using System.ComponentModel.DataAnnotations;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class StatusChangeApiModel
    {
        /// <summary>
        /// Id of the interview to reassign
        /// </summary>
        [Required]
        public Guid Id { set; get; }
        public string Comment { set; get; }
    }
}
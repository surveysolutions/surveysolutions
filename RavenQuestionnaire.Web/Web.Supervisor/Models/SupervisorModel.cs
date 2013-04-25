using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Web.Supervisor.Models
{
    public class SupervisorModel
    {
        public SupervisorModel() { }

        public SupervisorModel(Guid id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        [Required]
        [Display(Name = "Supervisor id")]
        public Guid Id { get; set; }

        
        [Required]
        [Display(Name = "Supervisor login and password")]
        public string Name { get; set; }
    }
}
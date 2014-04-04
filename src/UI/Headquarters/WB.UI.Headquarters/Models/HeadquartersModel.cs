using System;
using System.ComponentModel.DataAnnotations;

namespace WB.UI.Headquarters.Models
{
    public class HeadquartersModel
    {
        public HeadquartersModel() { }

        public HeadquartersModel(Guid id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        [Required]
        [Display(Name = "Headquarters id")]
        public Guid Id { get; set; }

        
        [Required]
        [Display(Name = "Headquarters login name")]
        public string Name { get; set; }

    }
}
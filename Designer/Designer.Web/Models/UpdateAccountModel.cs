﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Designer.Web.Models
{
    public class UpdateAccountModel
    {
        [Required]
        [Display(Name = "User name")]
        [Editable(false, AllowInitialValue = true)]
        public string UserName { get; set; }
        
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Is Approved")]
        public bool IsApproved { get; set; }

        [Display(Name = "Is Locked Out")]
        public bool IsLockedOut { get; set; }

        [Display(Name = "Comment")]
        [DataType(DataType.MultilineText)]
        public string Comment { get; set; }
    }
}
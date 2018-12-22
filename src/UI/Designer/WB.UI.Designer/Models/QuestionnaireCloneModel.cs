﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Models
{
    public class QuestionnaireCloneModel 
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "QuestionnaireTitle_required")]
        [AllowHtml]
        public string Title { get; set; }
    }
}

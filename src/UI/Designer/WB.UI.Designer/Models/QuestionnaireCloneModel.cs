using System;
using System.ComponentModel.DataAnnotations;

namespace WB.UI.Designer.Models
{
    public class QuestionnaireCloneModel : QuestionnaireViewModel
    {
        [Key]
        public Guid Id { get; set; }
    }
}
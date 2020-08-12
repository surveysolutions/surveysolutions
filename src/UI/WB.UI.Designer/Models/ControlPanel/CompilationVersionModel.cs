using System;
using System.ComponentModel.DataAnnotations;

namespace WB.UI.Designer.Models.ControlPanel
{
    public class CompilationVersionModel
    {
        public Guid QuestionnaireId { get; set; }
        public string? Description { get; set; }

        [Required]
        [Range(17, 19)]
        public int Version { get; set; }
    }
}

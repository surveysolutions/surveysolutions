using System;
using System.ComponentModel.DataAnnotations;

namespace WB.UI.Designer.Models
{
    public class AddCommentModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid EntityId { get; set; }

        [Required]
        public Guid QuestionnaireId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Comment { get; set; }
    }
}

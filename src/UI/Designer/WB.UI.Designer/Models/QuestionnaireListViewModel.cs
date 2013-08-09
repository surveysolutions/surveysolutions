using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WB.UI.Designer.Models
{
    [DisplayName("questionnaire")]
    public class QuestionnaireListViewModel : ActionItem
    {
        public override bool CanPreview
        {
            get
            {
                return false;
            }
        }

        [Display(Name = "Title", Order = 1)]
        [Default]
        public string Title { get; set; }

        [Display(Name = "Creation Date", Order = 2)]
        public DateTime CreationDate { get; set; }

        [Key]
        public Guid Id { get; set; }

        [Display(Name = "Last Entry Date", Order = 3)]
        public DateTime LastEntryDate { get; set; }

        [Display(Name = "Deleted?", Order = 4)]
        [OnlyForAdmin]
        public bool IsDeleted { get; set; }

        [Display(Name = "Public?", Order = 5)]
        [OnlyForAdmin]
        public bool IsPublic { get; set; }
    }
}
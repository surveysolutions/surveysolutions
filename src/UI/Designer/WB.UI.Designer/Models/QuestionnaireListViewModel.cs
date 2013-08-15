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

        [Display(Name = "Shared?", Order = 1)]
        public bool IsShared { get; set; }

        [Display(Name = "Title", Order = 2)]
        [Default]
        public string Title { get; set; }

        [Display(Name = "Creation Date", Order = 3)]
        public DateTime CreationDate { get; set; }

        [Key]
        public Guid Id { get; set; }

        [Display(Name = "Last Entry Date", Order = 4)]
        public DateTime LastEntryDate { get; set; }

        [Display(Name = "Deleted?", Order = 5)]
        [OnlyForAdmin]
        public bool IsDeleted { get; set; }

        [Display(Name = "Public?", Order = 6)]
        [OnlyForAdmin]
        public bool IsPublic { get; set; }
    }
}
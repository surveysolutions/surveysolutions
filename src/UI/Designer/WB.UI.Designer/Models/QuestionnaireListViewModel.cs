using System;
using System.ComponentModel.DataAnnotations;

namespace WB.UI.Designer.Models
{
    public class QuestionnaireListViewModel : ActionItem
    {
        public override bool CanPreview
        {
            get
            {
                return false;
            }
        }

        [Key]
        public Guid Id { get; set; }

        [Display(Name = "Title", Order = 1)]
        [Default]
        public string Title { get; set; }

        [Display(Name = "Last Entry Date", Order = 2)]
        public DateTime LastEntryDate { get; set; }

        [Display(Name = "Created by", Order = 3)]
        public string Owner { get; set; }

        [Display(Name = "Creation Date", Order = 4)]
        public DateTime CreationDate { get; set; }

        [Display(Name = "Deleted?", Order = 5)]
        [OnlyForAdmin]
        public bool IsDeleted { get; set; }

        [Display(Name = "Public?", Order = 6)]
        [OnlyForAdmin]
        public bool IsPublic { get; set; }
    }
}
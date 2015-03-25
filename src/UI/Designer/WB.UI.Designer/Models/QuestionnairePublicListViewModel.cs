using System;

namespace WB.UI.Designer.Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    [DisplayName("questionnaire")]
    public class QuestionnairePublicListViewModel : ActionItem
    {
        [Display(Name = "Title", Order = 1)]
        [Default]
        public string Title { get; set; }

        [Display(Name = "Created by", Order = 2)]
        public string CreatorName { get; set; }

        [Display(Name = "Creation Date", Order = 3)]
        public DateTime CreationDate { get; set; }

        [Key]
        public Guid Id { get; set; }

        [Display(Name = "Last Entry Date", Order = 4)]
        public DateTime LastEntryDate { get; set; }

        [Display(Name = "Deleted?", Order = 5)]
        [OnlyForAdmin]
        public bool IsDeleted { get; set; }
    }
}
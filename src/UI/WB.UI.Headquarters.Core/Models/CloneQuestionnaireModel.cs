using System;
using System.ComponentModel.DataAnnotations;
using WB.UI.Headquarters.Resources;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class CloneQuestionnaireModel
    {
        public CloneQuestionnaireModel() {}

        public CloneQuestionnaireModel(Guid id, long version, string title, bool isCensus, string comment)
        {
            this.Id = id;
            this.Version = version;
            this.IsCensus = isCensus;
            this.OriginalTitle = title;
            this.NewTitle = title;
            this.Comment = comment;
        }

        public Guid Id { get; set; }
        public long Version { get; set; }
        public bool IsCensus { get; set; }
        public string OriginalTitle { get; set; }

        [Display(ResourceType = typeof(FieldsAndValidations), Name = nameof(FieldsAndValidations.CloneQuestionnaireModel_NewTitle_Label))]
        [Required(ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessageResourceName = nameof(FieldsAndValidations.CloneQuestionnaireModel_NewTitle_Error_Required))]
        public string NewTitle { get; set; }

        [Display(ResourceType = typeof(Assignments), Name = nameof(Assignments.DetailsComments))]
        public string Comment { get; set; }

        public string Error { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using WB.Core.SharedKernels.SurveyManagement.Web.Properties;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class CloneQuestionnaireModel
    {
        public CloneQuestionnaireModel() {}

        public CloneQuestionnaireModel(Guid id, long version, string title, bool isCensus)
        {
            this.Id = id;
            this.Version = version;
            this.IsCensus = isCensus;
            this.OriginalTitle = title;
            this.NewTitle = title;
        }

        public Guid Id { get; set; }
        public long Version { get; set; }
        public bool IsCensus { get; set; }
        public string OriginalTitle { get; set; }

        [Display(ResourceType = typeof(FieldsAndValidations), Name = nameof(FieldsAndValidations.CloneQuestionnaireModel_NewTitle_Label))]
        [Required(ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessageResourceName = nameof(FieldsAndValidations.CloneQuestionnaireModel_NewTitle_Error_Required))]
        public string NewTitle { get; set; }
    }
}
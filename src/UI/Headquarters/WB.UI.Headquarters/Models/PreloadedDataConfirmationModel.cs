using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Resources;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;

namespace WB.UI.Headquarters.Models
{
    public class PreloadedDataQuestionnaireModel
    {
        public Guid Id { get; set; }

        public long Version { get; set; }

        public string Title { get; set; }
    }

    public class PreloadedDataInProgressModel
    {
        public PreloadedDataQuestionnaireModel Questionnaire { get; set; }
        public AssignmentsImportProcessStatus? ProcessStatus { get; set; }
    }

    public class PreloadedDataConfirmationModel : IValidatableObject
    {
        public Guid QuestionnaireId { get; set; }

        public long Version { get; set; }

        public string QuestionnaireTitle { get; set; }

        public string FileName { get; set; }

        public Guid? ResponsibleId { get; set; }

        public bool WasResponsibleProvided { get; set; }

        public long EntitiesCount { get; set; }
        public int EnumeratorsCount { get; set; }
        public int SupervisorsCount { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!this.WasResponsibleProvided && !this.ResponsibleId.HasValue)
            {
                yield return new ValidationResult(BatchUpload.SupervisorMustBeSelected, new[] { nameof(this.ResponsibleId) });
            }
        }
    }
}

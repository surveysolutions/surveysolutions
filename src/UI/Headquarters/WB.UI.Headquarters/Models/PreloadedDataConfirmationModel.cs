using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Resources;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;

namespace WB.UI.Headquarters.Models
{
    public class PreloadedDataQuestionnaireModel
    {
        public Guid QuestionnaireId { get; set; }

        public long Version { get; set; }

        public string QuestionnaireTitle { get; set; }
    }

    public class PreloadedDataConfirmationModel : IValidatableObject
    {
        public string Id { get; set; }

        public PreloadedContentType PreloadedContentType { get; set; }

        public Guid QuestionnaireId { get; set; }

        public long Version { get; set; }

        public string QuestionnaireTitle { get; set; }

        public string FileName { get; set; }

        public Guid? SupervisorId { get; set; }

        public bool WasSupervsorProvided { get; set; }

        public int InterviewsCount { get; set; }
        public int EnumeratorsCount { get; set; }
        public int SupervisorsCount { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!this.WasSupervsorProvided && !this.SupervisorId.HasValue)
            {
                yield return new ValidationResult(BatchUpload.SupervisorMustBeSelected, new[] { nameof(this.SupervisorId) });
            }
        }
    }
}
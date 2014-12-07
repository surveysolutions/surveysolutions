using System;
using AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Core.Model.ViewModel.Dashboard
{
    public class QuestionnaireDTO : DenormalizerRow
    {
        public QuestionnaireDTO(Guid id, Guid responsible, Guid survey, InterviewStatus status, FeaturedItem[] prefilledQuestions,
            long surveyVersion, string comments, bool? createdOnClient = false, bool justInitilized = false)
        {
            this.Id = id.FormatGuid();
            this.Status = status;
            this.Responsible = responsible.FormatGuid();
            this.Survey = survey.FormatGuid();
            this.PrefilledQuestions = prefilledQuestions ?? new FeaturedItem[0];
            this.CreatedOnClient = createdOnClient;
            this.JustInitilized = justInitilized;
            this.SurveyVersion = surveyVersion;
            this.Comments = comments;
        }

        public QuestionnaireDTO() {}

        public InterviewStatus Status { get; set; }
        public string Responsible { get; set; }
        public string Survey { get; set; }

        public FeaturedItem[] PrefilledQuestions { get; set; }

        public string Comments { get; set; }
        public bool Valid { get; set; }

        public bool? JustInitilized { get; set; }
        public bool? CreatedOnClient { get; set; }
        public long SurveyVersion { get; set; }
    }
}
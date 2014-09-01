using System;
using System.Collections.Generic;
using System.Linq;
using AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage;
using WB.Core.BoundedContexts.Capi.ModelUtils;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Core.Model.ViewModel.Dashboard
{
    public class QuestionnaireDTO : DenormalizerRow
    {
        public QuestionnaireDTO(Guid id, Guid responsible, Guid survey, InterviewStatus status, IList<FeaturedItem> properties,
            long surveyVersion, string comments, bool? createdOnClient = false, bool justInitilized = false)
        {
            this.Id = id.FormatGuid();
            this.Status = (int) status;
            this.Responsible = responsible.FormatGuid();
            this.Survey = survey.FormatGuid();
            this.SetProperties(properties);
            this.CreatedOnClient = createdOnClient;
            this.JustInitilized = justInitilized;
            this.SurveyVersion = surveyVersion;
            this.Comments = comments;
        }

        public QuestionnaireDTO() {}

        public int Status { get; set; }
        public string Responsible { get; set; }
        public string Survey { get; set; }

        public string Properties { get; set; }

        public string Comments { get; set; }
        public bool Valid { get; set; }

        public bool? JustInitilized { get; set; }
        public bool? CreatedOnClient { get; set; }
        public long SurveyVersion { get; set; }

        public DashboardQuestionnaireItem GetDashboardItem(string surveyKey, string title, string comments)
        {
            return new DashboardQuestionnaireItem(
                Guid.Parse(this.Id), Guid.Parse(surveyKey), this.GetTypedStatus(),
                this.GetProperties(), title, comments, this.CreatedOnClient,
                this.JustInitilized.HasValue && this.JustInitilized.Value);
        }

        private InterviewStatus GetTypedStatus()
        {
            return (InterviewStatus) this.Status;
        }

        public FeaturedItem[] GetProperties()
        {
            if (string.IsNullOrEmpty(this.Properties))
                return new FeaturedItem[0];
            return JsonUtils.GetObject<FeaturedItem[]>(this.Properties);
        }

        public void SetProperties(IEnumerable<FeaturedItem> properties)
        {
            this.Properties = JsonUtils.GetJsonData(properties.ToArray());
        }
    }
}
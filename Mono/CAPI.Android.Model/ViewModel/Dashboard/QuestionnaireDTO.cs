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
            long surveyVersion, bool? createdOnClient = false)
        {
            Id = id.FormatGuid();
            Status = (int)status;
            
            Responsible = responsible.FormatGuid();
            Survey = survey.FormatGuid();

            this.SetProperties(properties);

            CreatedOnClient = createdOnClient;
            SurveyVersion = surveyVersion;
            
        }

        public QuestionnaireDTO()
        {
        }

        public int Status { get; set; }
        public string Responsible { get; set; }
        public string Survey { get; set; }

        public string Properties { get; set; }

        public string Comments { get; set; }
        public bool Valid { get; set; }

        public bool? CreatedOnClient { get; set; }
        public long SurveyVersion { get; set; }

        public DashboardQuestionnaireItem GetDashboardItem(string surveyKey, string title)
        {
            return new DashboardQuestionnaireItem(
                Guid.Parse(Id),Guid.Parse(surveyKey), GetTypedStatus(),
                GetProperties(), title, CreatedOnClient);
        }

        private InterviewStatus GetTypedStatus()
        {
                return (InterviewStatus)Status;
        }


        public FeaturedItem[] GetProperties()
        {
            if (string.IsNullOrEmpty(Properties))
                return new FeaturedItem[0];
            return JsonUtils.GetObject<FeaturedItem[]>(Properties);
        }

        public void SetProperties(IEnumerable<FeaturedItem> properties)
        {
            Properties = JsonUtils.GetJsonData(properties.ToArray());
        }
    }
}
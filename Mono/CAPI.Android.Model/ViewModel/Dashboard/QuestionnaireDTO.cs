using System;
using System.Collections.Generic;
using System.Linq;
using AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Capi.ModelUtils;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Core.Model.ViewModel.Dashboard
{
    public class QuestionnaireDTO : DenormalizerRow
    {
        public QuestionnaireDTO(Guid id, Guid responsible, Guid survey, InterviewStatus status, IList<FeaturedItem> properties)
        {
            Id = id.FormatGuid();
            Status = (int)status;
            Properties = JsonUtils.GetJsonData(properties.ToArray());
            Responsible = responsible.FormatGuid();
            Survey = survey.FormatGuid();
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

        public DashboardQuestionnaireItem GetDashboardItem(string surveyKey, string title)
        {
            return new DashboardQuestionnaireItem(
                Guid.Parse(Id),Guid.Parse(surveyKey), GetTypedStatus(),
                GetProperties(), title);
        }

        private InterviewStatus GetTypedStatus()
        {
                return (InterviewStatus)Status;
        }


        private FeaturedItem[] GetProperties()
        {
            if (string.IsNullOrEmpty(Properties))
                return new FeaturedItem[0];
            return JsonUtils.GetObject<FeaturedItem[]>(Properties);
        }
    }
}
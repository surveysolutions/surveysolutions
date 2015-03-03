using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Capi.ViewModel.Dashboard
{
    public class DashboardQuestionnaireItem
    {
        public DashboardQuestionnaireItem(Guid publicKey, Guid surveyKey, InterviewStatus status, IEnumerable<FeaturedItem> properties,
            string title, string comments, bool? createdOnClient = false, bool canBeDeleted = false)
        {
            this.PublicKey = publicKey;
            this.Status = status;
            this.Properties = properties;
            this.SurveyKey = surveyKey;
            this.Title = title;
            this.Comments = comments;
            this.CreatedOnClient = createdOnClient;
            this.CanBeDeleted = canBeDeleted;
        }

        public IEnumerable<FeaturedItem> Properties { get; private set; }

        public Guid PublicKey { get; private set; }

        public string Title { get; private set; }

        public Guid SurveyKey { get; private set; }

        public bool? CreatedOnClient { get; set; }

        public bool CanBeDeleted { get; set; }

        public InterviewStatus Status { get; private set; }

        public string Comments { get; private set; }
    }
}
using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Interviewer.ViewModel.Dashboard
{
    public class DashboardQuestionnaireItem
    {
        public DashboardQuestionnaireItem(Guid publicKey, Guid surveyKey, DashboardInterviewCategories status, IEnumerable<FeaturedItem> properties,
            string title, long questionnaireVersion, string comments, DateTime? startedDateTime,
            DateTime? complitedDateTime, DateTime? createdDateTime, bool? createdOnClient = false, 
            bool canBeDeleted = false)
        {
            this.PublicKey = publicKey;
            this.Status = status;
            this.Properties = properties;
            this.SurveyKey = surveyKey;
            this.Title = title;
            this.QuestionnaireVersion = questionnaireVersion;
            this.Comments = comments;
            this.StartedDateTime = startedDateTime;
            this.ComplitedDateTime = complitedDateTime;
            this.CreatedDateTime = createdDateTime;
            this.CreatedOnClient = createdOnClient;
            this.CanBeDeleted = canBeDeleted;
        }

        public IEnumerable<FeaturedItem> Properties { get; private set; }

        public Guid PublicKey { get; private set; }

        public string Title { get; private set; }

        public long QuestionnaireVersion { get; private set; }

        public Guid SurveyKey { get; private set; }

        public bool? CreatedOnClient { get; set; }

        public bool CanBeDeleted { get; set; }

        public DashboardInterviewCategories Status { get; private set; }

        public string Comments { get; private set; }

        public DateTime? StartedDateTime { get; private set; }
        
        public DateTime? ComplitedDateTime { get; private set; }

        public DateTime? CreatedDateTime { get; private set; }
    }
}
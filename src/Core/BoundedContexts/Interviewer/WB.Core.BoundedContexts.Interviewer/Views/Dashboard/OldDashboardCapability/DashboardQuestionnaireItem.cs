using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.BoundedContexts.Interviewer.ViewModel.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.UI.Interviewer.ViewModel.Dashboard
{
    public class DashboardQuestionnaireItem
    {
        public class GpsCoordinates
        {
            public double Latitude { get; private set; }
            public double Longitude { get; private set; }

            public GpsCoordinates(double latitude, double longitude)
            {
                this.Latitude = latitude;
                this.Longitude = longitude;
            }
        }

        public DashboardQuestionnaireItem(Guid publicKey, Guid surveyKey, DashboardInterviewStatus status,
            IEnumerable<FeaturedItem> properties, string title, long questionnaireVersion, string comments, 
            DateTime? startedDateTime, DateTime? completedDateTime, DateTime? createdDateTime, 
            DateTime? rejectedDateTime, GpsCoordinates gpsLocation,
            bool? createdOnClient = false, bool canBeDeleted = false)
        {
            this.PublicKey = publicKey;
            this.Status = status;
            this.Properties = properties;
            this.SurveyKey = surveyKey;
            this.Title = title;
            this.QuestionnaireVersion = questionnaireVersion;
            this.Comments = comments;
            this.StartedDateTime = startedDateTime;
            this.CompletedDateTime = completedDateTime;
            this.CreatedDateTime = createdDateTime;
            this.RejectedDateTime = rejectedDateTime;
            this.GpsLocation = gpsLocation;
            this.CreatedOnClient = createdOnClient;
            this.CanBeDeleted = canBeDeleted;
        }

        public IEnumerable<FeaturedItem> Properties { get; private set; }

        public GpsCoordinates GpsLocation { get; private set; }

        public Guid PublicKey { get; private set; }

        public string Title { get; private set; }

        public long QuestionnaireVersion { get; private set; }

        public Guid SurveyKey { get; private set; }

        public bool? CreatedOnClient { get; set; }

        public bool CanBeDeleted { get; set; }

        public DashboardInterviewStatus Status { get; private set; }

        public string Comments { get; private set; }

        public DateTime? StartedDateTime { get; private set; }
        
        public DateTime? CompletedDateTime { get; private set; }

        public DateTime? CreatedDateTime { get; private set; }

        public DateTime? RejectedDateTime { get; private set; }
    }
}
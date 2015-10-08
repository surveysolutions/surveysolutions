using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.BoundedContexts.Interviewer.ViewModel.Dashboard;
using WB.UI.Interviewer.Implementations.DenormalizerStorage;

namespace WB.UI.Interviewer.ViewModel.Dashboard
{
    public class QuestionnaireDTO : DenormalizerRow
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

        private ISerializer serializer
        {
            get { return ServiceLocator.Current.GetInstance<ISerializer>(); }
        }

        public QuestionnaireDTO(Guid id, Guid responsible, Guid survey, InterviewStatus status, IEnumerable<FeaturedItem> properties, long surveyVersion, string comments, DateTime createdDateTime, DateTime? startedDateTime, DateTime? rejectedDateTime, GpsCoordinates gpsLocation, bool? createdOnClient = false, bool justInitilized = false)
        {
            this.Id = id.FormatGuid();
            this.Status = (int)status;
            this.Responsible = responsible.FormatGuid();
            this.Survey = survey.FormatGuid();
            this.CreatedOnClient = createdOnClient;
            this.JustInitilized = justInitilized;
            this.SurveyVersion = surveyVersion;
            this.Comments = comments;
            this.CreatedDateTime = createdDateTime;
            this.StartedDateTime = startedDateTime;
            this.RejectedDateTime = rejectedDateTime;
            this.GpsLocation = gpsLocation;

            this.SetProperties(properties);
        }

        public QuestionnaireDTO() { }

        public int Status { get; set; }
        public string Responsible { get; set; }
        public string Survey { get; set; }

        public string Properties { get; set; }

        public string Comments { get; set; }
        public bool Valid { get; set; }

        public bool? JustInitilized { get; set; }
        public bool? CreatedOnClient { get; set; }
        public long SurveyVersion { get; set; }

        public DateTime? StartedDateTime { get; set; }
        public DateTime? CompletedDateTime { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public DateTime? RejectedDateTime { get; set; }
        public GpsCoordinates GpsLocation { get; private set; }

        public IEnumerable<FeaturedItem> GetPrefilledQuestions()
        {
            return string.IsNullOrEmpty(this.Properties)
                ? new FeaturedItem[0]
                : this.serializer.Deserialize<IEnumerable<FeaturedItem>>(this.Properties);
        }

        public void SetProperties(IEnumerable<FeaturedItem> properties)
        {
            this.Properties = this.serializer.Serialize(properties);
        }
    }
}
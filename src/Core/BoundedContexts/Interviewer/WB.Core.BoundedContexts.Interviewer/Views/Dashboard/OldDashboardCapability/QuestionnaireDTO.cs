using System;
using Cirrious.MvvmCross.Plugins.Sqlite;

namespace WB.UI.Interviewer.ViewModel.Dashboard
{
    [Obsolete]
    public class QuestionnaireDTO
    {
        [PrimaryKey]
        public string Id { get; set; }
        public int Status { get; set; }
        public string Responsible { get; set; }
        public string Survey { get; set; }

        public string Properties { get; set; }

        public string Comments { get; set; }
        public bool Valid { get; set; }

        public bool? JustInitilized { get; set; }
        public bool? CreatedOnClient { get; set; }
        public long SurveyVersion { get; set; }

        public DateTime? CreatedDateTime { get; set; }
        public DateTime? StartedDateTime { get; set; }
        public DateTime? CompletedDateTime { get; set; }
        public DateTime? InterviewerAssignedDateTime { get; set; }
        public DateTime? RejectedDateTime { get; set; }

        public string GpsLocationQuestionId { get; set; }
        public double? GpsLocationLatitude { get; set; }
        public double? GpsLocationLongitude { get; set; }
    }
}
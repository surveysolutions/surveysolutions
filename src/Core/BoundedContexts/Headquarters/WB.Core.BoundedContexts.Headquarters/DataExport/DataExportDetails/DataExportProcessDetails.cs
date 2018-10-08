using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails
{
    public class DataExportProcessDetails : AbstractDataExportProcessDetails
    {
        public DataExportProcessDetails(DataExportFormat format, QuestionnaireIdentity questionnaire, string questionnaireTitle)
            : base(format)
        {
            this.Questionnaire = questionnaire;
            this.QuestionnaireTitle = questionnaireTitle;
        }

        public QuestionnaireIdentity Questionnaire { get; }
        public ExternalStorageType? StorageType { get; set; }
        public string AccessToken { get; set; }
        public string QuestionnaireTitle { get; }

        public override string NaturalId => $"{InterviewStatusString()}${this.Format}${this.Questionnaire}" +
                                            $"${this.FromDate?.ToString("YYYYMMDD") ?? "EMPTY FROM DATE"}" +
                                            $"${this.ToDate?.ToString("YYYYMMDD") ?? "EMPTY TO DATE"}";

        public override string Name => $"(ver. {this.Questionnaire.Version}) {this.QuestionnaireTitle}" 
        + (StorageType.HasValue ? $" {Enum.GetName(typeof(ExternalStorageType), this.StorageType)}" : "");

        public InterviewStatus? InterviewStatus { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        private string InterviewStatusString() => InterviewStatus?.ToString() ?? "All";
    }
}

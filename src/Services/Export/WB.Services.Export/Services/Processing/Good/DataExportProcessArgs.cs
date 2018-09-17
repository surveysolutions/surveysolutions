using System;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Services.Processing
{
    //public class DataExportProcessArgs
    //{
    //    public TenantInfo Tenant { get; set; }
    //    public QuestionnaireId Questionnaire { get; set; }
    //    public ExternalStorageType? StorageType { get; set; }
    //    public DataExportFormat Format { get; set; }
    //    public DateTime? FromDate { get; set; }
    //    public DateTime? ToDate { get; set; }
    //    public InterviewStatus? InterviewStatus { get; set; }

    //    public string NaturalId => $"{InterviewStatusString()}${this.Format}${this.Questionnaire}" +
    //                                        $"${this.FromDate?.ToString(@"YYYYMMDD") ?? "EMPTY FROM DATE"}" +
    //                                        $"${this.ToDate?.ToString(@"YYYYMMDD") ?? "EMPTY TO DATE"}";


    //    private string InterviewStatusString() => InterviewStatus?.ToString() ?? "All";
    //}

    //public class DataExportProcessState
    //{
    //    public DataExportProcessArgs Args { get; set; }

    //}

    public class DataExportProcessDetails : AbstractDataExportProcessDetails
    {
        public DataExportProcessDetails(DataExportFormat format, QuestionnaireId questionnaire, string questionnaireTitle)
            : base(format)
        {
            this.Questionnaire = questionnaire;
            this.QuestionnaireTitle = questionnaireTitle;
        }

        public TenantInfo Tenant { get; set; }
        public QuestionnaireId Questionnaire { get; }
        public ExternalStorageType? StorageType { get; set; }
        public string AccessToken { get; set; }

        public string QuestionnaireTitle { get; }

        public override string NaturalId => $"{InterviewStatusString()}${this.Format}${this.Questionnaire}" +
                                            $"${this.FromDate?.ToString(@"YYYYMMDD") ?? "EMPTY FROM DATE"}" +
                                            $"${this.ToDate?.ToString(@"YYYYMMDD") ?? "EMPTY TO DATE"}";

        public override string Name => $"(ver. {this.Questionnaire}) {this.QuestionnaireTitle}";

        public InterviewStatus? InterviewStatus { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        private string InterviewStatusString() => InterviewStatus?.ToString() ?? "All";
    }
}

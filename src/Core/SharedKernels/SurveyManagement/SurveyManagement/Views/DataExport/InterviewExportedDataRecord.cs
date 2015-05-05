using System;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public class InterviewExportedDataRecord : IView
    {
        public virtual string InterviewId { get; set; }
        public virtual Guid QuestionnaireId { get; set; }
        public virtual long QuestionnaireVersion { get; set; }
        public virtual byte[] Data { get; set; }
        public virtual string LastAction { get; set; }
    }
}
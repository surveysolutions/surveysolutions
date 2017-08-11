using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reports.InputModels
{
    public class CountDaysOfInterviewInStatusApiModel
    {
        public Guid? QuestionnaireId { get; set; }
        public long? QuestionnaireVersion { get; set; }
    }
}
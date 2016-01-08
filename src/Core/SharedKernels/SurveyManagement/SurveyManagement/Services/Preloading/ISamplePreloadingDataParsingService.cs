using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Preloading
{
    public interface ISamplePreloadingDataParsingService
    {
        InterviewSampleData[] ParseSample(string sampleId, QuestionnaireIdentity questionnaireIdentity);
    }

    public class InterviewSampleData
    {
        public Guid? SupervisorId { get; set; }
        public Guid? InterviewerId { get; set; }
        public Dictionary<Guid, object> Answers { get; set; }
    }
}
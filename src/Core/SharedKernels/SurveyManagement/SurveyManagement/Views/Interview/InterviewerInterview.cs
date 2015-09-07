using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewerInterview
    {
        public Guid Id { get; set; }
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
    }
}
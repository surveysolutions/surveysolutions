using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class CreateTemporaryInterviewCommand : InterviewCommand
    {
        public QuestionnaireIdentity QuestionnaireId { get; }

        public CreateTemporaryInterviewCommand(Guid interviewId, Guid userId, QuestionnaireIdentity questionnaireId) : base(interviewId, userId)
        {
            QuestionnaireId = questionnaireId;
        }
    }
}

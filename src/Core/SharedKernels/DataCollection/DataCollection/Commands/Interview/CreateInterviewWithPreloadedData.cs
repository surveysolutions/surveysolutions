using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class CreateInterviewWithPreloadedData : InterviewCommand
    {
        public CreateInterviewWithPreloadedData(Guid interviewId, Guid userId, Guid questionnaireId, long version, PreloadedDataDto preloadedDataDto, DateTime answersTime, Guid supervisorId)
            : base(interviewId, userId)
        {
            this.Id = interviewId;
            this.QuestionnaireId = questionnaireId;
            this.Version = version;
            this.PreloadedData = preloadedDataDto;
            this.AnswersTime = answersTime;
            this.SupervisorId = supervisorId;
        }

        public Guid Id { get; private set; }
        public Guid QuestionnaireId { get; private set; }
        public long Version { get; private set; }
        public PreloadedDataDto PreloadedData { get; private set; }
        public Guid SupervisorId { get; private set; }

        public DateTime AnswersTime { get; private set; }
    }
}

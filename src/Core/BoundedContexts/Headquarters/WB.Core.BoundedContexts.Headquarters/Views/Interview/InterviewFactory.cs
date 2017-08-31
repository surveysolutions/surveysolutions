using System;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewFactory : IInterviewFactory
    {
        private readonly IPlainStorageAccessor<DbQuestion> _repository;

        public InterviewFactory(IPlainStorageAccessor<DbQuestion> interviewQuestionsRepository)
        {
            _repository = interviewQuestionsRepository;
        }

        public Identity[] GetFlaggedQuestionIds(Guid interviewId) => this._repository.Query(_
            => _.Where(x => x.InterviewId == interviewId && x.IsFlagged).Select(x => x.QuestionIdentity).ToArray());

        public void SetFlagToQuestion(Guid interviewId, Identity questionIdentity)
        {
            var flaggedQuestion = this._repository.Query(_ => _.FirstOrDefault(x => x.InterviewId == interviewId && x.QuestionIdentity == questionIdentity)) ??
                                  new DbQuestion {InterviewId = interviewId, QuestionIdentity = questionIdentity};

            flaggedQuestion.IsFlagged = true;

            this._repository.Store(flaggedQuestion, null);
        }

        public void RemoveFlagFromQuestion(Guid interviewId, Identity questionIdentity)
        {
            var flaggedQuestion = this._repository.Query(_ =>
                _.FirstOrDefault(x => x.InterviewId == interviewId && x.QuestionIdentity == questionIdentity));

            if(flaggedQuestion != null)
                this._repository.Remove(flaggedQuestion);
        }
    }
}
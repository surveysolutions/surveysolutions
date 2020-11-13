using System;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Enumerator.Native.WebInterview.LifeCycle
{
    public class WebInterviewNotificationBuilder : IWebInterviewNotificationBuilder
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;

        public WebInterviewNotificationBuilder(IQuestionnaireStorage questionnaireStorage, IStatefulInterviewRepository statefulInterviewRepository)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.statefulInterviewRepository = statefulInterviewRepository;
        }
        private bool IsSupportFilterOptionCondition(IComposite documentEntity)
            => !string.IsNullOrWhiteSpace((documentEntity as IQuestion)?.Properties.OptionsFilterExpression);

        public void RefreshEntitiesWithFilteredOptions(InterviewLifecycle cycle, Guid interviewId)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());

            if (interview == null)
            {
                return;
            }

            var document = this.questionnaireStorage.GetQuestionnaireDocument(interview.QuestionnaireIdentity)
                           ?? throw new InterviewException("Questionnaire is missing", InterviewDomainExceptionType.QuestionnaireIsMissing);

            var entityIds = document.Find<IComposite>(this.IsSupportFilterOptionCondition)
                .Select(e => e.PublicKey).ToHashSet();

            foreach (var entityId in entityIds)
            {
                var identities = interview.GetAllIdentitiesForEntityId(entityId).ToArray();
                cycle.RefreshEntities(interviewId, identities);
            }
        }

        public void RefreshCascadingOptions(InterviewLifecycle cycle, Guid interviewId, Identity identity)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());
            if (interview == null) return;

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity, null)
                                ?? throw new InterviewException("Questionnaire is missing", InterviewDomainExceptionType.QuestionnaireIsMissing);

            var dependentQuestionIds = questionnaire.GetCascadingQuestionsThatDependUponQuestion(identity.Id);
            var dependentQuestionIdentities = dependentQuestionIds.SelectMany(x => interview.GetAllIdentitiesForEntityId(x)).ToArray();

            cycle.RefreshEntities(interviewId, dependentQuestionIdentities);
        }

        public void RefreshLinkedToListQuestions(InterviewLifecycle cycle, Guid interviewId, Identity questionIdentity)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());

            if (interview == null)
            {
                return;
            }

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity,
                interview.Language) ?? throw new InterviewException("Questionnaire is missing", InterviewDomainExceptionType.QuestionnaireIsMissing);

            if (interview.GetQuestion(questionIdentity) == null) return;
            if (interview.GetTextListQuestion(questionIdentity) == null) return;

            var listQuestionIds = questionnaire.GetLinkedToSourceEntity(questionIdentity.Id).ToArray();
            if (!listQuestionIds.Any())
                return;

            foreach (var listQuestionId in listQuestionIds)
            {
                var questionsToRefresh = interview.FindQuestionsFromSameOrDeeperLevel(listQuestionId,
                    questionIdentity);
                cycle.RefreshEntities(interviewId, questionsToRefresh.ToArray());
            }
        }

        public virtual void RefreshLinkedToRosterQuestions(InterviewLifecycle cycle, Guid interviewId, Identity[] rosterIdentities)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());

            if (interview == null)
            {
                return;
            }

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity,
                interview.Language) ?? throw new InterviewException("Questionnaire is missing", InterviewDomainExceptionType.QuestionnaireIsMissing);

            var rosterIds = rosterIdentities.Select(x => x.Id).Distinct();

            var linkedToRosterQuestionIds = rosterIds.SelectMany(x => questionnaire.GetLinkedToSourceEntity(x));

            foreach (var linkedToRosterQuestionId in linkedToRosterQuestionIds)
            {
                var identitiesToRefresh = interview.GetAllIdentitiesForEntityId(linkedToRosterQuestionId).ToArray();
                cycle.RefreshEntities(interviewId, identitiesToRefresh);
            }
        }

    }
}

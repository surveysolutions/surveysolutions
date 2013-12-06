using System;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace Core.Supervisor.Views.Interview
{
    using Main.Core.Documents;
    using Main.Core.View;
    using WB.Core.BoundedContexts.Supervisor.Views.Interview;
    using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

    public class InterviewDetailsViewFactory : IViewFactory<InterviewDetailsInputModel, InterviewDetailsView>
    {
        private readonly IReadSideRepositoryReader<InterviewData> interviewStore;
        private readonly IReadSideRepositoryReader<UserDocument> userStore;
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireStore;
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireRosterStructure> questionnaireRosterStructures;
        private readonly IVersionedReadSideRepositoryReader<ReferenceInfoForLinkedQuestions> questionnaireReferenceInfoForLinkedQuestions;
        private readonly IInterviewDataAndQuestionnaireMerger merger;

        public InterviewDetailsViewFactory(IReadSideRepositoryReader<InterviewData> interviewStore,
            IReadSideRepositoryReader<UserDocument> userStore,
            IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireStore,
            IVersionedReadSideRepositoryReader<QuestionnaireRosterStructure> questionnaireRosterStructures,
            IVersionedReadSideRepositoryReader<ReferenceInfoForLinkedQuestions> questionnaireReferenceInfoForLinkedQuestions,
            IInterviewDataAndQuestionnaireMerger merger)
        {
            this.interviewStore = interviewStore;
            this.userStore = userStore;
            this.questionnaireStore = questionnaireStore;
            this.questionnaireRosterStructures = questionnaireRosterStructures;
            this.questionnaireReferenceInfoForLinkedQuestions = questionnaireReferenceInfoForLinkedQuestions;
            this.merger = merger;
        }

        public InterviewDetailsView Load(InterviewDetailsInputModel input)
        {
            var interview = this.interviewStore.GetById(input.CompleteQuestionnaireId);
            if (interview == null || interview.IsDeleted)
                return null;

            QuestionnaireDocumentVersioned questionnaire = this.questionnaireStore.GetById(interview.QuestionnaireId, interview.QuestionnaireVersion);
            if (questionnaire == null)
                throw new ArgumentException(string.Format(
                    "Questionnaire with id {0} and version {1} is missing.", interview.QuestionnaireId, interview.QuestionnaireVersion));

            var questionnaireReferenceInfo = this.questionnaireReferenceInfoForLinkedQuestions.GetById(interview.QuestionnaireId, interview.QuestionnaireVersion);

             var questionnaireRosters = this.questionnaireRosterStructures.GetById(interview.QuestionnaireId, interview.QuestionnaireVersion);

            var user = this.userStore.GetById(interview.ResponsibleId);
            if (user == null)
                throw new ArgumentException(string.Format("User with id {0} is not found.", interview.ResponsibleId));

            return merger.Merge(interview, questionnaire, questionnaireReferenceInfo, questionnaireRosters, user);
                bool isQustionsParentGroupDisabled = interviewLevel.DisabledGroups != null && interviewLevel.DisabledGroups.Contains(currentGroup.PublicKey);
                    : new InterviewQuestionView(question, answeredQuestion, idToVariableMap, answersForTitleSubstitution,
                        isQustionsParentGroupDisabled);
        }
    }
}

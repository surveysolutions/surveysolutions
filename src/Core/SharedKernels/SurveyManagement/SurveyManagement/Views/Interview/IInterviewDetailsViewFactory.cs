using System;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public interface IInterviewDetailsViewFactory
    {
        InterviewDetailsView GetInterviewDetails(Guid interviewId);
    }

    public class InterviewDetailsViewFactoryNew : IInterviewDetailsViewFactory
    {
        private readonly IReadSideKeyValueStorage<InterviewData> interviewStore;
        private readonly IReadSideRepositoryReader<UserDocument> userStore;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireStore;
        private readonly IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructures;
        private readonly IReadSideKeyValueStorage<ReferenceInfoForLinkedQuestions> questionnaireReferenceInfoForLinkedQuestions;
        private readonly IInterviewDataAndQuestionnaireMerger merger;

        public InterviewDetailsViewFactoryNew(IReadSideKeyValueStorage<InterviewData> interviewStore,
            IReadSideRepositoryReader<UserDocument> userStore,
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireStore,
            IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructures,
            IReadSideKeyValueStorage<ReferenceInfoForLinkedQuestions> questionnaireReferenceInfoForLinkedQuestions,
            IInterviewDataAndQuestionnaireMerger merger)
        {
            this.interviewStore = interviewStore;
            this.userStore = userStore;
            this.questionnaireStore = questionnaireStore;
            this.questionnaireRosterStructures = questionnaireRosterStructures;
            this.questionnaireReferenceInfoForLinkedQuestions = questionnaireReferenceInfoForLinkedQuestions;
            this.merger = merger;
        }

        public InterviewDetailsView GetInterviewDetails(Guid interviewId)
        {
            var interview = interviewStore.GetById(interviewId);

            if (interview == null || interview.IsDeleted)
                return null;

            QuestionnaireDocumentVersioned questionnaire = this.questionnaireStore.AsVersioned().Get(interview.QuestionnaireId.FormatGuid(), interview.QuestionnaireVersion);

            if (questionnaire == null)
                throw new ArgumentException(string.Format("Questionnaire with id {0} and version {1} is missing.", interview.QuestionnaireId, interview.QuestionnaireVersion));

            var questionnaireReferenceInfo = this.questionnaireReferenceInfoForLinkedQuestions.AsVersioned().Get(interview.QuestionnaireId.FormatGuid(), interview.QuestionnaireVersion);

            var questionnaireRosters = this.questionnaireRosterStructures.AsVersioned().Get(interview.QuestionnaireId.FormatGuid(), interview.QuestionnaireVersion);

            var user = this.userStore.GetById(interview.ResponsibleId);
            if (user == null)
                throw new ArgumentException(string.Format("User with id {0} is not found.", interview.ResponsibleId));

            return merger.Merge(interview, questionnaire, questionnaireReferenceInfo, questionnaireRosters, user);
        }
    }
}
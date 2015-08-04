using System;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Revalidate
{
    public class InterviewInfoForRevalidationFactory : IViewFactory<InterviewInfoForRevalidationInputModel, InterviewInfoForRevalidationView>
    {
        private readonly IInterviewDataAndQuestionnaireMerger merger;
        private readonly IReadSideKeyValueStorage<InterviewData> interviewStore;
        private readonly IReadSideRepositoryReader<UserDocument> userStore;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireStore;
        private readonly IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructures;
        private readonly IReadSideKeyValueStorage<ReferenceInfoForLinkedQuestions> questionnaireReferenceInfoForLinkedQuestions;

        public InterviewInfoForRevalidationFactory(IReadSideKeyValueStorage<InterviewData> interviewStore,
            IReadSideRepositoryReader<UserDocument> userStore,
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireStore,
            IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructures,
            IReadSideKeyValueStorage<ReferenceInfoForLinkedQuestions> questionnaireReferenceInfoForLinkedQuestions,
            IInterviewDataAndQuestionnaireMerger merger)
        {
            this.merger = merger;
            this.interviewStore = interviewStore;
            this.userStore = userStore;
            this.questionnaireStore = questionnaireStore;
            this.questionnaireRosterStructures = questionnaireRosterStructures;
            this.questionnaireReferenceInfoForLinkedQuestions = questionnaireReferenceInfoForLinkedQuestions;
        }

        public InterviewInfoForRevalidationView Load(InterviewInfoForRevalidationInputModel input)
        {
            var interview = this.interviewStore.GetById(input.InterviewId);
            if (interview == null || interview.IsDeleted)
                return null;

            QuestionnaireDocumentVersioned questionnaire = this.questionnaireStore.AsVersioned().Get(interview.QuestionnaireId.FormatGuid(), interview.QuestionnaireVersion);
            if (questionnaire == null)
                throw new ArgumentException(string.Format(
                    "Questionnaire with id {0} and version {1} is missing.", interview.QuestionnaireId, interview.QuestionnaireVersion));

            var questionnaireReferenceInfo = this.questionnaireReferenceInfoForLinkedQuestions.AsVersioned().Get(interview.QuestionnaireId.FormatGuid(), interview.QuestionnaireVersion);

            var questionnaireRosters = this.questionnaireRosterStructures.AsVersioned().Get(interview.QuestionnaireId.FormatGuid(), interview.QuestionnaireVersion);

            var user = this.userStore.GetById(interview.ResponsibleId);
            if (user == null)
                throw new ArgumentException(string.Format("User with id {0} is not found.", interview.ResponsibleId));

            var mergedInterview = this.merger.Merge(interview, questionnaire, questionnaireReferenceInfo, questionnaireRosters, user);


            var revalidateInterviewView = new InterviewInfoForRevalidationView
            {
                Responsible = mergedInterview.Responsible,
                QuestionnairePublicKey = mergedInterview.QuestionnairePublicKey,
                Title = mergedInterview.Title,
                Description = mergedInterview.Description,
                PublicKey = mergedInterview.PublicKey,
                Status = mergedInterview.Status,
                InterviewId = input.InterviewId
            };

            revalidateInterviewView.FeaturedQuestions.AddRange(mergedInterview.Groups.SelectMany(group => group.Entities.OfType<InterviewQuestionView>().Where(q => q.IsFeatured)));
            revalidateInterviewView.MandatoryQuestions.AddRange(mergedInterview.Groups.SelectMany(group => group.Entities.OfType<InterviewQuestionView>().Where(q => q.IsMandatory && q.IsEnabled)));

            return revalidateInterviewView;
        }
    }
}

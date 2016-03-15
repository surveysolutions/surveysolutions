using System;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Revalidate
{
    public class InterviewTroubleshootFactory : IViewFactory<InterviewTroubleshootInputModel, InterviewTroubleshootView>
    {
        private readonly IInterviewDataAndQuestionnaireMerger merger;
        private readonly IReadSideKeyValueStorage<InterviewData> interviewStore;
        private readonly IReadSideKeyValueStorage<InterviewLinkedQuestionOptions> interviewLinkedQuestionOptionsStore;
        private readonly IReadSideRepositoryReader<UserDocument> userStore;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;

        public InterviewTroubleshootFactory(IReadSideKeyValueStorage<InterviewData> interviewStore,
            IReadSideRepositoryReader<UserDocument> userStore,
            IInterviewDataAndQuestionnaireMerger merger, IPlainQuestionnaireRepository plainQuestionnaireRepository, IReadSideKeyValueStorage<InterviewLinkedQuestionOptions> interviewLinkedQuestionOptionsStore)
        {
            this.merger = merger;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.interviewLinkedQuestionOptionsStore = interviewLinkedQuestionOptionsStore;
            this.interviewStore = interviewStore;
            this.userStore = userStore;
        }

        public InterviewTroubleshootView Load(InterviewTroubleshootInputModel input)
        {
            var interview = this.interviewStore.GetById(input.InterviewId);
            if (interview == null || interview.IsDeleted)
                return null;

            var questionnaire = this.plainQuestionnaireRepository.GetQuestionnaireDocument(interview.QuestionnaireId, interview.QuestionnaireVersion);
            if (questionnaire == null)
                throw new ArgumentException(
                    $"Questionnaire with id {interview.QuestionnaireId} and version {interview.QuestionnaireVersion} is missing.");

            var user = this.userStore.GetById(interview.ResponsibleId);
            if (user == null)
                throw new ArgumentException($"User with id {interview.ResponsibleId} is not found.");

            var mergedInterview = this.merger.Merge(interview, questionnaire, user.GetUseLight(), this.interviewLinkedQuestionOptionsStore.GetById(input.InterviewId));


            var interviewTroubleshootView = new InterviewTroubleshootView
            {
                Responsible = mergedInterview.Responsible,
                QuestionnairePublicKey = mergedInterview.QuestionnairePublicKey,
                QuestionnaireVersion = interview.QuestionnaireVersion,
                Title = mergedInterview.Title,
                Description = mergedInterview.Description,
                PublicKey = mergedInterview.PublicKey,
                Status = mergedInterview.Status,
                InterviewId = input.InterviewId
            };

            interviewTroubleshootView.FeaturedQuestions.AddRange(mergedInterview.Groups.SelectMany(group => group.Entities.OfType<InterviewQuestionView>().Where(q => q.IsFeatured)));
            
            return interviewTroubleshootView;
        }
    }
}

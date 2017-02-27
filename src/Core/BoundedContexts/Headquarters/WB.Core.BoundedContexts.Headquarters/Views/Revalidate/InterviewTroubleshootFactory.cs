using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.Revalidate
{
    public interface IInterviewTroubleshootFactory
    {
        InterviewTroubleshootView Load(InterviewTroubleshootInputModel input);
    }

    public class InterviewTroubleshootFactory : IInterviewTroubleshootFactory
    {
        private readonly IInterviewDataAndQuestionnaireMerger merger;
        private readonly IReadSideKeyValueStorage<InterviewData> interviewStore;
        private readonly IReadSideKeyValueStorage<InterviewLinkedQuestionOptions> interviewLinkedQuestionOptionsStore;
        private readonly IPlainStorageAccessor<UserDocument> userStore;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IAttachmentContentService attachmentContentService;

        public InterviewTroubleshootFactory(IReadSideKeyValueStorage<InterviewData> interviewStore,
            IPlainStorageAccessor<UserDocument> userStore,
            IInterviewDataAndQuestionnaireMerger merger, 
            IQuestionnaireStorage questionnaireStorage, 
            IReadSideKeyValueStorage<InterviewLinkedQuestionOptions> interviewLinkedQuestionOptionsStore,
            IAttachmentContentService attachmentContentService)
        {
            this.merger = merger;
            this.questionnaireStorage = questionnaireStorage;
            this.interviewLinkedQuestionOptionsStore = interviewLinkedQuestionOptionsStore;
            this.interviewStore = interviewStore;
            this.userStore = userStore;
            this.attachmentContentService = attachmentContentService;
        }

        public InterviewTroubleshootView Load(InterviewTroubleshootInputModel input)
        {
            var interview = this.interviewStore.GetById(input.InterviewId);
            if (interview == null || interview.IsDeleted)
                return null;

            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(interview.QuestionnaireId, interview.QuestionnaireVersion);
            if (questionnaire == null)
                throw new ArgumentException(
                    $"Questionnaire with id {interview.QuestionnaireId} and version {interview.QuestionnaireVersion} is missing.");

            var user = this.userStore.GetById(interview.ResponsibleId.FormatGuid());
            if (user == null)
                throw new ArgumentException($"User with id {interview.ResponsibleId} is not found.");

            var attachmentIdAndTypes = this.attachmentContentService.GetAttachmentInfosByContentIds(questionnaire.Attachments.Select(x => x.ContentId).ToList());
            var mergedInterview = this.merger.Merge(interview, questionnaire, user.GetUseLight(), 
                this.interviewLinkedQuestionOptionsStore.GetById(input.InterviewId), attachmentIdAndTypes);

            var interviewTroubleshootView = new InterviewTroubleshootView
            {
                Responsible = mergedInterview.Responsible,
                QuestionnairePublicKey = interview.QuestionnaireId,
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

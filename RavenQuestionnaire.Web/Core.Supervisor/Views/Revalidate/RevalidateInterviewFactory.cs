using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Supervisor.Views.Interview;
using Core.Supervisor.Views.UsersAndQuestionnaires;
using Main.Core.Documents;
using Main.Core.View;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace Core.Supervisor.Views.Revalidate
{
    public class RevalidateInterviewFactory : IViewFactory<RevalidateInterviewInputModel, RevalidatInterviewView>
    {
        public IInterviewDataAndQuestionnaireMerger merger;
        private readonly IReadSideRepositoryReader<InterviewData> interviewStore;
        private readonly IReadSideRepositoryReader<UserDocument> userStore;
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireStore;
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireRosterStructure> questionnaireRosterStructures;
        private readonly IVersionedReadSideRepositoryReader<ReferenceInfoForLinkedQuestions> questionnaireReferenceInfoForLinkedQuestions;

        public RevalidateInterviewFactory(IReadSideRepositoryReader<InterviewData> interviewStore,
            IReadSideRepositoryReader<UserDocument> userStore,
            IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireStore,
            IVersionedReadSideRepositoryReader<QuestionnaireRosterStructure> questionnaireRosterStructures,
            IVersionedReadSideRepositoryReader<ReferenceInfoForLinkedQuestions> questionnaireReferenceInfoForLinkedQuestions,
            IInterviewDataAndQuestionnaireMerger merger)
        {
            this.merger = merger;
            this.interviewStore = interviewStore;
            this.userStore = userStore;
            this.questionnaireStore = questionnaireStore;
            this.questionnaireRosterStructures = questionnaireRosterStructures;
            this.questionnaireReferenceInfoForLinkedQuestions = questionnaireReferenceInfoForLinkedQuestions;
        }

        public RevalidatInterviewView Load(RevalidateInterviewInputModel input)
        {
            var interview = this.interviewStore.GetById(input.InterviewId);
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

            var mergedInterview = merger.Merge(interview, questionnaire, questionnaireReferenceInfo, questionnaireRosters, user);


            var revalidateInterviewView = new RevalidatInterviewView
            {
                Responsible = mergedInterview.Responsible,
                QuestionnairePublicKey = mergedInterview.QuestionnairePublicKey,
                Title = mergedInterview.Title,
                Description = mergedInterview.Description,
                PublicKey = mergedInterview.PublicKey,
                Status = mergedInterview.Status,
                InterviewId = input.InterviewId
            };

            revalidateInterviewView.FeaturedQuestions.AddRange(mergedInterview.Groups.SelectMany(group => group.Questions.Where(q => q.IsFeatured)));
            revalidateInterviewView.MandatoryQuestions.AddRange(mergedInterview.Groups.SelectMany(group => group.Questions.Where(q => q.IsMandatory && q.IsEnabled)));

            return revalidateInterviewView;
        }
    }
}

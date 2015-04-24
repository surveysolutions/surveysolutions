using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class RosterReferenceViewModel : GroupReferenceViewModel
    {
        public string RosterTitle { get; set; }

        public RosterReferenceViewModel(
            IPlainRepository<QuestionnaireModel> questionnaireRepository, 
            IPlainRepository<InterviewModel> interviewRepository)
            : base(questionnaireRepository, interviewRepository)
        {
        }
    }
}
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class GroupViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        private readonly IStatefullInterviewRepository interviewRepository;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;

        public GroupViewModel(
            IStatefullInterviewRepository interviewRepository,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository)
        {
            this.interviewRepository = interviewRepository;
            this.questionnaireRepository = questionnaireRepository;
        }

        public void Init(string interviewId, Identity entityIdentity)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

            Title = questionnaire.GroupsWithoutNestedChildren[entityIdentity.Id].Title;
        }

        private string title;
        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(); }
        }
    }
}
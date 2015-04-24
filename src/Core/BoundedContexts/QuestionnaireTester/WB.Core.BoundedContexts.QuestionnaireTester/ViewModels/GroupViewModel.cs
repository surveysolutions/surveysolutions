using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class GroupViewModel : MvxNotifyPropertyChanged
    {
        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly IPlainRepository<QuestionnaireModel> questionnaireRepository;
        private readonly NavigationState navigationState;

        public GroupViewModel(IInterviewViewModelFactory interviewViewModelFactory,
             IPlainRepository<QuestionnaireModel> questionnaireRepository, NavigationState navigationState)
        {
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.questionnaireRepository = questionnaireRepository;
            this.navigationState = navigationState;
            this.navigationState.OnGroupChanged += navigationState_OnGroupChanged;
        }

        async void navigationState_OnGroupChanged(Identity newGroupIdentity)
        {
            await this.Init();
        }

        private Task Init()
        {
            return Task.Run(async () =>
            {
                var questionnaire = this.questionnaireRepository.Get(this.navigationState.QuestionnaireId);

                var group = questionnaire.GroupsWithoutNestedChildren[this.navigationState.CurrentGroup.Id];

                this.Name = group.Title;
                this.Items = await this.interviewViewModelFactory.GetEntitiesAsync(interviewId: this.navigationState.InterviewId,
                                                                                   groupIdentity: this.navigationState.CurrentGroup);
            });
        }

        private string name;
        public string Name
        {
            get { return name; } 
            set { name = value; RaisePropertyChanged(); }
        }

        private IEnumerable items;
        public IEnumerable Items
        {
            get { return items; }
            set { items = value; RaisePropertyChanged(); }
        }
    }
}
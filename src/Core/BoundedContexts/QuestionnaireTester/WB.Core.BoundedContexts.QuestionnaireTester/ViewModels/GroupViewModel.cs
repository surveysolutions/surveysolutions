using System;
using System.Collections;
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
        private NavigationState navigationState;

        public GroupViewModel(IInterviewViewModelFactory interviewViewModelFactory,
             IPlainRepository<QuestionnaireModel> questionnaireRepository)
        {
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.questionnaireRepository = questionnaireRepository;
        }

        public void Init(NavigationState navigationState)
        {
            if (navigationState == null) throw new ArgumentNullException("navigationState");
            if (this.navigationState != null) throw new Exception("ViewModel already initialized");

            this.navigationState = navigationState;
            this.navigationState.OnGroupChanged += navigationState_OnGroupChanged;
        }

        async void navigationState_OnGroupChanged(Identity newGroupIdentity)
        {
            var questionnaire = this.questionnaireRepository.Get(this.navigationState.QuestionnaireId);

            var group = questionnaire.GroupsWithoutNestedChildren[newGroupIdentity.Id];

            this.Name = group.Title;
            this.Items = await this.interviewViewModelFactory.GetEntitiesAsync(interviewId: this.navigationState.InterviewId,
                    groupIdentity: newGroupIdentity);
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
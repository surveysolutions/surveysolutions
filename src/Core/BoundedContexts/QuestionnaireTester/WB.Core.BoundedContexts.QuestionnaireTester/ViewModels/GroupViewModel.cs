using System;
using System.Collections;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class GroupViewModel : MvxNotifyPropertyChanged
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; RaisePropertyChanged(); }
        }

        private IList items;
        public IList Items
        {
            get { return items; }
            set { items = value; RaisePropertyChanged(); }
        }

        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private NavigationState navigationState;

        public GroupViewModel(IInterviewViewModelFactory interviewViewModelFactory,
             IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository)
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
            var questionnaire = this.questionnaireRepository.GetById(this.navigationState.QuestionnaireId);

            var group = questionnaire.GroupsWithoutNestedChildren[newGroupIdentity.Id];

            this.Name = group.Title;
            this.Items = this.interviewViewModelFactory.GetEntities(interviewId: this.navigationState.InterviewId, groupIdentity: newGroupIdentity);
        }
    }
}
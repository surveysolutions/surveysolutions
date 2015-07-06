using System;
using System.Collections;
using System.Linq;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class ActiveGroupViewModel : MvxNotifyPropertyChanged
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

        private readonly IMvxMessenger messenger;

        private NavigationState navigationState;

        public ActiveGroupViewModel(
            IInterviewViewModelFactory interviewViewModelFactory,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IMvxMessenger messenger)
        {
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.questionnaireRepository = questionnaireRepository;
            this.messenger = messenger;
        }

        public void Init(NavigationState navigationState)
        {
            if (navigationState == null) throw new ArgumentNullException("navigationState");
            if (this.navigationState != null) throw new Exception("ViewModel already initialized");

            this.navigationState = navigationState;
            this.navigationState.OnGroupChanged += navigationState_OnGroupChanged;
        }

        void navigationState_OnGroupChanged(GroupChangedEventArgs navigationParams)
        {
            var questionnaire = this.questionnaireRepository.GetById(this.navigationState.QuestionnaireId);

            var group = questionnaire.GroupsWithFirstLevelChildrenAsReferences[navigationParams.TargetGroup.Id];

            this.Name = group.Title;

            var listOfViewModels = this.interviewViewModelFactory.GetEntities(
                interviewId: this.navigationState.InterviewId,
                groupIdentity: navigationParams.TargetGroup, 
                navigationState: this.navigationState);

            this.AddToParentButton(listOfViewModels, navigationParams);

            var anchoreElementIndex = 0;

            if (navigationParams.AnchoredElementIdentity != null)
            {
                var childItem = group.Children.FirstOrDefault(x => x.Id == navigationParams.AnchoredElementIdentity.Id);
                anchoreElementIndex = childItem != null ? group.Children.IndexOf(childItem) : 0;
            }

            this.Items = listOfViewModels;
            messenger.Publish(new ScrollToAnchorMessage(this, anchoreElementIndex, 0));
        }

        private void AddToParentButton(IList listOfViewModels, GroupChangedEventArgs navigationParams)
        {
            var previousGroupNavigationViewModel = this.interviewViewModelFactory.GetNew<GroupNavigationViewModel>();
            previousGroupNavigationViewModel.Init(this.navigationState.InterviewId, navigationParams.TargetGroup, this.navigationState);
            listOfViewModels.Add(previousGroupNavigationViewModel);
        }
    }
}
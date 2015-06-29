using System;
using System.Collections;
using System.Linq;

using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
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

        void navigationState_OnGroupChanged(NavigationParams navigationParams)
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
            var offsetInsideOfAnchoredItemInPercentage = 0;

            if (navigationParams.AnchoredElementIdentity != null)
            {
                var item = listOfViewModels.Cast<IInterviewAnchoredEntity>()
                            .Where(x => x != null)
                            .FirstOrDefault(x => x.GetPositionOfAnchoredElement(navigationParams.AnchoredElementIdentity) >= 0);

                anchoreElementIndex = item != null ? listOfViewModels.IndexOf(item) : 0;

                var rosterViewModel = item as RosterViewModel;
                if (rosterViewModel!=null)
                {
                    var anchoredRosterInstance = item.GetPositionOfAnchoredElement(navigationParams.AnchoredElementIdentity);
                    if (rosterViewModel.Items.Count != 0)
                    {
                        offsetInsideOfAnchoredItemInPercentage = (100 * anchoredRosterInstance) / rosterViewModel.Items.Count;
                    }
                }
            }

            this.Items = listOfViewModels;
            messenger.Publish(new ScrollToAnchorMessage(this, anchoreElementIndex, offsetInsideOfAnchoredItemInPercentage));
        }

        private void AddToParentButton(IList listOfViewModels, NavigationParams navigationParams)
        {
            var previousGroupNavigationViewModel = Mvx.Resolve<PreviousGroupNavigationViewModel>();
            previousGroupNavigationViewModel.Init(this.navigationState.InterviewId, navigationParams.TargetGroup, this.navigationState);
            listOfViewModels.Add(previousGroupNavigationViewModel);
        }
    }
}
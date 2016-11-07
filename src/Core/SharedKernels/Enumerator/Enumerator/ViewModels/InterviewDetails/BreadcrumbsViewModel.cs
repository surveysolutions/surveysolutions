using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class BreadCrumbsViewModel : MvxNotifyPropertyChanged, 
        ILiteEventHandler<RosterInstancesTitleChanged>,IDisposable
    {
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly ISubstitutionService substitutionService;
        private NavigationState navigationState;
        private string interviewId;

        public BreadCrumbsViewModel(
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry,
            ISubstitutionService substitutionService)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry;
            this.substitutionService = substitutionService;
        }

        public void Init(string interviewId, NavigationState navigationState)
        {
            if (navigationState == null) throw new ArgumentNullException(nameof(navigationState));
            if (this.navigationState != null) throw new Exception($"ViewModel {typeof(BreadCrumbsViewModel)} already initialized");

            this.navigationState = navigationState;
            this.interviewId = interviewId;
            this.navigationState.ScreenChanged += this.OnScreenChanged;
            this.eventRegistry.Subscribe(this, interviewId);
        }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            if (this.Items == null)
                return;

            var interview = this.interviewRepository.Get(this.interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            foreach (var changedRosterInstance in @event.ChangedInstances)
            {
                var changedRosterInstanceIdentity = GetChangedRosterIdentity(changedRosterInstance);
                var breadCrumb = this.Items.SingleOrDefault(x => x.ItemId.Equals(changedRosterInstanceIdentity));

                if (breadCrumb != null)
                {
                    var groupTitle = questionnaire.GetGroupTitle(changedRosterInstance.RosterInstance.GroupId);
                    breadCrumb.ChangeText(this.GenerateRosterTitle(groupTitle, changedRosterInstance.Title));
                }
            }
        }

        private static Identity GetChangedRosterIdentity(ChangedRosterInstanceTitleDto changedRosterInstance)
        {
            var changedRosterInstanceIdentity = new Identity(changedRosterInstance.RosterInstance.GroupId,
                changedRosterInstance.RosterInstance.GetIdentity().RosterVector);
            return changedRosterInstanceIdentity;
        }

        void OnScreenChanged(ScreenChangedEventArgs eventArgs)
        {
            if (eventArgs.TargetScreen != ScreenType.Group)
            {
                this.Items = new ReadOnlyCollection<BreadCrumbItemViewModel>(new List<BreadCrumbItemViewModel>());
            }
            else
            {
                this.BuildBreadCrumbs(eventArgs.TargetGroup);
            }
        }

        private void BuildBreadCrumbs(Identity newGroupIdentity)
        {
            var interview = this.interviewRepository.Get(this.interviewId);
            var questionnaireModel = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            ReadOnlyCollection<Guid> parentIds = questionnaire.GetParentsStartingFromTop(newGroupIdentity.Id);

            var breadCrumbs = new List<BreadCrumbItemViewModel>();
            int metRosters = 0;
            foreach (Guid parentId in parentIds)
            {
                var groupTitle = questionnaireModel.GetGroupTitle(parentId);

                if (questionnaire.IsRosterGroup(parentId))
                {
                    metRosters++;
                    var itemRosterVector = newGroupIdentity.RosterVector.Shrink(metRosters);
                    var itemIdentity = new Identity(parentId, itemRosterVector);

                    var rosterInstance = interview.GetRoster(itemIdentity);
                        
                    var title = this.GenerateRosterTitle(groupTitle, rosterInstance.Title);
                    var breadCrumb = Mvx.Resolve<BreadCrumbItemViewModel>();
                    breadCrumb.Init(this.interviewId, itemIdentity, title, this.navigationState);
                    breadCrumbs.Add(breadCrumb);
                }
                else
                {
                    var itemIdentity = new Identity(parentId, newGroupIdentity.RosterVector.Shrink(metRosters));
                    var breadCrumb = Mvx.Resolve<BreadCrumbItemViewModel>();
                    breadCrumb.Init(this.interviewId, itemIdentity, groupTitle, this.navigationState);
                    breadCrumbs.Add(breadCrumb);
                }
            }

            this.Items.ForEach(x => x.Dispose());
            this.Items = new ReadOnlyCollection<BreadCrumbItemViewModel>(breadCrumbs);
        }

        private string GenerateRosterTitle(string groupTitle, string rosterInstanceTitle)
        {
            var rosterTitle = this.substitutionService.GenerateRosterName(groupTitle, rosterInstanceTitle);
            return rosterTitle;
        }

        private ReadOnlyCollection<BreadCrumbItemViewModel> items;
        public ReadOnlyCollection<BreadCrumbItemViewModel> Items
        {
            get { return this.items; }
            set { this.items = value; this.RaisePropertyChanged(); }
        }

        public void Dispose()
        {
            this.navigationState.ScreenChanged -= this.OnScreenChanged;
            this.eventRegistry.Unsubscribe(this);
            this.Items.ForEach(x => x.Dispose());
        }
    }
}
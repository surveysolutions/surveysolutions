using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using Identity = WB.Core.SharedKernels.DataCollection.Identity;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class BreadCrumbsViewModel : MvxNotifyPropertyChanged, 
        ILiteEventHandler<RosterInstancesTitleChanged>,IDisposable
    {
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireModelRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly ISubstitutionService substitutionService;
        private NavigationState navigationState;
        private string interviewId;

        public BreadCrumbsViewModel(
            IPlainQuestionnaireRepository questionnaireRepository,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireModelRepository,
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry,
            ISubstitutionService substitutionService)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.questionnaireModelRepository = questionnaireModelRepository;
            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry;
            this.substitutionService = substitutionService;
        }

        public void Init(string interviewId, NavigationState navigationState)
        {
            if (navigationState == null) throw new ArgumentNullException("navigationState");
            if (this.navigationState != null) throw new Exception("ViewModel already initialized");

            this.navigationState = navigationState;
            this.interviewId = interviewId;
            this.navigationState.ScreenChanged += this.OnScreenChanged;
            this.eventRegistry.Subscribe(this, interviewId);
        }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            var interview = this.interviewRepository.Get(this.interviewId);
            var questionnaire = this.questionnaireModelRepository.GetById(interview.QuestionnaireId);

            foreach (var changedRosterInstance in @event.ChangedInstances)
            {
                var changedRosterInstanceIdentity = GetChangedRosterIdentity(changedRosterInstance);
                var breadCrumb = this.Items.SingleOrDefault(x => x.ItemId.Equals(changedRosterInstanceIdentity));

                if (breadCrumb != null)
                {
                    var groupTitle = questionnaire.GroupsWithFirstLevelChildrenAsReferences[changedRosterInstance.RosterInstance.GroupId].Title;
                    breadCrumb.Text = this.GenerateRosterTitle(groupTitle, changedRosterInstance.Title);
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
            var questionnaireModel = this.questionnaireModelRepository.GetById(interview.QuestionnaireId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);

            ReadOnlyCollection<Guid> parentIds = questionnaire.GetParentsStartingFromTop(newGroupIdentity.Id);

            var breadCrumbs = new List<BreadCrumbItemViewModel>();
            int metRosters = 0;
            foreach (Guid parentId in parentIds)
            {
                var groupTitle = questionnaireModel.GroupsWithFirstLevelChildrenAsReferences[parentId].Title;

                if (questionnaire.IsRosterGroup(parentId))
                {
                    metRosters++;
                    var itemRosterVector = newGroupIdentity.RosterVector.Shrink(metRosters);
                    var itemIdentity = new Identity(parentId, itemRosterVector);

                    var rosterInstance = interview.GetRoster(itemIdentity);
                        
                    var title = this.GenerateRosterTitle(groupTitle, rosterInstance.Title);
                    breadCrumbs.Add(new BreadCrumbItemViewModel(this.navigationState)
                    {
                        Text = title,
                        ItemId = itemIdentity
                    });
                }
                else
                {
                    var itemId = new Identity(parentId, newGroupIdentity.RosterVector.Shrink(metRosters));
                    breadCrumbs.Add(new BreadCrumbItemViewModel(this.navigationState)
                    {
                        ItemId = itemId,
                        Text = groupTitle + " / "
                    });
                }
            }

            this.Items = new ReadOnlyCollection<BreadCrumbItemViewModel>(breadCrumbs);
        }

        private string GenerateRosterTitle(string groupTitle, string rosterInstanceTitle)
        {
            var rosterTitle = this.substitutionService.GenerateRosterName(groupTitle, rosterInstanceTitle);
            return rosterTitle + " / ";
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
            this.eventRegistry.Unsubscribe(this, interviewId);
        }
    }
}
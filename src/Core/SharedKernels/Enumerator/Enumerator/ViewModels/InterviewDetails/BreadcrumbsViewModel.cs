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
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Services;
using Identity = WB.Core.SharedKernels.DataCollection.Identity;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class BreadCrumbsViewModel : MvxNotifyPropertyChanged, 
        ILiteEventHandler<RosterInstancesTitleChanged>
    {
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly ISubstitutionService substitutionService;
        private NavigationState navigationState;
        private string interviewId;

        public BreadCrumbsViewModel(IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
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
            if (navigationState == null) throw new ArgumentNullException("navigationState");
            if (this.navigationState != null) throw new Exception("ViewModel already initialized");

            this.navigationState = navigationState;
            this.interviewId = interviewId;
            this.navigationState.GroupChanged += this.navigationState_OnGroupChanged;
            this.eventRegistry.Subscribe(this, interviewId);
        }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            var interview = this.interviewRepository.Get(this.interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

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
                changedRosterInstance.RosterInstance.OuterRosterVector.Concat(
                    changedRosterInstance.RosterInstance.RosterInstanceId.ToEnumerable()).ToArray());
            return changedRosterInstanceIdentity;
        }

        void navigationState_OnGroupChanged(GroupChangedEventArgs navigationParams)
        {
            if (navigationParams.ScreenType != ScreenType.Group)
            {
                this.Items = new ReadOnlyCollection<BreadCrumbItemViewModel>(new List<BreadCrumbItemViewModel>());
            }
            else
            {
                this.BuildBreadCrumbs(navigationParams.TargetGroup);
            }
        }

        private void BuildBreadCrumbs(Identity newGroupIdentity)
        {
            var interview = this.interviewRepository.Get(this.interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

            List<GroupReferenceModel> parentItems = questionnaire.Parents[newGroupIdentity.Id];

            var breadCrumbs = new List<BreadCrumbItemViewModel>();
            int metRosters = 0;
            foreach (var reference in parentItems)
            {
                var groupTitle = questionnaire.GroupsWithFirstLevelChildrenAsReferences[reference.Id].Title;

                if (reference.IsRoster)
                {
                    metRosters++;
                    var itemRosterVector = newGroupIdentity.RosterVector.Take(metRosters).ToArray();
                    var itemIdentity = new Identity(reference.Id, itemRosterVector);

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
                    var itemId = new Identity(reference.Id, newGroupIdentity.RosterVector.Take(metRosters).ToArray());
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
    }
}
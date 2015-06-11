using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using Identity = WB.Core.SharedKernels.DataCollection.Identity;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class BreadcrumbsViewModel : MvxNotifyPropertyChanged, 
        ILiteEventHandler<RosterInstancesTitleChanged>
    {
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry eventRegistry;
        private NavigationState navigationState;
        private string interviewId;

        public BreadcrumbsViewModel(IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry;
        }

        public void Init(string interviewId, NavigationState navigationState)
        {
            if (navigationState == null) throw new ArgumentNullException("navigationState");
            if (this.navigationState != null) throw new Exception("ViewModel already initialized");

            this.navigationState = navigationState;
            this.interviewId = interviewId;
            this.navigationState.OnGroupChanged += navigationState_OnGroupChanged;
            this.eventRegistry.Subscribe(this);
        }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            var interview = this.interviewRepository.Get(this.interviewId);
            var questionnaire = questionnaireRepository.GetById(interview.QuestionnaireId);

            var lastBreadCrumb = Items.Last();
            foreach (var changedRosterInstance in @event.ChangedInstances)
            {
                var changedRosterInstanceIdentity = GetChangedRosterIdentity(changedRosterInstance);
                if (changedRosterInstanceIdentity.Equals(lastBreadCrumb.ItemId))
                {
                    var groupTitle = questionnaire.GroupsWithFirstLevelChildrenAsReferences[lastBreadCrumb.ItemId.Id].Title;
                    lastBreadCrumb.Text = GenerateRosterTitle(groupTitle, changedRosterInstance.Title);
                    break;
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

        void navigationState_OnGroupChanged(Identity newGroupIdentity)
        {
            this.BuildBreadCrumbs(newGroupIdentity);
        }

        private void BuildBreadCrumbs(Identity newGroupIdentity)
        {
            var interview = this.interviewRepository.Get(this.interviewId);
            var questionnaire = questionnaireRepository.GetById(interview.QuestionnaireId);

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

                    var rosterInstance =
                        (InterviewRoster)
                            interview.Groups[ConversionHelper.ConvertIdAndRosterVectorToString(reference.Id, itemRosterVector)];
                    var title = GenerateRosterTitle(groupTitle, rosterInstance.Title);
                    breadCrumbs.Add(new BreadCrumbItemViewModel(navigationState)
                    {
                        Text = title,
                        ItemId = itemIdentity
                    });
                }
                else
                {
                    var itemId = new Identity(reference.Id, newGroupIdentity.RosterVector.Take(metRosters).ToArray());
                    breadCrumbs.Add(new BreadCrumbItemViewModel(navigationState)
                    {
                        ItemId = itemId,
                        Text = groupTitle + " / "
                    });
                }
            }

            this.Items = new ReadOnlyCollection<BreadCrumbItemViewModel>(breadCrumbs);
        }

        private static string GenerateRosterTitle(string groupTitle, string changedRosterInstanceIdentity)
        {
            return string.Format("{0} - {1} / ", groupTitle, changedRosterInstanceIdentity);
        }

        private ReadOnlyCollection<BreadCrumbItemViewModel> items;
        public ReadOnlyCollection<BreadCrumbItemViewModel> Items
        {
            get { return items; }
            set { items = value; RaisePropertyChanged(); }
        }
    }
}
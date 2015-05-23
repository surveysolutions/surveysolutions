using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class BreadcrumbsViewModel : MvxNotifyPropertyChanged
    {
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefullInterviewRepository interviewRepository;
        private NavigationState navigationState;
        private string interviewId;

        public BreadcrumbsViewModel(IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefullInterviewRepository interviewRepository)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
        }

        public void Init(string interviewId, NavigationState navigationState)
        {
            if (navigationState == null) throw new ArgumentNullException("navigationState");
            if (this.navigationState != null) throw new Exception("ViewModel already initialized");

            this.navigationState = navigationState;
            this.interviewId = interviewId;
            this.navigationState.OnGroupChanged += navigationState_OnGroupChanged;
        }

        void navigationState_OnGroupChanged(Identity newGroupIdentity)
        {
            var interview = this.interviewRepository.Get(this.interviewId);
            var questionnaire = questionnaireRepository.GetById(interview.QuestionnaireId);

            List<QuestionnaireReferenceModel> parentItems = new List<QuestionnaireReferenceModel>(questionnaire.GroupParents[newGroupIdentity.Id]);
            parentItems.Reverse();

            var breadCrumbs = new List<BreadCrumbItemViewModel>();
            int metRosters = 0;
            foreach (var reference in parentItems)
            {
                var groupTitle = questionnaire.GroupsWithoutNestedChildren[reference.Id].Title;

                if (reference.ModelType == typeof (RosterModel))
                {
                    metRosters++;
                    var itemRosterVector = newGroupIdentity.RosterVector.Take(metRosters).ToArray();
                    var itemIdentity = new Identity(reference.Id, itemRosterVector);
                    var itemRosterInstanceId = ConversionHelper.ConvertIdAndRosterVectorToString(reference.Id, itemRosterVector);

                    var rosterInstance = (InterviewRoster) interview.Groups[itemRosterInstanceId];
                    var title = string.Format("{0} - {1} / ", groupTitle, rosterInstance.Title);
                    breadCrumbs.Add(new BreadCrumbItemViewModel(navigationState)
                    {
                        Text = title,
                        ItemId = itemIdentity
                    });
                }
                else
                {
                    var itemId = new Identity(reference.Id, newGroupIdentity.RosterVector.Take(metRosters).ToArray());
                    breadCrumbs.Add(new BreadCrumbItemViewModel(navigationState) {
                        ItemId = itemId,
                        Text = groupTitle + " / "
                    });
                }
            }

            GroupModel newGroup = questionnaire.GroupsWithoutNestedChildren[newGroupIdentity.Id];
            var lastGroupTitle = newGroup.Title;
            if (newGroup is RosterModel)
            {
                var itemRosterInstanceId = ConversionHelper.ConvertIdAndRosterVectorToString(newGroupIdentity.Id,
                    newGroupIdentity.RosterVector);

                var rosterInstance = (InterviewRoster) interview.Groups[itemRosterInstanceId];
                breadCrumbs.Add(new BreadCrumbItemViewModel(navigationState)
                {
                    Text = string.Format("{0} - {1} / ", lastGroupTitle, rosterInstance.Title),
                    ItemId = newGroupIdentity
                });
            }
            else
            {
                breadCrumbs.Add(new BreadCrumbItemViewModel(this.navigationState)
                {
                    ItemId = newGroupIdentity,
                    Text = lastGroupTitle + " / "
                });
            }

            Debug.WriteLine(" BreadCrumbs - {0}", string.Join(" | ", breadCrumbs.Select(x => x.ItemId + " * " + x.Text)));

            this.Items = new ReadOnlyCollection<BreadCrumbItemViewModel>(breadCrumbs);
        }

        private ReadOnlyCollection<BreadCrumbItemViewModel> items;

        public ReadOnlyCollection<BreadCrumbItemViewModel> Items
        {
            get { return items; }
            set { items = value; RaisePropertyChanged(); }
        }
    }

    public class BreadCrumbItemViewModel
    {
        private readonly NavigationState navigationState;

        public BreadCrumbItemViewModel(NavigationState navigationState)
        {
            this.navigationState = navigationState;
        }

        public string Text { get; set; }
        public Identity ItemId { get; set; }

        public IMvxCommand NavigateCommand
        {
            get
            {
                return new MvxCommand(() =>
                {
                    Debug.WriteLine("Navigate to {0}", ItemId);
                    navigationState.NavigateTo(ItemId);
                });
            }
        }
    }
}
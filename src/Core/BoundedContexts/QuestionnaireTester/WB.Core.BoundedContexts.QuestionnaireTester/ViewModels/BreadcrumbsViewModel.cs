using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class BreadcrumbsViewModel : MvxNotifyPropertyChanged, 
        ILiteEventHandler<RosterInstancesTitleChanged>
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

        public void Handle(RosterInstancesTitleChanged @event)
        {
            this.BuldBreadCrumbs(this.navigationState.CurrentGroup);
        }

        void navigationState_OnGroupChanged(Identity newGroupIdentity)
        {
            this.BuldBreadCrumbs(newGroupIdentity);
        }

        private void BuldBreadCrumbs(Identity newGroupIdentity)
        {
            var interview = this.interviewRepository.Get(this.interviewId);
            var questionnaire = questionnaireRepository.GetById(interview.QuestionnaireId);

            List<QuestionnaireReferenceModel> parentItems = GetBreadCrumbsReferencesFromQuestionnaire(newGroupIdentity, questionnaire);

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

                    var rosterInstance =
                        (InterviewRoster)
                            interview.Groups[ConversionHelper.ConvertIdAndRosterVectorToString(reference.Id, itemRosterVector)];
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
                    breadCrumbs.Add(new BreadCrumbItemViewModel(navigationState)
                    {
                        ItemId = itemId,
                        Text = groupTitle + " / "
                    });
                }
            }

            this.Items = new ReadOnlyCollection<BreadCrumbItemViewModel>(breadCrumbs);
        }

        private static List<QuestionnaireReferenceModel> GetBreadCrumbsReferencesFromQuestionnaire(Identity newGroupIdentity,
            QuestionnaireModel questionnaire)
        {
            List<QuestionnaireReferenceModel> parentItems =
                new List<QuestionnaireReferenceModel>(questionnaire.GroupParents[newGroupIdentity.Id]);
            parentItems.Reverse();
            parentItems.Add(new QuestionnaireReferenceModel
            {
                Id = newGroupIdentity.Id,
                ModelType = questionnaire.GroupsWithoutNestedChildren[newGroupIdentity.Id].GetType()
            });
            return parentItems;
        }

        private ReadOnlyCollection<BreadCrumbItemViewModel> items;

        public ReadOnlyCollection<BreadCrumbItemViewModel> Items
        {
            get { return items; }
            set { items = value; RaisePropertyChanged(); }
        }
    }
}
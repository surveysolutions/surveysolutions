using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public BreadcrumbsViewModel(IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefullInterviewRepository interviewRepository)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
        }

        public void Init(NavigationState navigationState)
        {
            if (navigationState == null) throw new ArgumentNullException("navigationState");
            if (this.navigationState != null) throw new Exception("ViewModel already initialized");

            this.navigationState = navigationState;
            this.navigationState.OnGroupChanged += navigationState_OnGroupChanged;
        }

        void navigationState_OnGroupChanged(Identity newGroupIdentity)
        {
            var questionnaire = questionnaireRepository.GetById(this.navigationState.QuestionnaireId);
            List<QuestionnaireReferenceModel> parentItems = questionnaire.GroupParents[newGroupIdentity.Id];
            var breadCrumbs = parentItems.Select(x => new BreadCrumbItemViewModel(this.navigationState)
            {
                ItemId = new Identity(x.Id, new decimal[] {}),
                Text = questionnaire.GroupsWithoutNestedChildren[x.Id].Title + " / "
            }).ToList();
            breadCrumbs.Reverse();
            breadCrumbs.Add(new BreadCrumbItemViewModel(this.navigationState)
            {
                ItemId = newGroupIdentity,
                Text = questionnaire.GroupsWithoutNestedChildren[newGroupIdentity.Id].Title + " / "
            });

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
                return new MvxCommand(() => navigationState.NavigateTo(ItemId));
            }
        }
    }
}
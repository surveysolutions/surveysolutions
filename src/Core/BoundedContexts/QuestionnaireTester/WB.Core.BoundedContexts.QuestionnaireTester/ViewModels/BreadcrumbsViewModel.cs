using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class BreadcrumbsViewModel : MvxNotifyPropertyChanged
    {
        private NavigationState navigationState;

        public void Init(NavigationState navigationState)
        {
            if(navigationState == null) throw new ArgumentNullException("navigationState");
            if (this.navigationState != null) throw new Exception("ViewModel already initialized");

            this.navigationState = navigationState;
            this.navigationState.OnGroupChanged += navigationState_OnGroupChanged;

            this.Items = new ReadOnlyCollection<BreadCrumbItemViewModel>(new List<BreadCrumbItemViewModel>
            {
                new BreadCrumbItemViewModel
                {
                    Text =  "Group 1 bla bla"
                },
                new BreadCrumbItemViewModel
                {
                    Text = "Lorem ipsum groupu"
                },
                new BreadCrumbItemViewModel
                {
                    Text = "Some long long "
                },
            });
        }

        void navigationState_OnGroupChanged(Identity newGroupIdentity)
        {
            
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
        public string Text { get; set; }
        public Identity ItemId { get; set; }
    }
}
﻿using Microsoft.Practices.ServiceLocation;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    internal class SideBarSectionViewModelFactory : ISideBarSectionViewModelsFactory
    {
        readonly IServiceLocator serviceLocator;

        public SideBarSectionViewModelFactory(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public SideBarSectionViewModel BuildSectionItem(
            SideBarSectionsViewModel root, 
            SideBarSectionViewModel sectionToAddTo, 
            Identity enabledSubgroupIdentity, 
            NavigationState navigationState, 
            string interviewId)
        {
            var sideBarItem = serviceLocator.GetInstance<SideBarSectionViewModel>();
            sideBarItem.Init(interviewId, enabledSubgroupIdentity, root, sectionToAddTo, navigationState);
            return sideBarItem;
        }
    }

    public interface ISideBarSectionViewModelsFactory
    {
        SideBarSectionViewModel BuildSectionItem(SideBarSectionsViewModel root, SideBarSectionViewModel sectionToAddTo, Identity enabledSubgroupIdentity, NavigationState navigationState, string interviewId);
    }
}
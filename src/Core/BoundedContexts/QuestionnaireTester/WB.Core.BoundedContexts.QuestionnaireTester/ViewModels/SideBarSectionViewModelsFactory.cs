using System.Linq;
using Cirrious.CrossCore;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    internal class SideBarSectionViewModelsFactory : ISideBarSectionViewModelsFactory
    {
        public SideBarSectionViewModel BuildSectionItem(SideBarSectionViewModel sectionToAddTo,
            GroupModel model,
            Identity enabledSubgroupIdentity,
            IStatefulInterview interview,
            ISubstitutionService substitutionService,
            NavigationState navigationState)
        {
            var sideBarItem = Mvx.Create<SideBarSectionViewModel>();
            sideBarItem.Init(navigationState);

            sideBarItem.Parent = sectionToAddTo;
            sideBarItem.Title = model.Title;
            sideBarItem.SectionIdentity = enabledSubgroupIdentity;
            sideBarItem.HasChildren = interview.GetEnabledSubgroups(enabledSubgroupIdentity).Any();
            sideBarItem.NodeDepth = sideBarItem.UnwrapReferences(x => x.Parent).Count() - 1;
            if (model is RosterModel)
            {
                string rosterTitle = interview.GetRosterTitle(enabledSubgroupIdentity);
                sideBarItem.Title = substitutionService.GenerateRosterName(model.Title, rosterTitle);
            }
            return sideBarItem;
        }
    }

    public interface ISideBarSectionViewModelsFactory
    {
        SideBarSectionViewModel BuildSectionItem(SideBarSectionViewModel sectionToAddTo,
            GroupModel model,
            Identity enabledSubgroupIdentity,
            IStatefulInterview interview,
            ISubstitutionService substitutionService,
            NavigationState navigationState);
    }
}
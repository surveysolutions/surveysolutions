using System;

using Cirrious.MvvmCross.Plugins.Messenger;

using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.Infrastructure.PlainStorage;

using it = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.SectionsViewModelTests
{
    public class SectionsViewModelTestContext
    {
        protected static SectionsViewModel CreateSectionsViewModel(
            IMvxMessenger messenger = null,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository = null)
        {
            return new SectionsViewModel(
                messenger ?? Mock.Of<IMvxMessenger>(),
                questionnaireRepository ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>());
        }

        protected static NavigationState CreateNavigationState()
        {
            return new NavigationState();
        }
        protected static GroupsHierarchyModel CreateGroupsHierarchyModel(Guid id, string title)
        {
            return new GroupsHierarchyModel
                   {
                       Id = id,
                       Title = title,
                       IsRoster = false
                   };
        }
    }
}
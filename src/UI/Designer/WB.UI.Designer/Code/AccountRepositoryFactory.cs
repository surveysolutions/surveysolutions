using Main.Core.View;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Commanding.ServiceModel;
using WB.UI.Designer.Providers.CQRS;
using WB.UI.Designer.Providers.CQRS.Accounts.View;

namespace WB.UI.Designer
{
    using WB.UI.Shared.Web.MembershipProvider.Accounts;

    public static class AccountRepositoryFactory
    {
        private static ICommandService commandService
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ICommandService>();
            }
        }

        private static IViewFactory<AccountListViewInputModel, AccountListView> accountListViewFactory
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IViewFactory<AccountListViewInputModel, AccountListView>>();
            }
        }

        private static IViewFactory<AccountViewInputModel, AccountView> accountViewFactory
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IViewFactory<AccountViewInputModel, AccountView>>();
            }
        }

        public static IAccountRepository CreateRepository()
        {
            return new CQRSAccountRepository(commandService, accountListViewFactory, accountViewFactory)
            {
                IsUniqueEmailRequired = MembershipProviderSettings.Instance.RequiresUniqueEmail
            };
        }
    }
}
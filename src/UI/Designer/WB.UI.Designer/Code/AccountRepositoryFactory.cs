using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Shared.Web.MembershipProvider.Settings;

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

        private static IAccountListViewFactory accountListViewFactory
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IAccountListViewFactory>();
            }
        }

        private static IAccountViewFactory accountViewFactory
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IAccountViewFactory>();
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
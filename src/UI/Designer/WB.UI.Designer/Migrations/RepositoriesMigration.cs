using System;
using Autofac;
using FluentMigrator;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Designer.Migrations
{
    public abstract class RepositoriesMigration : Migration
    {
        private readonly IServiceLocator serviceLocator;

        protected RepositoriesMigration(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        protected void RunActionInScope(Action<IServiceLocator> action)
        {
            var lifetimeScope = serviceLocator.GetInstance<ILifetimeScope>();
            using (var scope = lifetimeScope.BeginLifetimeScope())
            {
                var serviceLocatorLocal = scope.Resolve<IServiceLocator>();

                action(serviceLocatorLocal);

                serviceLocatorLocal.GetInstance<IUnitOfWork>().AcceptChanges();
            }
        }
    }
}

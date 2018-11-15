using Autofac;

namespace WB.Core.Infrastructure.Modularity.Autofac
{
    public class UnitOfWorkScopeManager
    {
        private static ILifetimeScope rootScope;

        public static void SetScopeAdapter(ILifetimeScope scope)
        {
            rootScope = scope;
        }

        public static Scope BeginScope()
        {
            var lifetimeScope = rootScope.BeginLifetimeScope(AutofacServiceLocatorConstants.UnitOfWorkScope);
            return new Scope(lifetimeScope);
        }

        public static void EndScope(Scope scope)
        {
            scope.Dispose();
        }
    }
}
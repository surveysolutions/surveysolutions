using System;
using System.Linq;
using System.Threading;
using Ninject;
using Ninject.Activation;
using Ninject.Syntax;
using Ninject.Web.Common;
using WB.Infrastructure.Native.Threading;

namespace WB.Infrastructure.Native.Storage
{
    internal static class NinjectExtensions
    {
        public static bool HasBinding<T>(this IKernel kernel) => kernel.GetBindings(typeof(T)).Any();

        public static IBindingNamedWithOrOnSyntax<T> InIsolatedThreadScopeOrRequestScopeOrThreadScope<T>(this IBindingInSyntax<T> syntax)
        {
            var isolatedThreadScopeCallback = GetIsolatedThreadScopeCallback();
            var requestScopeCallback = GetScopeCallback(syntax.InRequestScope());
            var threadScopeCallback = GetScopeCallback(syntax.InThreadScope());

            return syntax.InScope(context
                => isolatedThreadScopeCallback.Invoke(context)
                ?? requestScopeCallback.Invoke(context)
                ?? threadScopeCallback.Invoke(context));
        }

        private static Func<IContext, object> GetScopeCallback<T>(IBindingNamedWithOrOnSyntax<T> requestScopeSyntax)
        {
            return requestScopeSyntax.BindingConfiguration.ScopeCallback;
        }

        private static Func<IContext, object> GetIsolatedThreadScopeCallback()
        {
            return context => Thread.CurrentThread.AsIsolatedThread();
        }
    }
}
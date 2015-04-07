using System;
using Ninject.Activation;
using Ninject.Syntax;
using Ninject.Web.Common;

namespace WB.Core.Infrastructure.Storage
{
    internal static class NinjectExtensions
    {
        public static IBindingNamedWithOrOnSyntax<T> InRequestOrThreadScope<T>(this IBindingInSyntax<T> syntax)
        {
            var requestScopeCallback = GetScopeCallback(syntax.InRequestScope());
            var threadScopeCallback = GetScopeCallback(syntax.InThreadScope());

            return syntax.InScope(context => requestScopeCallback.Invoke(context) ?? threadScopeCallback.Invoke(context));
        }

        private static Func<IContext, object> GetScopeCallback<T>(IBindingNamedWithOrOnSyntax<T> requestScopeSyntax)
        {
            return requestScopeSyntax.BindingConfiguration.ScopeCallback;
        }
    }
}
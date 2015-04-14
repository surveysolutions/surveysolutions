using System;
using System.Collections.Generic;
using System.Threading;
using Ninject;
using Ninject.Activation;
using Ninject.Syntax;
using Ninject.Web.Common;

namespace WB.Core.Infrastructure.Storage
{
    public static class IsolatedThreadExtensions
    {
        public static Thread AsIsolatedThread(this Thread thread)
        {
            return IsolatedThreadManager.IsIsolated(thread) ? thread : null;
        }
    }

    public static class IsolatedThreadManager
    {
        private static readonly HashSet<int> IsolatedThreads = new HashSet<int>();

        public static void MarkCurrentThreadAsIsolated()
        {
            IsolatedThreads.Add(Thread.CurrentThread.ManagedThreadId);
        }

        public static void ReleaseCurrentThreadFromIsolation()
        {
            IsolatedThreads.Remove(Thread.CurrentThread.ManagedThreadId);
        }

        public static bool IsIsolated(Thread thread)
        {
            return IsolatedThreads.Contains(thread.ManagedThreadId);
        }
    }

    internal static class NinjectExtensions
    {
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
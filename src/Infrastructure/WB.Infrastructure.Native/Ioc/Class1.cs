using System;
using System.Threading;
using Ninject.Activation;
using Ninject.Infrastructure.Disposal;
using Ninject.Syntax;

namespace WB.Infrastructure.Native.Ioc
{
    /// <summary>
    /// Ninject ambient scope.
    /// </summary>
    /// <remarks>
    /// Nested scopes are supported, but multi-threading in nested scopes is not supported. https://mono.software/2016/04/21/Ninject-ambient-scope-and-deterministic-dispose/
    /// </remarks>
    public class NinjectAmbientScope : NinjectDisposableScope
    {
        #region Fields

        private static readonly AsyncLocal<NinjectAmbientScope> scopeHolder = new AsyncLocal<NinjectAmbientScope>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NinjectAmbientScope" /> class.
        /// </summary>
        public NinjectAmbientScope()
        {
            scopeHolder.Value = this;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the current ambient scope.
        /// </summary>
        public static NinjectAmbientScope Current
        {
            get { return scopeHolder.Value; }
        }

        #endregion Properties
    }

    /// <summary>
    /// Defines ambient scope extension methods.
    /// </summary>
    public static class NinjectAmbientScopeExtensions
    {
        #region Methods

        /// <summary>
        /// Sets the scope to ambient scope.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="syntax">The syntax.</param>
        /// <returns>The syntax to define more information.</returns>
        public static IBindingNamedWithOrOnSyntax<T> InAmbientScope<T>(this IBindingInSyntax<T> syntax)
        {
            return syntax.InScope(GetAmbientScope);
        }

        private static object GetAmbientScope(IContext ctx)
        {
            var scope = NinjectAmbientScope.Current;
            if (scope != null)
            {
                return scope;
            }
            throw new ApplicationException("No ambient scope defined");
        }

        #endregion Methods
    }

    /// <summary>
    /// Ninject disposable scope
    /// </summary>
    public class NinjectDisposableScope : IDisposable, INotifyWhenDisposed
    {
        #region Events

        public event EventHandler Disposed;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NinjectDisposableScope" /> class.
        /// </summary>
        public NinjectDisposableScope()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether the scope is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get;
            private set;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            this.IsDisposed = true;
            if (this.Disposed != null)
            {
                this.Disposed(this, EventArgs.Empty);
            }
        }

        #endregion IDisposable Members
    }
}

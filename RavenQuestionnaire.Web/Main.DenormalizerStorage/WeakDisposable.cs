// -----------------------------------------------------------------------
// <copyright file="DisposeNotifiesObject.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Main.DenormalizerStorage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class WeakDisposable<T> : IDisposable where T : class
    {
        public WeakDisposable(T data, Guid key)
        {
            Data = data;
            Key = key;
        }

        public event EventHandler BefoureFinalize;

        public T Data { get;private set; }
        public Guid Key { get; private set; }
        #region Implementation of IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

         // finalizer simply calls Dispose(false)
        ~WeakDisposable()
        {
           
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                var handler = this.BefoureFinalize;
                if (handler != null)
                    handler(this, EventArgs.Empty);
                // if this is a dispose call dispose on all state you
                // hold, and take yourself off the Finalization queue.
                if (disposing)
                {
                    this.Data = null;
                }


                this.disposed = true;
            }
        }
        private bool disposed = false;
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Text;

using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid.Platform;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.CustomServices.UserInteraction
{
    public class UserInteractionService : IUserInteractionService
    {
        private static readonly HashSet<Guid> userInteractions = new HashSet<Guid>();
        private static readonly object UserInteractionsLock = new object();
        private static TaskCompletionSource<object> userInteractionsAwaiter = null;

        protected Activity CurrentActivity
        {
            get
            {
                return Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity;
            }
        }

        public Task<bool> ConfirmAsync(
            string message,
            string title = "",
            string okButton = "OK",
            string cancelButton = "Cancel")
        {
            var tcs = new TaskCompletionSource<bool>();
            this.Confirm(message, k => tcs.TrySetResult(k), title, okButton, cancelButton);
            return tcs.Task;
        }

        public Task AlertAsync(string message, string title = "", string okButton = "OK")
        {
            var tcs = new TaskCompletionSource<object>();
            this.Alert(message, () => tcs.TrySetResult(null), title, okButton);
            return tcs.Task;
        }

        public Task WaitPendingUserInteractionsAsync()
        {
            lock (UserInteractionsLock)
            {
                if (userInteractions.Count == 0)
                    return Task.FromResult(null as object);

                if (userInteractionsAwaiter == null)
                {
                    userInteractionsAwaiter = new TaskCompletionSource<object>();
                }

                return userInteractionsAwaiter.Task;
            }
        }

        private void Confirm(
            string message,
            Action<bool> answer,
            string title = null,
            string okButton = "OK",
            string cancelButton = "Cancel")
        {
            this.ConfirmImpl(message, answer, title, okButton, cancelButton);
        }

        private void Alert(string message, Action done = null, string title = "", string okButton = "OK")
        {
            this.AlertImpl(message, done, title, okButton);
        }

        private void ConfirmImpl(string message, Action<bool> callback, string title, string okButton, string cancelButton)
        {
            var userInteractionId = Guid.NewGuid();

            try
            {
                HandleDialogOpen(userInteractionId);

                Application.SynchronizationContext.Post(
                    ignored =>
                    {
                        if (this.CurrentActivity == null)
                        {
                            HandleDialogClose(userInteractionId);
                            return;
                        }

                        new AlertDialog.Builder(this.CurrentActivity)
                            .SetMessage(Html.FromHtml(message))
                            .SetTitle(Html.FromHtml(title))
                            .SetPositiveButton(okButton, delegate { HandleDialogClose(userInteractionId, () => { if (callback != null) callback(true); }); })
                            .SetNegativeButton(cancelButton, delegate { HandleDialogClose(userInteractionId, () => { if (callback != null) callback(false); }); })
                            .SetCancelable(false)
                            .Show();
                    },
                    null);
            }
            catch
            {
                HandleDialogClose(userInteractionId);
                throw;
            }
        }

        private void AlertImpl(string message, Action callback, string title, string okButton)
        {
            var userInteractionId = Guid.NewGuid();

            try
            {
                HandleDialogOpen(userInteractionId);

                Application.SynchronizationContext.Post(
                    ignored =>
                    {
                        if (this.CurrentActivity == null)
                        {
                            HandleDialogClose(userInteractionId);
                            return;
                        }

                        new AlertDialog.Builder(this.CurrentActivity)
                            .SetMessage(Html.FromHtml(message))
                            .SetTitle(Html.FromHtml(title))
                            .SetPositiveButton(okButton, delegate { HandleDialogClose(userInteractionId, () => { if (callback != null) callback(); }); })
                            .SetCancelable(false)
                            .Show();
                    },
                    null);
            }
            catch
            {
                HandleDialogClose(userInteractionId);
                throw;
            }
        }

        private static void HandleDialogOpen(Guid userInteractionId)
        {
            lock (UserInteractionsLock)
            {
                userInteractions.Add(userInteractionId);
            }
        }

        private static void HandleDialogClose(Guid userInteractionId, Action callback = null)
        {
            try
            {
                if (callback != null)
                {
                    callback.Invoke();
                }
            }
            finally
            {
                lock (UserInteractionsLock)
                {
                    userInteractions.Remove(userInteractionId);

                    if (userInteractions.Count == 0)
                    {
                        if (userInteractionsAwaiter != null)
                        {
                            userInteractionsAwaiter.TrySetResult(new object());
                            userInteractionsAwaiter = null;
                        }
                    }
                }
            }
        }
    }
}
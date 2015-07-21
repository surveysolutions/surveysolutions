using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid.Platform;
using WB.Core.BoundedContexts.Tester.Services;

namespace WB.UI.Tester.CustomServices.UserInteraction
{
    public class UserInteractionService : IUserInteraction, IUserInteractionAwaiter
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

        public void Confirm(
            string message,
            Action<bool> answer,
            string title = null,
            string okButton = "OK",
            string cancelButton = "Cancel")
        {
            this.ConfirmImpl(message, answer, title, okButton, cancelButton);
        }

        public Task<bool> ConfirmAsync(
            string message,
            string title = "",
            string okButton = "OK",
            string cancelButton = "Cancel")
        {
            var tcs = new TaskCompletionSource<bool>();
            Confirm(message, tcs.SetResult, title, okButton, cancelButton);
            return tcs.Task;
        }

        public void Alert(string message, Action done = null, string title = "", string okButton = "OK")
        {
            this.AlertImpl(message, done, title, okButton);
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
                            .SetMessage(message)
                            .SetTitle(title)
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
                            .SetMessage(message)
                            .SetTitle(title)
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
                            userInteractionsAwaiter.SetResult(new object());
                            userInteractionsAwaiter = null;
                        }
                    }
                }
            }
        }
    }
}
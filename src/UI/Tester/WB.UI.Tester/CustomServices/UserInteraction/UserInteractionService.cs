using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Widget;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid.Platform;
using WB.Core.BoundedContexts.Tester.Services;

namespace WB.UI.Tester.CustomServices.UserInteraction
{
    public class UserInteractionService : IUserInteraction, IUserInteractionAwaiter
    {
        private static HashSet<Guid> userInteractions = new HashSet<Guid>();
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
            Action okClicked,
            string title = null,
            string okButton = "OK",
            string cancelButton = "Cancel")
        {
            Confirm(message, confirmed => { if (confirmed) okClicked(); }, title, okButton, cancelButton);
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

        public void ConfirmThreeButtons(
            string message,
            Action<ConfirmThreeButtonsResponse> answer,
            string title = null,
            string positive = "Yes",
            string negative = "No",
            string neutral = "Maybe")
        {
            this.ConfirmThreeButtonsImpl(message, answer, title, positive, negative, neutral);
        }

        public Task<ConfirmThreeButtonsResponse> ConfirmThreeButtonsAsync(
            string message,
            string title = null,
            string positive = "Yes",
            string negative = "No",
            string neutral = "Maybe")
        {
            var tcs = new TaskCompletionSource<ConfirmThreeButtonsResponse>();
            ConfirmThreeButtons(message, tcs.SetResult, title, positive, negative, neutral);
            return tcs.Task;
        }

        public void Alert(string message, Action done = null, string title = "", string okButton = "OK")
        {
            this.AlertImpl(message, done, title, okButton);
        }

        public Task AlertAsync(string message, string title = "", string okButton = "OK")
        {
            var tcs = new TaskCompletionSource<object>();
            Alert(message, () => tcs.SetResult(null), title, okButton);
            return tcs.Task;
        }

        public void Input(
            string message,
            Action<string> okClicked,
            string placeholder = null,
            string title = null,
            string okButton = "OK",
            string cancelButton = "Cancel",
            string initialText = null)
        {
            Input(
                message,
                (ok, text) => { if (ok) okClicked(text); },
                placeholder,
                title,
                okButton,
                cancelButton,
                initialText);
        }

        public void Input(
            string message,
            Action<bool, string> answer,
            string hint = null,
            string title = null,
            string okButton = "OK",
            string cancelButton = "Cancel",
            string initialText = null)
        {
            this.InputImpl(message, answer, hint, title, okButton, cancelButton, initialText);
        }

        public Task<InputResponse> InputAsync(
            string message,
            string placeholder = null,
            string title = null,
            string okButton = "OK",
            string cancelButton = "Cancel",
            string initialText = null)
        {
            var tcs = new TaskCompletionSource<InputResponse>();
            Input(
                message,
                (ok, text) => tcs.SetResult(new InputResponse { Ok = ok, Text = text }),
                placeholder,
                title,
                okButton,
                cancelButton,
                initialText);
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

        private void ConfirmThreeButtonsImpl(string message, Action<ConfirmThreeButtonsResponse> callback, string title, string positive, string negative, string neutral)
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
                            .SetPositiveButton(positive, delegate { HandleDialogClose(userInteractionId, () => { if (callback != null) callback(ConfirmThreeButtonsResponse.Positive); }); })
                            .SetNegativeButton(negative, delegate { HandleDialogClose(userInteractionId, () => { if (callback != null) callback(ConfirmThreeButtonsResponse.Negative); }); })
                            .SetNeutralButton(neutral, delegate { HandleDialogClose(userInteractionId, () => { if (callback != null) callback(ConfirmThreeButtonsResponse.Neutral); }); })
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

        private void InputImpl(string message, Action<bool, string> callback, string hint, string title, string okButton, string cancelButton, string initialText)
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

                        var input = new EditText(this.CurrentActivity) { Hint = hint, Text = initialText };

                        new AlertDialog.Builder(this.CurrentActivity)
                            .SetMessage(message)
                            .SetTitle(title)
                            .SetView(input)
                            .SetPositiveButton(okButton, delegate { HandleDialogClose(userInteractionId, () => { if (callback != null) callback(true, input.Text); }); })
                            .SetNegativeButton(cancelButton, delegate { HandleDialogClose(userInteractionId, () => { if (callback != null) callback(false, input.Text); }); })
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
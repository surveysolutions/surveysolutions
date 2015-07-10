using System;
using System.Threading.Tasks;
using Android.App;
using Android.Widget;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid.Platform;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;

namespace WB.UI.QuestionnaireTester.CustomServices.UserInteraction
{
    public class UserInteractionService : IUserInteraction, IUserInteractionAwaiter
    {
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
            return Task.FromResult(null as object);
        }

        private void ConfirmThreeButtonsImpl(string message, Action<ConfirmThreeButtonsResponse> answer, string title, string positive, string negative, string neutral)
        {
            Application.SynchronizationContext.Post(
                ignored =>
                {
                    if (this.CurrentActivity == null) return;
                    new AlertDialog.Builder(this.CurrentActivity).SetMessage(message)
                        .SetTitle(title)
                        .SetPositiveButton(
                            positive,
                            delegate { if (answer != null) answer(ConfirmThreeButtonsResponse.Positive); })
                        .SetNegativeButton(
                            negative,
                            delegate { if (answer != null) answer(ConfirmThreeButtonsResponse.Negative); })
                        .SetNeutralButton(
                            neutral,
                            delegate { if (answer != null) answer(ConfirmThreeButtonsResponse.Neutral); })
                        .SetCancelable(false)
                        .Show();
                },
                null);
        }

        private void ConfirmImpl(string message, Action<bool> answer, string title, string okButton, string cancelButton)
        {
            //Mvx.Resolve<IMvxMainThreadDispatcher>().RequestMainThreadAction();
            Application.SynchronizationContext.Post(
                ignored =>
                {
                    if (this.CurrentActivity == null) return;
                    new AlertDialog.Builder(this.CurrentActivity).SetMessage(message)
                        .SetTitle(title)
                        .SetPositiveButton(okButton, delegate { if (answer != null) answer(true); })
                        .SetNegativeButton(cancelButton, delegate { if (answer != null) answer(false); })
                        .SetCancelable(false)
                        .Show();
                },
                null);
        }

        private void AlertImpl(string message, Action done, string title, string okButton)
        {
            Application.SynchronizationContext.Post(
                ignored =>
                {
                    if (this.CurrentActivity == null) return;
                    new AlertDialog.Builder(this.CurrentActivity).SetMessage(message)
                        .SetTitle(title)
                        .SetPositiveButton(okButton, delegate { if (done != null) done(); })
                        .SetCancelable(false)
                        .Show();
                },
                null);
        }

        private void InputImpl(string message, Action<bool, string> answer, string hint, string title, string okButton, string cancelButton, string initialText)
        {
            Application.SynchronizationContext.Post(
                ignored =>
                {
                    if (this.CurrentActivity == null) return;
                    var input = new EditText(this.CurrentActivity) { Hint = hint, Text = initialText };

                    new AlertDialog.Builder(this.CurrentActivity).SetMessage(message)
                        .SetTitle(title)
                        .SetView(input)
                        .SetPositiveButton(okButton, delegate { if (answer != null) answer(true, input.Text); })
                        .SetNegativeButton(cancelButton, delegate { if (answer != null) answer(false, input.Text); })
                        .SetCancelable(false)
                        .Show();
                },
                null);
        }
    }
}
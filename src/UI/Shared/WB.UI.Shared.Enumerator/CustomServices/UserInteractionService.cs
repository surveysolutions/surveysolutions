using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Text;
using Android.Widget;
using MvvmCross;
using MvvmCross.Platforms.Android;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Utils;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    internal class UserInteractionService : IUserInteractionService
    {
        private static readonly HashSet<Guid> userInteractions = new HashSet<Guid>();
        private static readonly object UserInteractionsLock = new object();
        private static TaskCompletionSource<object> userInteractionsAwaiter;

        private Activity CurrentActivity => Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity;

        public Task<bool> ConfirmAsync(
            string message,
            string title = "",
            string okButton = null,
            string cancelButton = null, 
            bool isHtml = true)
        {
            var tcs = new TaskCompletionSource<bool>();
            okButton = okButton ?? UIResources.Ok;
            cancelButton = cancelButton ?? UIResources.Cancel;

            this.Confirm(message, k => tcs.TrySetResult(k), title, okButton, cancelButton, isHtml);
            return tcs.Task;
        }

        public Task<string> ConfirmWithTextInputAsync(
           string message,
           string title = "",
           string okButton = null,
           string cancelButton = null,
           bool isTextInputPassword=false)
        {
            var tcs = new TaskCompletionSource<string>();
            okButton = okButton ?? UIResources.Ok;
            cancelButton = cancelButton ?? UIResources.Cancel;

            this.ConfirmWithTextInputImpl(message, k => tcs.TrySetResult(k ?? String.Empty),
                () => tcs.TrySetResult(null), title,
                okButton, cancelButton, isTextInputPassword);
            return tcs.Task;
        }

        public Task<string> SelectOneOptionFromList(string message,
            string[] options)
        {
            var tcs = new TaskCompletionSource<string>();

            var builder = new Android.Support.V7.App.AlertDialog.Builder(this.CurrentActivity);

            builder.SetTitle(message);
            builder.SetItems(options, (sender, args) =>
            {
                tcs.TrySetResult(options[args.Which]);
            });
            builder.SetCancelable(false);
            builder.SetNegativeButton(UIResources.Cancel, (sender, args) =>
            {
                tcs.TrySetResult(null);
            });
            builder.Show();

            return tcs.Task;
        }

        public Task AlertAsync(string message, string title = "", string okButton = null)
        {
            var tcs = new TaskCompletionSource<object>();
            okButton = okButton ?? UIResources.Ok;
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

        public void ShowToast(string message)
        {
            this.CurrentActivity.RunOnUiThread(() => Toast.MakeText(this.CurrentActivity, message, ToastLength.Short).Show());
        }

        public bool HasPendingUserInterations => userInteractions.Count > 0;

        private void Confirm(
            string message,
            Action<bool> answer,
            string title = null,
            string okButton = null,
            string cancelButton = null,
            bool isHtml = true)
        {
            this.ConfirmImpl(message, answer, title, okButton, cancelButton, isHtml);
        }

        private void Alert(string message, Action done = null, string title = "", string okButton = null)
        {
            this.AlertImpl(message, done, title, okButton);
        }

        private void ConfirmImpl(string message, Action<bool> callback, string title, string okButton, string cancelButton, bool isHtml)
        {
            var userInteractionId = Guid.NewGuid();
            okButton = okButton ?? UIResources.Ok;
            cancelButton = cancelButton ?? UIResources.Cancel;

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

                        new Android.Support.V7.App.AlertDialog.Builder(this.CurrentActivity)
                            .SetMessage(isHtml ? message.ToAndroidSpanned(): new SpannedString(message))
                            .SetTitle(isHtml ? title.ToAndroidSpanned() : new SpannedString(title))
                            .SetPositiveButton(okButton, delegate { HandleDialogClose(userInteractionId, () => callback?.Invoke(true)); })
                            .SetNegativeButton(cancelButton, delegate { HandleDialogClose(userInteractionId, () => callback?.Invoke(false)); })
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

        private void ConfirmWithTextInputImpl(string message, 
            Action<string> okCallback, 
            Action cancelCallBack, 
            string title, 
            string okButton, 
            string cancelButton, 
            bool isTextInputPassword)
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

                        var inflatedView = (LinearLayout)this.CurrentActivity.LayoutInflater.Inflate(Resource.Layout.confirmation_edit_text, null);
                        EditText editText = inflatedView.FindViewById<EditText>(Resource.Id.confirmationEditText);
                        if (isTextInputPassword)
                        {
                            editText.InputType = InputTypes.ClassText | InputTypes.TextVariationPassword;
                        }
                        new Android.Support.V7.App.AlertDialog.Builder(this.CurrentActivity)
                            .SetMessage(message.ToAndroidSpanned())
                            .SetTitle(title.ToAndroidSpanned()).SetView(inflatedView)
                            .SetPositiveButton(okButton,
                                delegate
                                {
                                    HandleDialogClose(userInteractionId, () =>
                                    {
                                        okCallback?.Invoke(editText.Text);
                                    });

                                    this.CurrentActivity.HideKeyboard(inflatedView.WindowToken);
                                })
                            .SetNegativeButton(cancelButton,
                                delegate
                                {
                                    HandleDialogClose(userInteractionId,
                                        () =>
                                        {
                                            cancelCallBack?.Invoke();
                                        });
                                    this.CurrentActivity.HideKeyboard(inflatedView.WindowToken);
                                })
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

                        new Android.Support.V7.App.AlertDialog.Builder(this.CurrentActivity)
                            .SetMessage(message.ToAndroidSpanned())
                            .SetTitle(title.ToAndroidSpanned())
                            .SetPositiveButton(okButton, delegate { HandleDialogClose(userInteractionId, () =>
                            {
                                callback?.Invoke();
                            }); })
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
            callback?.Invoke();
        }
    }
}

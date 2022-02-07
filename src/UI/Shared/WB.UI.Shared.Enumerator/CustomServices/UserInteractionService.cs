using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Text;
using Android.Text.Format;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Dialog;
using Google.Android.Material.TextField;
using Java.Lang;
using MvvmCross;
using MvvmCross.Platforms.Android;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Utils;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    internal class UserInteractionService : IUserInteractionService
    {
        private readonly IMvxAndroidCurrentTopActivity mvxCurrentTopActivity;
        private static readonly HashSet<Guid> UserInteractions = new HashSet<Guid>();
        private static readonly object UserInteractionsLock = new object();
        private static TaskCompletionSource<object> userInteractionsAwaiter;
        
        private class DialogInterfaceOnCancelListener : Java.Lang.Object, IDialogInterfaceOnCancelListener
        {
            private readonly Action action;

            public DialogInterfaceOnCancelListener(Action action)
            {
                this.action = action;
            }

            public void OnCancel(IDialogInterface dialog)
            {
                action();
            }
        }

        public UserInteractionService()
        {
            this.mvxCurrentTopActivity = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();
        }

        public Task<bool> ConfirmAsync(
            string message,
            string title = "",
            string okButton = null,
            string cancelButton = null, 
            bool isHtml = true)
        {
            var tcs = new TaskCompletionSource<bool>();
            okButton ??= UIResources.Ok;
            cancelButton ??= UIResources.Cancel;

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
            okButton ??= UIResources.Ok;
            cancelButton ??= UIResources.Cancel;

            this.ConfirmWithTextInputImpl(message, k => tcs.TrySetResult(k ?? string.Empty),
                () => tcs.TrySetResult(null), title,
                okButton, cancelButton, isTextInputPassword);
            return tcs.Task;
        }
        
        public Task<ChangePasswordDialogResult> ConfirmNewPasswordInputAsync(
           string message,
           string title = "",
           string okButton = null,
           string cancelButton = null,
           Func<ChangePasswordDialogOkCallback, Task> okCallback = null)
        {
            var tcs = new TaskCompletionSource<ChangePasswordDialogResult>();
            okButton ??= UIResources.Ok;
            cancelButton ??= UIResources.Cancel;

            this.ConfirmPasswordInputImpl(message, 
                (dialogResult) => tcs.TrySetResult(dialogResult),
                okCallback,
                () => tcs.TrySetResult(null), 
                title,
                okButton, 
                cancelButton);
            return tcs.Task;
        }

        public Task<string> SelectOneOptionFromList(string message,
            string[] options)
        {
            var tcs = new TaskCompletionSource<string>();

            var builder = new MaterialAlertDialogBuilder(this.mvxCurrentTopActivity.Activity);

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

        public Task AskDateAsync(EventHandler<DateTime> okCallback, DateTime date, DateTime? minDate = null)
        {
            var tcs = new TaskCompletionSource<string>();

            var datePicker = new DatePickerDialog(this.mvxCurrentTopActivity.Activity,
                (sender, arg) =>
                {
                    okCallback(sender, new DateTime(arg.Year, arg.Month + 1, arg.DayOfMonth));
                    tcs.TrySetResult(null);
                },
                date.Year,
                date.Month - 1,
                date.Day);

            if (minDate.HasValue)
            {
                datePicker.DatePicker.MinDate = Convert.ToInt64(minDate.Value.ToUniversalTime().Subtract(
                    new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                ).TotalMilliseconds);
            }
            datePicker.SetOnCancelListener(new DialogInterfaceOnCancelListener(() => tcs.TrySetResult(null)));
            datePicker.SetCancelable(false);
            datePicker.SetCanceledOnTouchOutside(false);
            datePicker.Show();

            return tcs.Task;
        }

        public Task AskTimeAsync(EventHandler<TimeSpan> okCallback, TimeSpan time)
        {
            var tcs = new TaskCompletionSource<string>();

            var activity = this.mvxCurrentTopActivity.Activity;
            var is24HourView = DateFormat.Is24HourFormat(activity);
            var timePickerDialog = new TimePickerDialog(activity,
                (sender, arg) =>
                {
                    okCallback(sender, new TimeSpan(arg.HourOfDay, arg.Minute, 0));
                    tcs.TrySetResult(null);
                },
                time.Hours,
                time.Minutes,
                is24HourView);

            timePickerDialog.SetOnCancelListener(new DialogInterfaceOnCancelListener(() => tcs.TrySetResult(null)));
            timePickerDialog.SetCancelable(false);
            timePickerDialog.SetCanceledOnTouchOutside(false);
            timePickerDialog.Show();

            return tcs.Task;
        }

        public Task ShowAlertWithQRCodeAndText(string qrCodeText, string description)
        {
            var tcs = new TaskCompletionSource<object>();
            this.AlertWithQrCode(() => tcs.TrySetResult(null),description, qrCodeText, UIResources.Ok);
            return tcs.Task;
        }

        public Task AlertAsync(string message, string title = "", string okButton = null)
        {
            var tcs = new TaskCompletionSource<object>();
            okButton ??= UIResources.Ok;
            this.Alert(message, () => tcs.TrySetResult(null), title, okButton);
            return tcs.Task;
        }

        public void ShowToast(string message)
        {
            var activity = this.mvxCurrentTopActivity.Activity; //sometime activity is null
            activity?.RunOnUiThread(() => Toast.MakeText(activity, message, ToastLength.Short).Show());
        }

        public bool HasPendingUserInteractions => UserInteractions.Count > 0;

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
            okButton ??= UIResources.Ok;
            cancelButton ??= UIResources.Cancel;

            try
            {
                HandleDialogOpen(userInteractionId);

                Application.SynchronizationContext.Post(
                    ignored =>
                    {
                        if (this.mvxCurrentTopActivity.Activity == null)
                        {
                            HandleDialogClose(userInteractionId);
                            return;
                        }

                        new MaterialAlertDialogBuilder(this.mvxCurrentTopActivity.Activity)
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
                        if (this.mvxCurrentTopActivity.Activity == null)
                        {
                            HandleDialogClose(userInteractionId);
                            return;
                        }

                        var inflatedView = (LinearLayout)this.mvxCurrentTopActivity.Activity.LayoutInflater.Inflate(Resource.Layout.confirmation_edit_text, null);
                        EditText editText = inflatedView.FindViewById<EditText>(Resource.Id.confirmationEditText);
                        if (isTextInputPassword)
                        {
                            editText.InputType = InputTypes.ClassText | InputTypes.TextVariationPassword;
                        }
                        new MaterialAlertDialogBuilder(this.mvxCurrentTopActivity.Activity)
                            .SetMessage(message.ToAndroidSpanned())
                            .SetTitle(title.ToAndroidSpanned()).SetView(inflatedView)
                            .SetPositiveButton(okButton,
                                delegate
                                {
                                    HandleDialogClose(userInteractionId, () =>
                                    {
                                        okCallback?.Invoke(editText.Text);
                                    });

                                    this.mvxCurrentTopActivity.Activity.HideKeyboard(inflatedView.WindowToken);
                                })
                            .SetNegativeButton(cancelButton,
                                delegate
                                {
                                    HandleDialogClose(userInteractionId,
                                        () =>
                                        {
                                            cancelCallBack?.Invoke();
                                        });
                                    this.mvxCurrentTopActivity.Activity.HideKeyboard(inflatedView.WindowToken);
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

        private void ConfirmPasswordInputImpl(string message, 
            Action<ChangePasswordDialogResult> okCallback, 
            Func<ChangePasswordDialogOkCallback, Task> canClose,
            Action cancelCallBack, 
            string title, 
            string okButton, 
            string cancelButton)
        {
            var userInteractionId = Guid.NewGuid();

            try
            {
                HandleDialogOpen(userInteractionId);

                Application.SynchronizationContext.Post(
                    ignored =>
                    {
                        if (this.mvxCurrentTopActivity.Activity == null)
                        {
                            HandleDialogClose(userInteractionId);
                            return;
                        }

                        var inflatedView = (LinearLayout)this.mvxCurrentTopActivity.Activity.LayoutInflater.Inflate(Resource.Layout.confirmation_password, null);
                        EditText oldPasswordText = inflatedView.FindViewById<EditText>(Resource.Id.oldPasswordEditText);
                        //oldPasswordText.InputType = InputTypes.ClassText | InputTypes.TextVariationPassword;
                        EditText passwordText = inflatedView.FindViewById<EditText>(Resource.Id.confirmationEditText);
                        //passwordText.InputType = InputTypes.ClassText | InputTypes.TextVariationPassword;
                        EditText confirmPasswordText = inflatedView.FindViewById<EditText>(Resource.Id.reconfirmationEditText);

                        inflatedView.FindViewById<TextInputLayout>(Resource.Id.oldPasswordEditTextWrapper).Hint = UIResources.OldPasswordHint;
                        inflatedView.FindViewById<TextInputLayout>(Resource.Id.confirmationEditTextWrapper).Hint = UIResources.NewPasswordHint;
                        var oldPassError = inflatedView.FindViewById<TextInputLayout>(Resource.Id.oldPasswordEditTextWrapper);
                        var passError = inflatedView.FindViewById<TextInputLayout>(Resource.Id.confirmationEditTextWrapper);
                        var confirmPassError = inflatedView.FindViewById<TextInputLayout>(Resource.Id.reconfirmationEditTextWrapper);
                        confirmPassError.Hint = UIResources.ConfirmNewPasswordHint;
                        //confirmPasswordText.InputType = InputTypes.ClassText | InputTypes.TextVariationPassword;
                        var dialogBuilder = new MaterialAlertDialogBuilder(this.mvxCurrentTopActivity.Activity)
                            .SetMessage(message.ToAndroidSpanned())
                            .SetTitle(title.ToAndroidSpanned()).SetView(inflatedView)
                            .SetPositiveButton(okButton, (IDialogInterfaceOnClickListener)null)
                            .SetNegativeButton(cancelButton,
                                delegate
                                {
                                    HandleDialogClose(userInteractionId,
                                        () =>
                                        {
                                            cancelCallBack?.Invoke();
                                        });
                                    this.mvxCurrentTopActivity.Activity.HideKeyboard(inflatedView.WindowToken);
                                })
                            .SetCancelable(false);
                        var dialog = dialogBuilder.Create();
                        dialog.Show();
                        Button button = dialog.GetButton((int)DialogButtonType.Positive);
                        button.SetOnClickListener(new OnClickListener(async delegate
                        {
                            oldPassError.Error = null;
                            passError.Error = null;
                            confirmPassError.Error = null;

                            if (!string.IsNullOrWhiteSpace(passwordText.Text) &&
                                passwordText.Text != confirmPasswordText.Text)
                            {
                                confirmPassError.Error = UIResources.PasswordMatchError;
                                return;
                            }
                                
                            var dialogCallback = new ChangePasswordDialogOkCallback()
                            {
                                DialogResult = new ChangePasswordDialogResult()
                                {
                                    NewPassword = passwordText.Text,
                                    OldPassword = oldPasswordText.Text,
                                }
                            };
                            if (canClose != null)
                                await canClose.Invoke(dialogCallback);
                            if (!dialogCallback.NeedClose)
                            {
                                oldPassError.Error = dialogCallback.OldPasswordError;
                                passError.Error = dialogCallback.NewPasswordError;
                                return;
                            }
                                    
                            HandleDialogClose(userInteractionId, () =>
                            {
                                okCallback?.Invoke(dialogCallback.DialogResult);
                            });
                            
                            dialog.Hide();

                            this.mvxCurrentTopActivity.Activity.HideKeyboard(inflatedView.WindowToken);
                        }));
                        
                        oldPasswordText.AddTextChangedListener(new PasswordTextWatcher(() => { oldPassError.Error = null; }));
                        passwordText.AddTextChangedListener(new PasswordTextWatcher(() => { passError.Error = null; }));
                        confirmPasswordText.AddTextChangedListener(new PasswordTextWatcher(() => { confirmPassError.Error = null; }));
                    },
                    null);
            }
            catch
            {
                HandleDialogClose(userInteractionId);
                throw;
            }
        }
        
        private class PasswordTextWatcher : Java.Lang.Object, ITextWatcher
        {
            private readonly Action action;

            public PasswordTextWatcher(Action action)
            {
                this.action = action;
            }

            public void AfterTextChanged(IEditable s)
            {
                action.Invoke();
            }

            public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
            {
            }

            public void OnTextChanged(ICharSequence s, int start, int before, int count)
            {
            }
        }
        
        private class OnClickListener : Java.Lang.Object, View.IOnClickListener
        {
            private readonly Action action;

            public OnClickListener(Action action)
            {
                this.action = action;
            }

            public void OnClick(View v)
            {
                action();
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
                        if (this.mvxCurrentTopActivity.Activity == null)
                        {
                            HandleDialogClose(userInteractionId);
                            return;
                        }
                        
                        new MaterialAlertDialogBuilder(this.mvxCurrentTopActivity.Activity)
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
        
        private void AlertWithQrCode(Action callback, string title, string qrCodeValue, string okButton)
        {
            var userInteractionId = Guid.NewGuid();

            try
            {
                HandleDialogOpen(userInteractionId);

                Application.SynchronizationContext.Post(
                    ignored =>
                    {
                        if (this.mvxCurrentTopActivity.Activity == null)
                        {
                            HandleDialogClose(userInteractionId);
                            return;
                        }

                        View view = this.mvxCurrentTopActivity.Activity.LayoutInflater
                            .Inflate(Resource.Layout.qr_code_popup, null);
                        
                        var textView = view!.FindViewById<TextView>(Resource.Id.linkWebInterview);
                        textView.Text = qrCodeValue;
                        var image = view!.FindViewById<ImageView>(Resource.Id.qrCodeImage);
                        image!.SetImageBitmap(QRCodeRenderer.RenderToBitmap(qrCodeValue));

                        new MaterialAlertDialogBuilder(this.mvxCurrentTopActivity.Activity)
                            .SetView(view)
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
                UserInteractions.Add(userInteractionId);
            }
        }

        private static void HandleDialogClose(Guid userInteractionId, Action callback = null)
        {
            lock (UserInteractionsLock)
            {
                UserInteractions.Remove(userInteractionId);

                if (UserInteractions.Count == 0)
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

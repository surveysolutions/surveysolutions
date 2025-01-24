﻿using System;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IUserInteractionService
    {
        Task<bool> ConfirmAsync(string message, string title = "", string okButton = null, string cancelButton = null, bool isHtml = true);
        Task<string> ConfirmWithTextInputAsync(string message, string title = "", string okButton = null, string cancelButton = null, bool isTextInputPassword = false);
        Task<ChangePasswordDialogResult> ConfirmNewPasswordInputAsync(string message, string title = "", string okButton = null, string cancelButton = null, Func<ChangePasswordDialogOkCallback, Task> okCallback = null);
        Task AlertAsync(string message, string title = "", string okButton = null);


        void ShowToast(string message, bool isTop = false);

        bool HasPendingUserInteractions { get; }

        Task<string> SelectOneOptionFromList(string message, string[] options);

        Task<string> SelectOneOptionFromList(string message, Tuple<string, string>[] options);

        Task AskDateAsync(EventHandler<DateTime> okCallback, DateTime date, DateTime? minDate = null);
        Task AskTimeAsync(EventHandler<TimeSpan> okCallback, TimeSpan time);
        Task ShowAlertWithQRCodeAndText(string qrCodeText, string description);
    }
}

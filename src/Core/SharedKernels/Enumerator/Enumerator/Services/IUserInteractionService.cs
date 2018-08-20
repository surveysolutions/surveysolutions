using System;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IUserInteractionService
    {
        Task<bool> ConfirmAsync(string message, string title = "", string okButton = null, string cancelButton = null, bool isHtml = true);
        Task<string> ConfirmWithTextInputAsync(string message, string title = "", string okButton = null, string cancelButton = null, bool isTextInputPassword = false);
        Task AlertAsync(string message, string title = "", string okButton = null);

        Task WaitPendingUserInteractionsAsync();

        void ShowToast(string message);

        bool HasPendingUserInterations { get; }

        Task<string> SelectOneOptionFromList(string message,
            string[] options);

        void ShowGoogleApiErrorDialog(int errorCode, int requestCode, Action onCancel = null);
    }
}

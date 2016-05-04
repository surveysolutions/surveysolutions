using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IUserInteractionService
    {
        Task<bool> ConfirmAsync(string message, string title = "", string okButton = "OK", string cancelButton = "Cancel");
        Task<string> ConfirmWithTextInputAsync(string message, string title = "", string okButton = "OK", string cancelButton = "Cancel", bool isTextInputPassword = false);
        Task AlertAsync(string message, string title = "", string okButton = "OK");

        Task WaitPendingUserInteractionsAsync();
    }
}

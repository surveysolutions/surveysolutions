using System;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Tester.Services
{
    public interface IUserInteractionService
    {
        Task<bool> ConfirmAsync(string message, string title = "", string okButton = "OK", string cancelButton = "Cancel");

        Task AlertAsync(string message, string title = "", string okButton = "OK");

        Task WaitPendingUserInteractionsAsync();
    }
}

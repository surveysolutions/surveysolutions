using System;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Tester.Services
{
    public interface IUserInteraction
    {
        void Confirm(string message, Action<bool> answer, string title = null, string okButton = "OK", string cancelButton = "Cancel");
        Task<bool> ConfirmAsync(string message, string title = "", string okButton = "OK", string cancelButton = "Cancel");

        void Alert(string message, Action done = null, string title = "", string okButton = "OK");
    }
}

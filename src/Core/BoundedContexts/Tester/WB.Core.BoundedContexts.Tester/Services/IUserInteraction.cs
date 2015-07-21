using System;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Tester.Services
{
    public class InputResponse
    {
        public bool Ok { get; set; }
        public string Text { get; set; }
    }

    public enum ConfirmThreeButtonsResponse
    {
        Positive,
        Negative,
        Neutral
    }

    public interface IUserInteraction
    {
        void Confirm(string message, Action okClicked, string title = null, string okButton = "OK", string cancelButton = "Cancel");
        void Confirm(string message, Action<bool> answer, string title = null, string okButton = "OK", string cancelButton = "Cancel");
        Task<bool> ConfirmAsync(string message, string title = "", string okButton = "OK", string cancelButton = "Cancel");

        void Alert(string message, Action done = null, string title = "", string okButton = "OK");
        Task AlertAsync(string message, string title = "", string okButton = "OK");

        void Input(string message, Action<string> okClicked, string placeholder = null, string title = null, string okButton = "OK", string cancelButton = "Cancel", string initialText = null);
        void Input(string message, Action<bool, string> answer, string placeholder = null, string title = null, string okButton = "OK", string cancelButton = "Cancel", string initialText = null);
        Task<InputResponse> InputAsync(string message, string placeholder = null, string title = null, string okButton = "OK", string cancelButton = "Cancel", string initialText = null);

        void ConfirmThreeButtons(string message, Action<ConfirmThreeButtonsResponse> answer, string title = null, string positive = "Yes", string negative = "No", string neutral = "Maybe");
        Task<ConfirmThreeButtonsResponse> ConfirmThreeButtonsAsync(string message, string title = null, string positive = "Yes", string negative = "No", string neutral = "Maybe");
    }
}

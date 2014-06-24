using System;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public interface IAnswerProgressIndicator
    {
        void Setup(Action show, Action hide);
        void Show();
        void Hide();
    }
}
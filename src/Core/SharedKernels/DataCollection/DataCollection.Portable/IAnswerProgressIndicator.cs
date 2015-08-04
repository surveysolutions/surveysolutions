using System;

namespace WB.Core.SharedKernels.DataCollection
{
    public interface IAnswerProgressIndicator
    {
        void Setup(Action show, Action hide);
        void Show();
        void Hide();
    }
}
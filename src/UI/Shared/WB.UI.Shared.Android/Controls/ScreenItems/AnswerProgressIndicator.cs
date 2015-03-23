using System;
using WB.Core.SharedKernels.DataCollection;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public class AnswerProgressIndicator : IAnswerProgressIndicator
    {
        private Action show;
        private Action hide;

        public void Setup(Action show, Action hide)
        {
            this.show = show;
            this.hide = hide;
        }

        public void Show()
        {
            try
            {
                if (this.show != null)
                    this.show();
            }
            catch { }
        }

        public void Hide()
        {
            try
            {
                if (this.hide != null)
                    this.hide();
            }
            catch { }
        }
    }
}
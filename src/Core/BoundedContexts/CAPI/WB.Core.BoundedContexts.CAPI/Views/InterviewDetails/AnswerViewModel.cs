using System;

namespace WB.Core.BoundedContexts.CAPI.Views.InterviewDetails
{
    public class AnswerViewModel : Cirrious.MvvmCross.ViewModels.MvxViewModel, ICloneable
    {
        public AnswerViewModel(Guid publicKey, string title, string value, bool selected, string imagePublicKey)
        {
            this.PublicKey = publicKey;
            this.Title = title;
            this.Selected = selected;
            this.Value = decimal.Parse(value);
            this.ImagePublicKey = imagePublicKey;
            this.AnswerOrder = 0;
        }

        public Guid PublicKey { get; private set; }
        public string ImagePublicKey { get; private set; }
        public string Title { get; private set; }
        public decimal Value { get; private set; }
        public bool Selected { get;  set; }
        public int AnswerOrder { get; set; }

        #region Implementation of ICloneable

        public object Clone()
        {
            return new AnswerViewModel(this.PublicKey, this.Title, this.Value.ToString(), this.Selected, this.ImagePublicKey);
        }

        #endregion
    }
}
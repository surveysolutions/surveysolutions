using System;
using System.Globalization;

namespace WB.Core.BoundedContexts.Capi.Views.InterviewDetails
{
    public class AnswerViewModel : Cirrious.MvvmCross.ViewModels.MvxViewModel, ICloneable
    {
        public AnswerViewModel(Guid publicKey, string title, string value, bool selected, string imagePublicKey)
        {
            this.PublicKey = publicKey;
            this.Title = title;
            this.Selected = selected;
            if (!string.IsNullOrEmpty(value))
                this.Value = decimal.Parse(value, CultureInfo.InvariantCulture);
            this.ImagePublicKey = imagePublicKey;
            this.AnswerOrder = 0;
        }

        public Guid PublicKey { get; private set; }
        public string ImagePublicKey { get; private set; }
        public string Title { get; private set; }
        public decimal Value { get; private set; }
        public bool Selected { get;  set; }
        public int AnswerOrder { get; set; }

        public object Clone()
        {
            return new AnswerViewModel(this.PublicKey, this.Title, this.Value.ToString(CultureInfo.InvariantCulture), this.Selected, this.ImagePublicKey);
        }
    }

    public interface ICloneable
    {
        object Clone();
    }
}
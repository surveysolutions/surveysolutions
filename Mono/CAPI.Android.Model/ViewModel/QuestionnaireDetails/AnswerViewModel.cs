using System;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{
    public class AnswerViewModel : Cirrious.MvvmCross.ViewModels.MvxViewModel, ICloneable
    {
        public AnswerViewModel(Guid publicKey, string title, decimal value, bool selected, string imagePublicKey)
        {
            PublicKey = publicKey;
            Title = title;
            Selected = selected;
            Value = value;
            ImagePublicKey = imagePublicKey;
        }

        public Guid PublicKey { get; private set; }
        public string ImagePublicKey { get; private set; }
        public string Title { get; private set; }
        public decimal Value { get; private set; }
        public bool Selected { get;  set; }

        #region Implementation of ICloneable

        public object Clone()
        {
            return new AnswerViewModel(this.PublicKey, this.Title,this.Value, this.Selected, this.ImagePublicKey);
        }

        #endregion
    }
}
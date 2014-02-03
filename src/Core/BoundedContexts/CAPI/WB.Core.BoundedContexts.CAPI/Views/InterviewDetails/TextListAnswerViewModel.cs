using System;

namespace WB.Core.BoundedContexts.Capi.Views.InterviewDetails
{
    public class TextListAnswerViewModel : Cirrious.MvvmCross.ViewModels.MvxViewModel, ICloneable
    {
        public TextListAnswerViewModel(decimal value, string title)
        {
            this.Value = value;
            this.Answer = title;
        }

        public decimal Value { get; set; }
        public string Answer { get; set; }

        public object Clone()
        {
            return new TextListAnswerViewModel(this.Value, this.Answer);
        }
    }
}
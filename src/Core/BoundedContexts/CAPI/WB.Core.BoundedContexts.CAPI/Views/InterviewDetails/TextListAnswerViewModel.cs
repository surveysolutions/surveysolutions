using System;

namespace WB.Core.BoundedContexts.Capi.Views.InterviewDetails
{
    public class TextListAnswerViewModel : Cirrious.MvvmCross.ViewModels.MvxViewModel, ICloneable
    {
        public TextListAnswerViewModel(string value, string title)
        {
            if (!string.IsNullOrEmpty(value))
                this.Value = decimal.Parse(value);
            
            this.Title = title;
        }

        public decimal Value { get; set; }
        public string Title { get; set; }

        public object Clone()
        {
            return new TextListAnswerViewModel(this.Value.ToString(), this.Title);
        }
    }
}
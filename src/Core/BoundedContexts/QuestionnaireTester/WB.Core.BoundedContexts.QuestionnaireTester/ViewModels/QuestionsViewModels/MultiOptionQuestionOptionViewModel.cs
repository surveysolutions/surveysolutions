using System;
using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class MultiOptionQuestionOptionViewModel : MvxNotifyPropertyChanged
    {
        private bool @checked;

        public event EventHandler<OptionCheckedArgs> BeforeCheckedChanged;

        public decimal Value { get; set; }
        public string Title { get; set; }

        public bool Checked
        {
            get { return @checked; }
            set
            {
                try
                {
                    OnBeforeCheckedChanged(value);
                    @checked = value;
                }
                finally
                {
                    RaisePropertyChanged();
                }
            }
        }

        protected virtual void OnBeforeCheckedChanged(bool newValue)
        {
            var handler = BeforeCheckedChanged;
            if (handler != null) handler(this, new OptionCheckedArgs(newValue));
        }
    }

    public class OptionCheckedArgs : EventArgs
    {
        public bool NewValue { get; set; }

        public OptionCheckedArgs(bool newValue)
        {
            NewValue = newValue;
        }
    }
}
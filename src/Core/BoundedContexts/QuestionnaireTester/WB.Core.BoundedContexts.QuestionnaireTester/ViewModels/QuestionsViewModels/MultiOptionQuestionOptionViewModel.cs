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
                var args = new OptionCheckedArgs(value);
                OnBeforeCheckedChanged(args);
                if (!args.CancelCheck)
                {
                    @checked = value;
                }

                RaisePropertyChanged();
            }
        }

        protected virtual void OnBeforeCheckedChanged(OptionCheckedArgs args)
        {
            var handler = BeforeCheckedChanged;
            if (handler != null) handler(this, args);
        }
    }

    public class OptionCheckedArgs : EventArgs
    {
        public bool NewValue { get; set; }

        public bool CancelCheck { get; set; }

        public OptionCheckedArgs(bool newValue)
        {
            NewValue = newValue;
        }
    }
}
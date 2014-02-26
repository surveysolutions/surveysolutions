using Cirrious.MvvmCross.ViewModels;

namespace CapiDataGenerator
{
    public class LogMessage : MvxViewModel
    {
        private string _message;
        private string _link;
        private string _linkText;

        public LogMessage(string message)
        {
            Message = message;
        }

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                RaisePropertyChanged(() => Message);
            }
        }

        public string LinkText
        {
            get { return _linkText; }
            set
            {
                _linkText = value;
                RaisePropertyChanged(() => LinkText);
            }
        }

        public string Link
        {
            get { return _link; }
            set
            {
                _link = value;
                RaisePropertyChanged(() => Link);
            }
        }
    }
}
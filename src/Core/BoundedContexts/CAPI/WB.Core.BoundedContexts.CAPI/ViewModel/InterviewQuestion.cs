namespace WB.Core.BoundedContexts.Capi.ViewModel
{
    public class InterviewQuestion : InterviewEntity
    {
        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                RaisePropertyChanged(() => Title);
            }
        }

        private string instructions;
        public string Instructions
        {
            get { return instructions; }
            set
            {
                instructions = value;
                RaisePropertyChanged(() => Instructions);
            }
        }

        private bool isValid;
        public bool IsValid
        {
            get { return isValid; }
            set
            {
                isValid = value;
                RaisePropertyChanged(() => IsValid);
            }
        }

        private bool isMandatory;
        public bool IsMandatory
        {
            get { return isMandatory; }
            set
            {
                isMandatory = value;
                RaisePropertyChanged(() => IsMandatory);
            }
        }
    }
}
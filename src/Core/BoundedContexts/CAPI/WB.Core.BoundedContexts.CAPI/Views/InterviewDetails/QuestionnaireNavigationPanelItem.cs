using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.CAPI.Views.InterviewDetails
{
    public class QuestionnaireNavigationPanelItem : Cirrious.MvvmCross.ViewModels.MvxViewModel,
                                                    IQuestionnaireItemViewModel
    {
        public QuestionnaireNavigationPanelItem(InterviewItemId publicKey, IQuestionnaireViewModel fullScreen)
        {
            this.PublicKey = publicKey;
            this.fullScreen = fullScreen;
            fullScreen.PropertyChanged += screen_PropertyChanged;
        }

        private IQuestionnaireViewModel fullScreen;

        public InterviewItemId PublicKey { get; private set; }


        public IQuestionnaireItemViewModel Clone(int[] propagationVector)
        {
            return new QuestionnaireNavigationPanelItem(new InterviewItemId(this.PublicKey.Id, propagationVector),
                                                        this.fullScreen);
        }

        public string Text
        {
            get { return this.Screen.ScreenName; }
        }

        public bool Enabled
        {
            get { return this.Screen.Enabled; }
        }

        public int Total
        {
            get { return this.Screen.Total; }
        }

        public int Answered
        {
            get { return this.Screen.Answered; }
        }

        protected IQuestionnaireViewModel Screen
        {
            get { return this.fullScreen; }
        }

        private void screen_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Answered" && e.PropertyName != "Total" && e.PropertyName != "Enabled")
                return;
            this.RaisePropertyChanged(e.PropertyName);
        }
    }
}
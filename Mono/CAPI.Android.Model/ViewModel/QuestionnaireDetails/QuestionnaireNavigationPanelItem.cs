using System;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{
    public class QuestionnaireNavigationPanelItem : Cirrious.MvvmCross.ViewModels.MvxViewModel,
                                                    IQuestionnaireItemViewModel
    {
        public QuestionnaireNavigationPanelItem(ItemPublicKey publicKey, IQuestionnaireViewModel fullScreen)
        {
            this.PublicKey = publicKey;
            this.fullScreen = fullScreen;
            fullScreen.PropertyChanged += screen_PropertyChanged;
        }

        private IQuestionnaireViewModel fullScreen;

        public ItemPublicKey PublicKey { get; private set; }


        public IQuestionnaireItemViewModel Clone(Guid propagationKey)
        {
            return new QuestionnaireNavigationPanelItem(new ItemPublicKey(this.PublicKey.PublicKey, propagationKey),
                                                        fullScreen);
        }

        public string Text
        {
            get { return Screen.ScreenName; }
        }

        public bool Enabled
        {
            get { return Screen.Enabled; }
        }

        public int Total
        {
            get { return Screen.Total; }
        }

        public int Answered
        {
            get { return Screen.Answered; }
        }

        protected IQuestionnaireViewModel Screen
        {
            get { return fullScreen; }
        }

        private void screen_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Answered" && e.PropertyName != "Total" && e.PropertyName != "Enabled")
                return;
            RaisePropertyChanged(e.PropertyName);
        }
    }
}
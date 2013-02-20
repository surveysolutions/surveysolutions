using System;

namespace AndroidApp.Core.Model.ViewModel.QuestionnaireDetails
{
    public class QuestionnaireNavigationPanelItem : Cirrious.MvvmCross.ViewModels.MvxViewModel,
                                                    IQuestionnaireItemViewModel
    {
        public QuestionnaireNavigationPanelItem(ItemPublicKey publicKey, Func<ItemPublicKey, IQuestionnaireViewModel> getFullScreen)
        {
            this.PublicKey = publicKey;
            this.getFullScreen = getFullScreen;
        }

        private Func<ItemPublicKey, IQuestionnaireViewModel> getFullScreen;

        public ItemPublicKey PublicKey { get; private set; }
       

        public IQuestionnaireItemViewModel Clone(Guid propagationKey)
        {
            return new QuestionnaireNavigationPanelItem(new ItemPublicKey(this.PublicKey.PublicKey, propagationKey),
                                                        getFullScreen);
        }
        public string Text { get { return Screen.ScreenName; } }

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
            get
            {
                if (screen == null)
                {
                    screen = getFullScreen(this.PublicKey);
                    screen.PropertyChanged += screen_PropertyChanged;
                }
                return screen;
            }
        }

        private void screen_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Answered" && e.PropertyName != "Total")
                return;
            RaisePropertyChanged(e.PropertyName);
        }

        private IQuestionnaireViewModel screen;

     /*   public void RestoreFullScreenFunk(Func<ItemPublicKey, IQuestionnaireViewModel> getFullScreen)
        {
            this.getFullScreen = getFullScreen;
            var t = Screen;
        }*/
    }
}
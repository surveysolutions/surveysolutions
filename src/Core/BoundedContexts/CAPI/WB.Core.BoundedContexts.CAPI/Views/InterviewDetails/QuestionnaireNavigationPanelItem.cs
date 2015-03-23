using System;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Capi.Views.InterviewDetails
{
    public class QuestionnaireNavigationPanelItem : Cirrious.MvvmCross.ViewModels.MvxViewModel,
                                                    IQuestionnaireItemViewModel
    {
        public QuestionnaireNavigationPanelItem(InterviewItemId publicKey, Func<string, IQuestionnaireViewModel> getFullScreen)
        {
            this.PublicKey = publicKey;
            this.getFullScreen = getFullScreen;
        }

        private Func<string, IQuestionnaireViewModel> getFullScreen;

        public InterviewItemId PublicKey { get; private set; }

        public IQuestionnaireItemViewModel Clone(decimal[] propagationVector)
        {
            return new QuestionnaireNavigationPanelItem(new InterviewItemId(this.PublicKey.Id, propagationVector),
                                                        this.getFullScreen);
        }

        public string Text
        {
            get { return this.Screen.ScreenName; }
        }

        public bool Enabled
        {
            get { return this.Screen.Enabled; }
        }

        public void SetParentEnabled(bool enabled)
        {
            this.Screen.SetParentEnabled(enabled);
        }

        public void SetEnabled(bool enabled)
        {
            this.Screen.SetEnabled(enabled);
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
            get
            {
                if (screen == null)
                {
                    screen = getFullScreen(ConversionHelper.ConvertIdAndRosterVectorToString(PublicKey.Id, PublicKey.InterviewItemPropagationVector));
                    screen.PropertyChanged += screen_PropertyChanged;
                }
                return this.screen;
            }
        }

        private IQuestionnaireViewModel screen;

        private void screen_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Answered" && e.PropertyName != "Total" && e.PropertyName != "Enabled")
                return;
            this.RaisePropertyChanged(e.PropertyName);
        }
    }
}
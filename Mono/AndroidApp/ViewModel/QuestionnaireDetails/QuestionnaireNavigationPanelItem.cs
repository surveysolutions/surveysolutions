using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace AndroidApp.ViewModel.QuestionnaireDetails
{
    public class QuestionnaireNavigationPanelItem : Cirrious.MvvmCross.ViewModels.MvxViewModel,
                                                    IQuestionnaireItemViewModel
    {
        public QuestionnaireNavigationPanelItem(ItemPublicKey screenPublicKey, string title, int total, int answered,
                                                bool enabled, Func<IQuestionnaireViewModel> getFullScreen)
        {
            PublicKey = screenPublicKey;
            Text = title;
            Total = total;
            Answered = answered;
            Enabled = enabled;
            this.getFullScreen = getFullScreen;
        }

        private readonly Func<IQuestionnaireViewModel> getFullScreen;
        public ItemPublicKey PublicKey { get; private set; }
        public string Text { get; private set; }

        public IQuestionnaireItemViewModel Clone(Guid propagationKey)
        {
            return new QuestionnaireNavigationPanelItem(new ItemPublicKey(this.PublicKey.PublicKey, propagationKey),
                                                        this.Text, this.Total, this.Answered, this.Enabled,
                                                        getFullScreen);
        }

        public bool Enabled { get; private set; }
        public int Total { get; private set; }
        public int Answered { get; private set; }

        public IQuestionnaireViewModel Screen
        {
            get { return getFullScreen(); }
        }
    }
}
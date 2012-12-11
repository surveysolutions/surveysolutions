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
using AndroidApp.ViewModel.Input;
using AndroidApp.ViewModel.Model;
using Main.Core.View;

namespace AndroidApp.ViewModel.QuestionnaireDetails
{
    public class QuestionnaireNavigationPanelViewFactory : IViewFactory<QuestionnaireNavigationPanelInput, QuestionnaireNavigationPanelModel>
    {
        #region Implementation of IViewFactory<QuestionnaireDetailsInput,QuestionnaireDetailsModel>

        public QuestionnaireNavigationPanelModel Load(QuestionnaireNavigationPanelInput input)
        {
            return new QuestionnaireNavigationPanelModel(input.QuestionnairePublicKey,new QuestionnaireNavigationPanelItem[]
                {
                    new QuestionnaireNavigationPanelItem(Guid.NewGuid(),"hello1",20,1),
                    new QuestionnaireNavigationPanelItem(Guid.NewGuid(),"hello2",30,14),
                });
        }

        #endregion
    }
}
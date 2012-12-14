// -----------------------------------------------------------------------
// <copyright file="QuestionnaireScreenViewFactory.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.View;

namespace AndroidApp.ViewModel.QuestionnaireDetails
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class QuestionnaireScreenViewFactory : IViewFactory<QuestionnaireScreenInput, QuestionnaireScreenViewModel>
    {
        #region Implementation of IViewFactory<QuestionnaireScreenInput,QuestionnaireScreenViewModel>

        public QuestionnaireScreenViewModel Load(QuestionnaireScreenInput input)
        {
            var screens =  new QuestionnaireNavigationPanelItem[]
                {
                    new QuestionnaireNavigationPanelItem(Guid.NewGuid(),"hello1",20,1),
                    new QuestionnaireNavigationPanelItem(Guid.NewGuid(),"hello2",30,14),
                };
            return new QuestionnaireScreenViewModel(input.QuestionnaireId, input.ScreenPublicKey ?? Guid.NewGuid(),
                                                    input.PropagationKey, screens,
                                                    Enumerable.Empty<QuestionnaireNavigationPanelItem>(), screens);
        }

        #endregion
    }
}

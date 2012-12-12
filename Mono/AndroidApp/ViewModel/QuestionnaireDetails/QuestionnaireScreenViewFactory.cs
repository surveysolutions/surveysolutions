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
            return new QuestionnaireScreenViewModel(input.QuestionnaireId, input.ScreenPublicKey ?? Guid.NewGuid(),
                                                    input.PropagationKey, null, null,
                                                    Enumerable.Empty<QuestionnaireNavigationPanelItem>());
        }

        #endregion
    }
}

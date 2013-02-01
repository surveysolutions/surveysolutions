// -----------------------------------------------------------------------
// <copyright file="QuestionnaireScreenViewFactory.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using Main.Core.View;
using Main.DenormalizerStorage;

namespace AndroidApp.Core.Model.ViewModel.QuestionnaireDetails
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class QuestionnaireScreenViewFactory : IViewFactory<QuestionnaireScreenInput, CompleteQuestionnaireView>
    {
        private readonly IDenormalizerStorage<CompleteQuestionnaireView> _documentStorage;

        public QuestionnaireScreenViewFactory(IDenormalizerStorage<CompleteQuestionnaireView> documentStorage)
        {
            this._documentStorage = documentStorage;
        }

        #region Implementation of IViewFactory<QuestionnaireScreenInput,QuestionnaireScreenViewModel>

        public CompleteQuestionnaireView Load(QuestionnaireScreenInput input)
        {
            var result= Queryable.First<CompleteQuestionnaireView>(this._documentStorage.Query());
            result.Restore();
            return result;
        }

        #endregion
       

    }
}

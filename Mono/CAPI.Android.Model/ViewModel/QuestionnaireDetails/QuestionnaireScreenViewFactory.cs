using Main.Core.View;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{
    public class QuestionnaireScreenViewFactory : IViewFactory<QuestionnaireScreenInput, CompleteQuestionnaireView>
    {
        #warning Writer should not be used in View Factory
        private readonly IReadSideRepositoryWriter<CompleteQuestionnaireView> _documentStorage;

        public QuestionnaireScreenViewFactory(IReadSideRepositoryWriter<CompleteQuestionnaireView> documentStorage)
        {
            this._documentStorage = documentStorage;
        }

        #region Implementation of IViewFactory<QuestionnaireScreenInput,QuestionnaireScreenViewModel>

        public CompleteQuestionnaireView Load(QuestionnaireScreenInput input)
        {
            return this._documentStorage.GetById(input.QuestionnaireId);
        }

        #endregion

    }
}

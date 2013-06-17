// -----------------------------------------------------------------------
// <copyright file="QuestionnaireScreenViewFactory.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Main.Core.Documents;
using Main.Core.View;
using Main.DenormalizerStorage;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

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

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
            var result = this._documentStorage.GetById(input.QuestionnaireId);
            if (result == null)
            {
                GenerateEvents(input.QuestionnaireId);
                result = this._documentStorage.GetById(input.QuestionnaireId);
            }
            return result;
        }

        #endregion


        public void GenerateEvents(Guid questionnaireId)
        {
            var bus = NcqrsEnvironment.Get<IEventBus>() as InProcessEventBus;
            var eventStore = NcqrsEnvironment.Get<IEventStore>();
            var snapshotStore = NcqrsEnvironment.Get<ISnapshotStore>();

            long minVersion = 0;
            var snapshot = snapshotStore.GetSnapshot(questionnaireId, long.MaxValue);
            if (snapshot != null)
            {
                var originalDoc = snapshot.Payload as CompleteQuestionnaireDocument;
                if (originalDoc != null)
                {
                    this._documentStorage.Store(
                        new CompleteQuestionnaireView(originalDoc),
                        questionnaireId);
                    minVersion = snapshot.Version + 1;
                }
            }
            foreach (CommittedEvent committedEvent in
                eventStore.ReadFrom(questionnaireId, minVersion, long.MaxValue))
            {
                bus.Publish(committedEvent);
            }
        }

    }
}

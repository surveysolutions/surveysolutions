// -----------------------------------------------------------------------
// <copyright file="QuestionnaireScreenViewFactory.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Main.Core.View;
using Main.DenormalizerStorage;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using Ncqrs.Restoring.EventStapshoot;
using Ncqrs.Restoring.EventStapshoot.EventStores;

using WB.Core.Infrastructure;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
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
            var eventStore = NcqrsEnvironment.Get<IEventStore>() as ISnapshootEventStore;
            var snapshotStore = NcqrsEnvironment.Get<ISnapshotStore>() as InMemorySnapshootStore;

            long minVersion = 0;
            var snapshot = eventStore.GetLatestSnapshoot(questionnaireId);
            if (snapshot != null)
            {
                bus.Publish(snapshot);
                snapshotStore.SaveEventToSnapshotStore(snapshot);
                minVersion = snapshot.EventSequence + 1;
            }
            foreach (CommittedEvent committedEvent in
                eventStore.ReadFrom(questionnaireId, minVersion, long.MaxValue))
            {
                bus.Publish(committedEvent);
            }
        }

    }
}

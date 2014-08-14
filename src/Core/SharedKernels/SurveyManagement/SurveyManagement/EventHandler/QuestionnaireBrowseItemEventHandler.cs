using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    [Obsolete("Remove it when HQ is a separate application")]
    public class QuestionnaireBrowseItemEventHandler : IEventHandler, IEventHandler<TemplateImported>, IEventHandler<PlainQuestionnaireRegistered>, IEventHandler<QuestionnaireDeleted> 
    {
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireBrowseItem> readsideRepositoryWriter;

        public QuestionnaireBrowseItemEventHandler(IVersionedReadSideRepositoryWriter<QuestionnaireBrowseItem> readsideRepositoryWriter, IPlainQuestionnaireRepository plainQuestionnaireRepository)
        {
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.readsideRepositoryWriter = readsideRepositoryWriter;
        }

        public string Name { get { return this.GetType().Name; } }

        public Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public Type[] BuildsViews { get { return new Type[] { typeof(QuestionnaireBrowseItem) }; } }

        private  QuestionnaireBrowseItem CreateBrowseItem(long version, QuestionnaireDocument questionnaireDocument, bool allowCensusMode)
        {
            return new QuestionnaireBrowseItem(questionnaireDocument, version, allowCensusMode);
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            long version = evnt.Payload.Version ?? evnt.EventSequence;
            QuestionnaireDocument questionnaireDocument = evnt.Payload.Source;

            var view = CreateBrowseItem(version, questionnaireDocument, evnt.Payload.AllowCensusMode);
            readsideRepositoryWriter.Store(view, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<PlainQuestionnaireRegistered> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version;
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            var view = CreateBrowseItem(version, questionnaireDocument, evnt.Payload.AllowCensusMode);
            readsideRepositoryWriter.Store(view, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            readsideRepositoryWriter.Remove(evnt.EventSourceId, evnt.Payload.QuestionnaireVersion);
        }
    }
}

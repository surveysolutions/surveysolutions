using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class QuestionnaireDenormalizer : BaseDenormalizer, IEventHandler<TemplateImported>, IEventHandler<PlainQuestionnaireRegistered>, 
        IEventHandler<QuestionnaireDeleted>
    {
        private readonly IVersionedReadSideKeyValueStorage<QuestionnaireDocumentVersioned> documentStorage;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviews;
        private readonly IQuestionnaireCacheInitializer questionnaireCacheInitializer;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor;

        public QuestionnaireDenormalizer(
            IVersionedReadSideKeyValueStorage<QuestionnaireDocumentVersioned> documentStorage, 
            IQuestionnaireCacheInitializer questionnaireCacheInitializer,
            IPlainQuestionnaireRepository plainQuestionnaireRepository, 
            IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor,
            IReadSideRepositoryWriter<InterviewSummary> interviews)
        {
            this.documentStorage = documentStorage;
            this.questionnaireCacheInitializer = questionnaireCacheInitializer;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.questionnareAssemblyFileAccessor = questionnareAssemblyFileAccessor;
            this.interviews = interviews;
        }

        public override object[] Writers
        {
            get { return new object[] { documentStorage }; }
        }

        public override object[] Readers
        {
            get { return new object[] { interviews }; }
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version ?? evnt.EventSequence;
            QuestionnaireDocument questionnaireDocument = evnt.Payload.Source;

            this.StoreQuestionnaire(id, version, questionnaireDocument, evnt.Payload.AllowCensusMode, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<PlainQuestionnaireRegistered> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version;

            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            this.StoreQuestionnaire(id, version, questionnaireDocument, evnt.Payload.AllowCensusMode, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            this.documentStorage.Remove(evnt.EventSourceId, evnt.Payload.QuestionnaireVersion);

         /*   var anyInterviewExists =
                        interviews.Query(_ => _.Any(i =>!i.IsDeleted && i.QuestionnaireId == evnt.EventSourceId && i.QuestionnaireVersion == evnt.Payload.QuestionnaireVersion));
            if (!anyInterviewExists)
            {
                this.questionnareAssemblyFileAccessor.RemoveAssembly(evnt.EventSourceId, evnt.Payload.QuestionnaireVersion);
            }*/
        }

        private void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument, bool allowCensusMode, DateTime timestamp)
        {
            var document = questionnaireDocument.Clone() as QuestionnaireDocument;
            if (document == null)
                return;

            this.questionnaireCacheInitializer.InitializeQuestionnaireDocumentWithCaches(document);

            this.documentStorage.Store(
                new QuestionnaireDocumentVersioned() { Questionnaire = document, Version = version },
                id);
            
            
        }
    }
}
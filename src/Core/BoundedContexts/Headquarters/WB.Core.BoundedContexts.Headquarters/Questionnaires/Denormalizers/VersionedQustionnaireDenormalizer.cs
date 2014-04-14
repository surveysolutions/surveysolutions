using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Denormalizers
{
    public class VersionedQustionnaireDenormalizer : BaseDenormalizer, IEventHandler<TemplateImported>
    {
        private readonly IQuestionnaireCacheInitializer questionnaireCacheInitializer;
        private readonly IReadSideRepositoryWriter<QuestionnaireDocument> documentStorage;

        public VersionedQustionnaireDenormalizer(IQuestionnaireCacheInitializer questionnaireCacheInitializer,
            IReadSideRepositoryWriter<QuestionnaireDocument> documentStorage)
        {
            if (questionnaireCacheInitializer == null) throw new ArgumentNullException("questionnaireCacheInitializer");

            this.questionnaireCacheInitializer = questionnaireCacheInitializer;
            this.documentStorage = documentStorage;
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            QuestionnaireDocument document = evnt.Payload.Source.Clone();
            if (document == null)
                return;

            this.questionnaireCacheInitializer.InitializeQuestionnaireDocumentWithCaches(document);

            this.documentStorage.Store(document,
                document.PublicKey.FormatGuid() + "$" + evnt.EventSequence);
        }

        public override Type[] BuildsViews
        {
            get { return new Type[] { typeof(QuestionnaireDocument) }; }
        }
    }
}
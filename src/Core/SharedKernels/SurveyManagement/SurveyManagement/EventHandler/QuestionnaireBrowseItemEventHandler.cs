using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    [Obsolete("Remove it when HQ is a separate application")]
    public class QuestionnaireBrowseItemEventHandler : AbstractFunctionalEventHandler<QuestionnaireBrowseItem>,
        ICreateHandler<QuestionnaireBrowseItem, TemplateImported>,
        ICreateHandler<QuestionnaireBrowseItem, PlainQuestionnaireRegistered>
    {
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;

        public QuestionnaireBrowseItemEventHandler(IVersionedReadSideRepositoryWriter<QuestionnaireBrowseItem> readsideRepositoryWriter, IPlainQuestionnaireRepository plainQuestionnaireRepository)
            : base(readsideRepositoryWriter)
        {
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
        }

        public override Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public QuestionnaireBrowseItem Create(IPublishedEvent<TemplateImported> evnt)
        {
            long version = evnt.EventSequence;
            QuestionnaireDocument questionnaireDocument = evnt.Payload.Source;

            return CreateBrowseItem(version, questionnaireDocument, evnt.Payload.AllowCensusMode);
        }

        public QuestionnaireBrowseItem Create(IPublishedEvent<PlainQuestionnaireRegistered> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version;
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            return CreateBrowseItem(version, questionnaireDocument, evnt.Payload.AllowCensusMode);
        }

        private static QuestionnaireBrowseItem CreateBrowseItem(long version, QuestionnaireDocument questionnaireDocument, bool allowCensusMode)
        {
            return new QuestionnaireBrowseItem(questionnaireDocument, version, allowCensusMode);
        }
    }
}

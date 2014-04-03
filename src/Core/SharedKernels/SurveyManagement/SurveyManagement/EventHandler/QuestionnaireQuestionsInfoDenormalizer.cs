using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class QuestionnaireQuestionsInfoDenormalizer : IEventHandler, IEventHandler<TemplateImported>
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireQuestionsInfo> questionnaires;

        public QuestionnaireQuestionsInfoDenormalizer(IReadSideRepositoryWriter<QuestionnaireQuestionsInfo> questionnaires)
        {
            this.questionnaires = questionnaires;
        }

        public string Name
        {
            get { return this.GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public Type[] BuildsViews
        {
            get { return new[] { typeof(QuestionnaireQuestionsInfo) }; }
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            var map = new QuestionnaireQuestionsInfo
            {
                QuestionIdToVariableMap = evnt.Payload.Source.Find<IQuestion>(question => true).ToDictionary(x => x.PublicKey, x => x.StataExportCaption)
            };
             
            this.questionnaires.Store(map, RepositoryKeysHelper.GetVersionedKey(evnt.EventSourceId, evnt.EventSequence));
        }
    }
}
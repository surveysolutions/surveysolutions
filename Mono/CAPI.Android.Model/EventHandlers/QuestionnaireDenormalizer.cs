using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace CAPI.Android.Core.Model.EventHandlers
{
    public class QuestionnaireDenormalizer : IEventHandler<TemplateImported>
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireDocument> questionnarieStorage;

        public QuestionnaireDenormalizer(IReadSideRepositoryWriter<QuestionnaireDocument> questionnarieStorage)
        {
            this.questionnarieStorage = questionnarieStorage;
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            questionnarieStorage.Store(evnt.Payload.Source, evnt.EventSourceId);
        }
    }
}
﻿using System;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.DataCollection.EventHandler
{
    public class QuestionnaireBrowseItemDenormalizer : IEventHandler<TemplateImported>, IEventHandler
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireBrowseItem> documentStorage;

        public QuestionnaireBrowseItemDenormalizer(IReadSideRepositoryWriter<QuestionnaireBrowseItem> documentStorage)
        {
            this.documentStorage = documentStorage;
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            var document = evnt.Payload.Source;

            var browseItem = new QuestionnaireBrowseItem(document);

            this.documentStorage.Store(browseItem, document.PublicKey);
        }

        public string Name
        {
            get { return GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public Type[] BuildsViews
        {
            get { return new Type[] { typeof(QuestionnaireBrowseItem) }; }
        }
    }
}

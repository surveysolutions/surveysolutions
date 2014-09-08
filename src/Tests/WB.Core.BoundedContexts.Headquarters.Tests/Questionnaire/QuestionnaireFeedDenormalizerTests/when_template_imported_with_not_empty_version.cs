using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Denormalizers;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Tests.Questionnaire.QuestionnaireFeedDenormalizerTests
{
    [Subject(typeof(QuestionnaireFeedDenormalizer))]
    internal class when_template_imported_with_not_empty_version
    {
        Establish context = () =>
        {
            questionnaireId = Guid.NewGuid();

            templateImportedEvent = Create.PublishedEvent(questionnaireId, new TemplateImported() { AllowCensusMode = true, Version = 6});

            writer = Substitute.For<IReadSideRepositoryWriter<QuestionnaireFeedEntry>>();

            denormalizer = Create.QuestionnaireFeedDenormalizer(writer);
        };

        private Because of = () => denormalizer.Handle(templateImportedEvent);

        private It should_write_new_event_to_feed = () =>
            writer.Received().Store(
                Arg.Is<QuestionnaireFeedEntry>(x =>
                    x.QuestionnaireId == templateImportedEvent.EventSourceId &&
                    x.QuestionnaireVersion == templateImportedEvent.Payload.Version &&
                    x.Timestamp == templateImportedEvent.EventTimeStamp &&
                    x.EntryType == QuestionnaireEntryType.QuestionnaireCreatedInCensusMode &&
                    x.EntryId == templateImportedEvent.EventIdentifier.FormatGuid()),
                templateImportedEvent.EventIdentifier.FormatGuid());

        static QuestionnaireFeedDenormalizer denormalizer;
        static IPublishedEvent<TemplateImported> templateImportedEvent;
        static IReadSideRepositoryWriter<QuestionnaireFeedEntry> writer;
        private static Guid questionnaireId;
    }
}

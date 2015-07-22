extern alias datacollection;

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
using WB.Tests.Unit.BoundedContexts.Headquarters;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Questionnaire;

using TemplateImported = datacollection::Main.Core.Events.Questionnaire.TemplateImported;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.QuestionnaireFeedDenormalizerTests
{
    [Subject(typeof(QuestionnaireFeedDenormalizer))]
    internal class when_template_imported
    {
        Establish context = () =>
        {
            questionnaireId = Guid.NewGuid();

            templateImportedEvent = new TemplateImported() { AllowCensusMode = true }.ToPublishedEvent(questionnaireId);

            writer = Substitute.For<IReadSideRepositoryWriter<QuestionnaireFeedEntry>>();

            denormalizer = Create.QuestionnaireFeedDenormalizer(writer);
        };

        Because of = () => denormalizer.Handle(templateImportedEvent);

        It should_write_new_event_to_feed = () =>
            writer.Received().Store(
                Arg.Is<QuestionnaireFeedEntry>(x => 
                    x.QuestionnaireId == templateImportedEvent.EventSourceId &&
                    x.QuestionnaireVersion == templateImportedEvent.EventSequence &&
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

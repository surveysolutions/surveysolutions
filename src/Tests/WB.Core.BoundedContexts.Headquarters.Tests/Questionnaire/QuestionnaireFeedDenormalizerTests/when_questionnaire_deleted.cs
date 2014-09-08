﻿using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Denormalizers;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Tests.Questionnaire.QuestionnaireFeedDenormalizerTests
{
    [Subject(typeof (QuestionnaireFeedDenormalizer))]
    public class when_questionnaire_deleted
    {
        Establish context = () =>
        {
            questionnaireId = Guid.NewGuid();

            questionnaireDeleted = Create.PublishedEvent(questionnaireId, new QuestionnaireDeleted() { QuestionnaireVersion = 1});

            writer = Substitute.For<IReadSideRepositoryWriter<QuestionnaireFeedEntry>>();

            denormalizer = Create.QuestionnaireFeedDenormalizer(writer);
        };

        private Because of = () => denormalizer.Handle(questionnaireDeleted);

        private It should_write_new_event_to_feed = () =>
            writer.Received().Store(
                Arg.Is<QuestionnaireFeedEntry>(x =>
                    x.QuestionnaireId == questionnaireDeleted.EventSourceId &&
                    x.QuestionnaireVersion == questionnaireDeleted.Payload.QuestionnaireVersion &&
                    x.Timestamp == questionnaireDeleted.EventTimeStamp &&
                    x.EntryType == QuestionnaireEntryType.QuestionnaireDeleted &&
                    x.EntryId == questionnaireDeleted.EventIdentifier.FormatGuid()),
                questionnaireDeleted.EventIdentifier.FormatGuid());

        static QuestionnaireFeedDenormalizer denormalizer;
        static IPublishedEvent<QuestionnaireDeleted> questionnaireDeleted;
        static IReadSideRepositoryWriter<QuestionnaireFeedEntry> writer;
        private static Guid questionnaireId;
    }
}


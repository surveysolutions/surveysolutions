using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_RosterChanged_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
            rosterTitleQuestionId = Guid.Parse("22222222222222222222222222222222");
            rosterSizeSource = RosterSizeSourceType.FixedTitles;
            rosterFixedTitles = new[] { new Tuple<decimal, string>(1,rosterFixedTitle1), new Tuple<decimal, string>(2,rosterFixedTitle2) };

            questionnaireDocument = CreateQuestionnaireDocument(
                CreateGroup(groupId: groupId, setup: group =>
                {
                    group.RosterSizeQuestionId = null;
                    group.RosterSizeSource = RosterSizeSourceType.Question;
                })
            );

            @event = CreateRosterChangedEvent(groupId: groupId, rosterSizeQuestionId: rosterSizeQuestionId,
                rosterSizeSource: rosterSizeSource, rosterFixedTitles: rosterFixedTitles, rosterTitleQuestionId: rosterTitleQuestionId);

            var documentStorage = Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(writer
                => writer.GetById(it.IsAny<string>()) == questionnaireDocument);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_set_group_RosterSizeQuestionId_property_to_specified_value = () =>
            questionnaireDocument.GetGroup(groupId)
                .RosterSizeQuestionId.ShouldEqual(rosterSizeQuestionId);

        It should_set_group_RosterSizeSource_property_to_specified_value = () =>
            questionnaireDocument.GetGroup(groupId)
                .RosterSizeSource.ShouldEqual(rosterSizeSource);

        It should_set_group_RosterFixedTitles_property_to_specified_value = () =>
             questionnaireDocument.GetGroup(groupId)
                 .FixedRosterTitles.Select(f=>f.Item2).ShouldEqual(new[] { rosterFixedTitle1, rosterFixedTitle2 });

        It should_set_group_RosterTitleQuestionId_property_to_specified_value = () =>
            questionnaireDocument.GetGroup(groupId)
                .RosterTitleQuestionId.ShouldEqual(rosterTitleQuestionId);


        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<RosterChanged> @event;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid groupId;
        private static Guid rosterSizeQuestionId;
        private static RosterSizeSourceType rosterSizeSource;
        private static Tuple<decimal, string>[] rosterFixedTitles;
        private static string rosterFixedTitle1 = "title1";
        private static string rosterFixedTitle2 = "title2";
        private static Guid rosterTitleQuestionId;
    }
}
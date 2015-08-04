using System;
using System.Collections.Generic;
using System.Linq;

using Machine.Specifications;
using Moq;

using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_all_broken_dependencies_after_group_deleting : QuestionnaireInfoFactoryTestContext
    {
        Establish context = () =>
        {
            questionnaireView = CreateQuestionsAndGroupsCollectionViewWithBrokenLinks();

            questionDetailsReaderMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(questionnaireView);

            expressionProcessorMock
                .Setup(x => x.GetIdentifiersUsedInExpression("q2 == \"bbbb\""))
                .Returns(new List<string> { "q2" });

            expressionProcessorMock
                .Setup(x => x.GetIdentifiersUsedInExpression("q1 > 10"))
                .Returns(new List<string> { "q1" });

            factory = CreateQuestionnaireInfoFactory(
                questionDetailsReaderMock.Object,
                expressionProcessor: expressionProcessorMock.Object);
        };

        Because of = () =>
            result = factory.GetAllBrokenGroupDependencies(questionnaireId, groupId);

        It should_return_not_null_view = () =>
            result.ShouldNotBeNull();

        It should_return_not_null_view1 =
            () => result.Select(x => x.Id).ShouldContainOnly(new string[] { q2Id.FormatGuid(), q4Id.FormatGuid() });

        private static QuestionnaireInfoFactory factory;
        private static List<QuestionnaireItemLink> result;
        private static QuestionsAndGroupsCollectionView questionnaireView;
        private static Mock<IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView>> questionDetailsReaderMock = new Mock<IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView>>();
        private static string questionnaireId = "11111111111111111111111111111111";
        private static Guid groupId = g2Id;
        private static Mock<IExpressionProcessor> expressionProcessorMock = new Mock<IExpressionProcessor>();
    }
}
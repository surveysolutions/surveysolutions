using System;
using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_export_view_factory_from_containing_nested_group_inside_roster : ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionInsideNestedGroupId = Guid.NewGuid();
            rosterId = Guid.NewGuid();
            nestedGroupId = Guid.NewGuid();
            rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");

            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.Numeric
                },
                new Group()
                {
                    PublicKey = rosterId,
                    IsRoster = true,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new Group("title")
                        {
                            PublicKey = nestedGroupId,
                            Children = new List<IComposite>
                            {
                                new NumericQuestion()
                                {
                                    PublicKey = questionInsideNestedGroupId,
                                    QuestionType = QuestionType.Numeric,
                                    StataExportCaption = questionInsideNestedGroupVariableName,
                                    QuestionText = questionInsideNestedGroupTitle
                                }
                            }.ToReadOnlyCollection()
                        }
                    }.ToReadOnlyCollection()
                });

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(Create.Entity.PlainQuestionnaire(questionnaireDocument, 1, null));
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaireDocument);
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);
            BecauseOf();

        }

        public void BecauseOf() =>
            questionnaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(new QuestionnaireIdentity(questionnaireDocument.PublicKey, 1));

        [NUnit.Framework.Test] public void should_create_header_with_1_column_for_question_inside_nested_group () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterSizeQuestionId }].HeaderItems[questionInsideNestedGroupId].ColumnHeaders.Count.Should().Be(1);

        [NUnit.Framework.Test] public void should_create_header_with_1_column_for_question_inside_nested_group_with_name_equal_to_questions_variable_name () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterSizeQuestionId }].HeaderItems[questionInsideNestedGroupId].ColumnHeaders[0].Name
                .Should().Be(questionInsideNestedGroupVariableName);

        [NUnit.Framework.Test] public void should_create_header_with_1_column_for_question_inside_nested_group_with_title_equal_to_questions_title () =>
           questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterSizeQuestionId }].HeaderItems[questionInsideNestedGroupId].ColumnHeaders[0].Title
               .Should().Be(questionInsideNestedGroupTitle);


        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static ExportViewFactory exportViewFactory;
        private static Guid rosterId;
        private static Guid nestedGroupId;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid questionInsideNestedGroupId;
        private static Guid rosterSizeQuestionId;
        private static string questionInsideNestedGroupVariableName="q1";
        private static string questionInsideNestedGroupTitle = "q title";
    }
}

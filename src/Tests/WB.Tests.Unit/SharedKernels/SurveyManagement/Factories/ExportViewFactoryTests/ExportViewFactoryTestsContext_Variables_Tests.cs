using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    [TestFixture]
    internal class ExportViewFactoryTestsContext_Variables_Tests : ExportViewFactoryTestsContext
    {
        [Test]
        public void when_create_export_structure_from_questionnaire_containing_variable()
        {
            var questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(numericTriggerQuestionId),
                Create.Entity.Variable(variableId),
                Create.Entity.Roster(roster1Id, rosterSizeQuestionId: numericTriggerQuestionId, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, children: new IComposite[]
                {
                    Create.Entity.Variable(variableInRosterId),
                    Create.Entity.NumericIntegerQuestion(numericQuestionInRosterId)
                })
            );

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(Create.Entity.PlainQuestionnaire(questionnaireDocument, 1, null));
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaireDocument);
            var exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);


            var questionnaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument.PublicKey, 1);


            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()].LevelLabels.ShouldBeNull();
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()].HeaderItems[variableId].ShouldNotBeNull();
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { roster1Id }].HeaderItems[variableInRosterId].ShouldNotBeNull();
        }

        [TestCase("it is string", VariableType.String, "it is string")]
        [TestCase(789L, VariableType.LongInteger, "789")]
        [TestCase(789.56, VariableType.Double, "789.56")]
        [TestCase(true, VariableType.Boolean, "1")]
        public void when_creating_interview_export_view_by_interview_with_1_variable(object variable, VariableType variableType, string exportResult)
        {
            var interviewData = Create.Entity.InterviewData(variableId, variable);

            var questionnaireDocument =
                Create.Entity.QuestionnaireDocument(children: Create.Entity.Variable(id: variableId, type: variableType));

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(Create.Entity.PlainQuestionnaire(questionnaireDocument, 1, null));
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaireDocument);
            var exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);

            var result = exportViewFactory.CreateInterviewDataExportView(exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument.PublicKey, 1),
                interviewData);

            result.Levels[0].Records[0].GetPlainAnswers().First().ShouldEqual(new[] { exportResult });
        }



        private static readonly Guid numericTriggerQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid numericQuestionInRosterId = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid roster1Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid variableId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid variableInRosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}

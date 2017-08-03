using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.MetadataExportServiceTests
{
    internal class when_question_title_contains_html : MetadataExportServiceTestContext
    {
        [Test]
        public void should_cut_html_tags_from_label()
        {
            var textQuestionId = Id.g1;
            var questionnaire = Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: textQuestionId,
                    text: "text <strong>question</start>",
                    label: "a",
                    instruction: "ttt")
            });

            questionnaire.Title = "main";
            var questionnaireLabelFactoryMock = new Mock<IQuestionnaireLabelFactory>();
            questionnaireLabelFactoryMock.Setup(
                    x => x.CreateLabelsForQuestionnaire(Moq.It.IsAny<QuestionnaireExportStructure>()))
                .Returns(new[]
                {
                    Create.Entity.QuestionnaireLevelLabels("main level",
                        Create.Entity.LabeledVariable(variableName: "txt", questionId: textQuestionId))
                });
        }
    }
}
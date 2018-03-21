using System;
using System.Linq;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_interview_has_multioption_linked_question_answer : ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            multyOptionLinkedQuestionId = Guid.Parse("d7127d06-5668-4fa3-b255-8a2a0aaaa020");
            linkedSourceQuestionId = Guid.NewGuid();

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.Roster(rosterId: Guid.NewGuid(), 
                              variable: "row", 
                              fixedTitles: new[] { "1", "2" },
                              children: new[] {
                                  Create.Entity.TextQuestion(questionId: linkedSourceQuestionId, variable: "varTxt")
                              }),
                Create.Entity.MultyOptionsQuestion(id: multyOptionLinkedQuestionId,
                    variable: "mult",
                    linkedToQuestionId: linkedSourceQuestionId));

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(Create.Entity.PlainQuestionnaire(questionnaire, 1, null));
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaire);
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);

            questionnaaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaire.PublicKey, 1);

            interview = Create.Entity.InterviewData(Create.Entity.InterviewQuestion(multyOptionLinkedQuestionId, new[] { 2 }));
            BecauseOf();
        }

        public void BecauseOf() => result = exportViewFactory.CreateInterviewDataExportView(questionnaaireExportStructure, interview);

        [NUnit.Framework.Test] public void should_put_answers_to_export_in_appropriate_order () 
        {
            InterviewDataExportLevelView first = result.Levels.First();
            var exportedQuestion = first.Records.First().GetPlainAnswers().First();
            exportedQuestion.Length.Should().Be(2);
            exportedQuestion.Should().BeEquivalentTo(new[] {"2", ExportFormatSettings.MissingStringQuestionValue});
        }

        static ExportViewFactory exportViewFactory;
        static QuestionnaireExportStructure questionnaaireExportStructure;
        static InterviewDataExportView result;
        static InterviewData interview;
        static Guid multyOptionLinkedQuestionId;
        static Guid linkedSourceQuestionId;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_interview_has_yesno_question_with_not_sorted_answers : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            questionId = Guid.Parse("d7127d06-5668-4fa3-b255-8a2a0aaaa020");
            variableName = "yesno";
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultyOptionsQuestion(id: questionId,
                    variable: variableName,
                    options: new List<Answer> {
                        Create.Entity.Answer("foo", 28),
                        Create.Entity.Answer("bar", 42),
                        Create.Entity.Answer("blah", 21),
                        Create.Entity.Answer("bar_null", 15)
                    }, areAnswersOrdered: false,
                    yesNoView: true));

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(new PlainQuestionnaire(questionnaire, 1, null));
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaire);
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);

            questionnaaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaire.PublicKey, 1);

            interview = Create.Entity.InterviewData(Create.Entity.InterviewQuestion(questionId, new[]
            {
                Create.Entity.AnsweredYesNoOption(21m, true),
                Create.Entity.AnsweredYesNoOption(42m, false),
                Create.Entity.AnsweredYesNoOption(28m, true),
            }));
        };

        Because of = () => result = exportViewFactory.CreateInterviewDataExportView(questionnaaireExportStructure, interview);

        It should_fill_yesno_question_answer_without_order = () =>
        {
            InterviewDataExportLevelView first = result.Levels.First();
            var exportedQuestion = first.Records.First().GetPlainAnswers().First();
            exportedQuestion.Length.ShouldEqual(4);
            exportedQuestion.ShouldEqual(new[] { "1", "0", "1", ExportFormatSettings.MissingNumericQuestionValue }); // 1 0 1
        };

        static ExportViewFactory exportViewFactory;
        static QuestionnaireExportStructure questionnaaireExportStructure;
        static Guid questionId;
        static InterviewData interview;
        static InterviewDataExportView result;
        static string variableName;
    }
}
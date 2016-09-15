using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_interview_has_multioption_ordered_question_answer : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            questionId = Guid.Parse("d7127d06-5668-4fa3-b255-8a2a0aaaa020");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultyOptionsQuestion(id: questionId, areAnswersOrdered:true, options: new List<Answer> { Create.Entity.Answer("foo", 28), Create.Entity.Answer("bar", 42) }));

            exportViewFactory = CreateExportViewFactory();
            questionnaaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaire, 1);

            interview = Create.Entity.InterviewData(Create.Entity.InterviewQuestion(questionId, new[] { 42m, 28m }));
        };

        Because of = () => result = exportViewFactory.CreateInterviewDataExportView(questionnaaireExportStructure, interview);

        It should_put_answers_to_export = () => result.Levels.Length.ShouldEqual(1);

        It should_put_answers_to_export_in_appropriate_order = () =>
        {
            InterviewDataExportLevelView first = result.Levels.First();
            var exportedQuestion = first.Records.First().GetPlainAnswers().First();
            exportedQuestion.Length.ShouldEqual(2);
            exportedQuestion.ShouldEqual(new[] { "2", "1" });
        };

        static ExportViewFactory exportViewFactory;
        static QuestionnaireExportStructure questionnaaireExportStructure;
        static InterviewDataExportView result;
        static InterviewData interview;
        static Guid questionId;

    }
}
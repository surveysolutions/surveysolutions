﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_interview_has_yesno_question_with_not_sorted_answers : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            questionId = Guid.Parse("d7127d06-5668-4fa3-b255-8a2a0aaaa020");
            variableName = "yesno";
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.MultyOptionsQuestion(id: questionId,
                    variable: variableName,
                    options: new List<Answer> {
                        Create.Answer("foo", 28),
                        Create.Answer("bar", 42),
                        Create.Answer("blah", 21),
                        Create.Answer("bar_null", 15)
                    }, areAnswersOrdered: false,
                    yesNoView: true));

            exportViewFactory = CreateExportViewFactory();
            questionnaaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaire, 1);

            interview = Create.InterviewData(Create.InterviewQuestion(questionId, new[]
            {
                Create.AnsweredYesNoOption(21m, true),
                Create.AnsweredYesNoOption(42m, false),
                Create.AnsweredYesNoOption(28m, true),
            }));
        };

        Because of = () => result = exportViewFactory.CreateInterviewDataExportView(questionnaaireExportStructure, interview);

        It should_fill_yesno_question_answer_without_order = () =>
        {
            InterviewDataExportLevelView first = result.Levels.First();
            var exportedQuestion = first.Records.First().GetQuestions().First();
            exportedQuestion.Answers.Length.ShouldEqual(4);
            exportedQuestion.Answers.SequenceEqual(new[] { "1", "0", "1", "" }).ShouldBeTrue(); // 1 0 1
        };

        static ExportViewFactory exportViewFactory;
        static QuestionnaireExportStructure questionnaaireExportStructure;
        static Guid questionId;
        static InterviewData interview;
        static InterviewDataExportView result;
        static string variableName;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_building_answer_from_string_array_for_yesno_qustion : QuestionDataParserTestContext
    {
        Establish context = () =>
        {
            questionDataParser = CreateQuestionDataParser();
            question = new MultyOptionsQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.MultyOption,
                YesNoView = true,
                Answers = new List<Answer> {
                    new Answer { AnswerValue = "1", AnswerText = "a" },
                    new Answer { AnswerValue = "2", AnswerText = "b" },
                    new Answer { AnswerValue = "3", AnswerText = "c" }
                },
                StataExportCaption = questionVarName
            };

            answerWithColumns = new[]
            {
                Tuple.Create(questionVarName + "__1", "1"),
                Tuple.Create(questionVarName + "__1", ""),
                Tuple.Create(questionVarName + "__3", "0")
            };
        };
        
        Because of = () => result = questionDataParser.BuildAnswerFromStringArray(answerWithColumns, question);

        It should_return_array_of_yesno_answers = () =>
            result.ShouldBeOfExactType<YesNoAnswer>();

        It should_set_yes_to_yes_the_option = () => ((YesNoAnswer)result).CheckedOptions.First().Yes.ShouldBeTrue();

        It should_set_no_to_no_option = () => ((YesNoAnswer)result).CheckedOptions.Last().Yes.ShouldBeFalse();

        It should_not_add_not_answered_option = () => ((YesNoAnswer)result).CheckedOptions.Count.ShouldEqual(2);

        static Tuple<string, string>[] answerWithColumns;
    }
}
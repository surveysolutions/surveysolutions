using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_cloning_questionnaire_and_new_title_consists_only_of_spaces : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = SetupQuestionnaireWithProjectionsReadyForCloning(questionnaireIdentity);
        };

        Because of = () =>
            questionnaireException = Catch.Only<QuestionnaireException>(() =>
                questionnaire.CloneQuestionnaire(Create.Command.CloneQuestionnaire(
                    questionnaireIdentity: questionnaireIdentity, newTitle: "   ")));

        It should_throw_QuestionnaireException_containing_specific_words = () =>
            questionnaireException.Message.ToLower().ToSeparateWords().ShouldContain("title", "empty");

        private static QuestionnaireException questionnaireException;
        private static Questionnaire questionnaire;
        private static QuestionnaireIdentity questionnaireIdentity
            = Create.QuestionnaireIdentity(Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"), 3);
    }
}
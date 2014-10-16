using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_migrating_expressions_to_csharp_twice : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            Setup.SimpleQuestionnaireDocumentUpgraderToMockedServiceLocator();

            questionnaire = Create.Questionnaire();
        };

        Because of = () =>
            exception = Catch.Exception(() =>
            {
                questionnaire.MigrateExpressionsToCSharp();
                questionnaire.MigrateExpressionsToCSharp();
            });

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containing__expressions____migrated__ = () =>
            exception.Message.ToLower().ToSeparateWords().ShouldContain("expressions", "migrated");

        private static Exception exception;
        private static Questionnaire questionnaire;
    }
}
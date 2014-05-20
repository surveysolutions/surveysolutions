using System;
using Machine.Specifications;

namespace Questionnaire.Core.Web.Security.Tests.QuestionaireRoleProviderTests
{
    internal class when_check_that_user_is_in_role_and_user_does_not_exist_in_cache : QuestionnaireRoleProviderTestsContext
    {
        Establish context = () =>
        {
            provider = CreateProvider();
        };

        Because of = () => 
            exception = Catch.Exception(() => provider.IsUserInRole("some_user_name", "some_role"));

        It should_exception_be_null =
            exception.ShouldBeNull;

        private static QuestionnaireRoleProvider provider;
        private static Exception exception;
    }
}

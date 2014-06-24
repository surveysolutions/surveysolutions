using System.Collections.Specialized;
using Machine.Specifications;
using It = Machine.Specifications.It;

namespace Questionnaire.Core.Web.Security.Tests.QuestionnaireMembershipProviderTests
{
    internal class when_initialize_and_all_provider_params_specified : QuestionnaireMembershipProviderTestsContext
    {
        Establish context = () =>
        {
            provider = CreateProvider();
        };

        Because of = () =>
            provider.Initialize(string.Empty, config);

        It should_ApplicationName_be_equal_to_specified_ApplicationName = () =>
            provider.ApplicationName.ShouldEqual(ApplicationName);

        It should_EnablePasswordReset_be_equal_to_specified_EnablePasswordReset = () =>
            provider.EnablePasswordReset.ShouldEqual(EnablePasswordReset);

        It should_EnablePasswordRetrieval_be_equal_to_specified_EnablePasswordRetrieval = () =>
            provider.EnablePasswordRetrieval.ShouldEqual(EnablePasswordRetrieval);

        It should_RequiresQuestionAndAnswer_be_equal_to_specified_RequiresQuestionAndAnswer = () =>
            provider.RequiresQuestionAndAnswer.ShouldEqual(RequiresQuestionAndAnswer);

        It should_RequiresUniqueEmail_be_equal_to_specified_RequiresUniqueEmail = () =>
            provider.RequiresUniqueEmail.ShouldEqual(RequiresUniqueEmail);

        It should_MaxInvalidPasswordAttempts_be_equal_to_specified_MaxInvalidPasswordAttempts = () =>
            provider.MaxInvalidPasswordAttempts.ShouldEqual(MaxInvalidPasswordAttempts);

        It should_PasswordAttemptWindow_be_equal_to_specified_PasswordAttemptWindow = () =>
            provider.PasswordAttemptWindow.ShouldEqual(PasswordAttemptWindow);

        It should_MinRequiredPasswordLength_be_equal_to_specified_MinRequiredPasswordLength = () =>
            provider.MinRequiredPasswordLength.ShouldEqual(MinRequiredPasswordLength);

        It should_MinRequiredNonAlphanumericCharacters_be_equal_to_specified_MinRequiredNonalphanumericCharacters = () =>
            provider.MinRequiredNonAlphanumericCharacters.ShouldEqual(MinRequiredNonalphanumericCharacters);

        private static QuestionnaireMembershipProvider provider;

        private const string ApplicationName ="app name";
        private const bool EnablePasswordReset = true;
        private const bool EnablePasswordRetrieval = true;
        private const bool RequiresQuestionAndAnswer = true;
        private const bool RequiresUniqueEmail = true;
        private const int MaxInvalidPasswordAttempts = 100;
        private const int PasswordAttemptWindow = 100;
        private const int MinRequiredPasswordLength = 10;
        private const int MinRequiredNonalphanumericCharacters = 5;

        private static NameValueCollection config = new NameValueCollection()
        {
            {"applicationName", ApplicationName},
            {"enablePasswordReset", EnablePasswordReset.ToString()},
            {"enablePasswordRetrieval", EnablePasswordRetrieval.ToString()},
            {"requiresQuestionAndAnswer", RequiresQuestionAndAnswer.ToString()},
            {"requiresUniqueEmail", RequiresUniqueEmail.ToString()},
            {"maxInvalidPasswordAttempts", MaxInvalidPasswordAttempts.ToString()},
            {"passwordAttemptWindow", PasswordAttemptWindow.ToString()},
            {"minRequiredPasswordLength", MinRequiredPasswordLength.ToString()},
            {"minRequiredNonalphanumericCharacters", MinRequiredNonalphanumericCharacters.ToString()},
        };
    }
}

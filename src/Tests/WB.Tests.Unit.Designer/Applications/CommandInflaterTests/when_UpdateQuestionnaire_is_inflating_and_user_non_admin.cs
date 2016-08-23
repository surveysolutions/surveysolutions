using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Designer.Code.Implementation;
using WB.UI.Shared.Web.Membership;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.Designer.Applications.CommandInflaterTests
{
    internal class when_UpdateQuestionnaire_is_inflating_and_user_non_admin : CommandInflaterTestsContext
    {
        Establish context = () =>
        {
            var membershipUserService = Mock.Of<IMembershipUserService>(
                _ => _.WebUser == Mock.Of<IMembershipWebUser>(u => u.IsAdmin == false));

            var questionnaire = CreateQuestionnaireDocument(questoinnaireId, questionnaiteTitle, ownerId);

            var documentStorage = Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(storage
                    => storage.GetById(it.IsAny<string>()) == questionnaire);

            command = new UpdateQuestionnaire(questoinnaireId, questionnaiteTitle, isPublic: true, responsibleId: actionUserId);

            commandInflater = CreateCommandInflater(membershipUserService, documentStorage);
        };

        Because of = () => exception = Catch.Exception(() => commandInflater.PrepareDeserializedCommandForExecution(command));

        It should_be_throwed_command_inflating_exception_with_specified_type_and_messge = () =>
        {
            var commandInflaitingException = exception as CommandInflaitingException;
            commandInflaitingException.ExceptionType.ShouldEqual(CommandInflatingExceptionType.Forbidden);
            commandInflaitingException.Message.ShouldEqual("You don't have permissions to make questionnaire as public");
        };
          

        private static CommandInflater commandInflater;
        private static UpdateQuestionnaire command;
        private static readonly Guid questoinnaireId = Guid.Parse("13333333333333333333333333333333");
        
        private static string questionnaiteTitle = "questionnaire title";
        private static readonly Guid actionUserId = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid ownerId = Guid.Parse("11111111111111111111111111111111");
        private static Exception exception;
    }
}
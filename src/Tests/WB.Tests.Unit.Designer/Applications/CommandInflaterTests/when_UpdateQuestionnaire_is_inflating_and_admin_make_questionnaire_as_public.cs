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
    internal class when_UpdateQuestionnaire_is_inflating_and_admin_make_questionnaire_as_public : CommandInflaterTestsContext
    {
        Establish context = () =>
        {
            var membershipUserService = Mock.Of<IMembershipUserService>(
                _ => _.WebUser == Mock.Of<IMembershipWebUser>(u => u.IsAdmin == true));

            var questionnaire = CreateQuestionnaireDocument(questoinnaireId, questionnaiteTitle, adminId);

            var documentStorage = Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(storage
                    => storage.GetById(it.IsAny<string>()) == questionnaire);

            command = new UpdateQuestionnaire(questoinnaireId, questionnaiteTitle, isPublic: true, responsibleId: adminId);

            commandInflater = CreateCommandInflater(membershipUserService, documentStorage);
        };

        Because of = () => exception = Catch.Exception(() => commandInflater.PrepareDeserializedCommandForExecution(command));

        It should_not_throw_any_command_inflating_exceptions = () => exception.ShouldBeNull();  

        private static CommandInflater commandInflater;
        private static UpdateQuestionnaire command;
        private static readonly Guid questoinnaireId = Guid.Parse("13333333333333333333333333333333");
        
        private static string questionnaiteTitle = "questionnaire title";
        private static readonly Guid adminId = Guid.Parse("11111111111111111111111111111111");
        private static Exception exception;
    }
}
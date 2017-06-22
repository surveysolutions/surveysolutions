using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.CommandDeserialization;

namespace WB.Tests.Unit.Designer.Applications.CommandDeserializerTests
{
    internal class when_deserializing_command_of_type__DeleteGroup__with_questionnaire_id_and_group_id : CommandDeserializerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            type = "DeleteGroup";

            questionnaireId = "11111111-1111-1111-1111-111111111111";
            groupId = "22222222-2222-2222-2222-222222222222";

            command = string.Format(@"{{
                ""questionnaireId"": ""{0}"",
                ""groupId"": ""{1}""
            }}", questionnaireId, groupId);

            deserializer = CreateCommandDeserializer();
            BecauseOf();
        }

        private void BecauseOf() =>
            result = deserializer.Deserialize(type, command);

        [NUnit.Framework.Test] public void should_return_NewDeleteGroupCommand () =>
            result.ShouldBeOfExactType<DeleteGroup>();

        [NUnit.Framework.Test] public void should_return_same_questionnaire_id_in_NewDeleteGroupCommand () =>
            ((DeleteGroup)result).QuestionnaireId.ShouldEqual(Guid.Parse(questionnaireId));

        [NUnit.Framework.Test] public void should_return_same_group_id_in_NewDeleteGroupCommand () =>
            ((DeleteGroup)result).GroupId.ShouldEqual(Guid.Parse(groupId));

        private static ICommand result;
        private static DesignerCommandDeserializer deserializer;
        private static string questionnaireId;
        private static string groupId;
        private static string type;
        private static string command;
    }
}
using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.Tests.Unit.Designer.Applications.CommandDeserializerTests
{
    internal class when_deserializing_command_of_type__UpdateGroup__with_questionnaire_id_and_group_id_and_title_and_propogation_kind_and_description_and_condition : CommandDeserializerTestsContext
    {
        Establish context = () =>
        {
            type = "UpdateGroup";

            title = @"<b width='7'>MA<font color='red'>IN</font></b><img /><script> alert('hello world!')</script><script/>";
            questionnaireId = "11111111-1111-1111-1111-111111111111";
            groupId = "22222222-2222-2222-2222-222222222222";
            propagationKind = "AutoPropagated";
            condition = "1 == 2";

            command = $@"{{
                ""questionnaireId"": ""{questionnaireId}"",
                ""groupId"": ""{groupId}"",
                ""title"": ""{title}"",
                ""propagationKind"": ""{propagationKind}"",
                ""condition"": ""{condition}""
            }}";

            deserializer = CreateCommandDeserializer();
        };

        Because of = () =>
            result = deserializer.Deserialize(type, command);

        It should_return_NewUpdateGroupCommand = () =>
            result.ShouldBeOfExactType<UpdateGroup>();

        It should_return_same_title_in_NewUpdateGroupCommand = () =>
            ((UpdateGroup)result).Title.ShouldEqual(sanitizedTitle);

        It should_return_same_questionnaire_id_in_NewUpdateGroupCommand = () =>
            ((UpdateGroup)result).QuestionnaireId.ShouldEqual(Guid.Parse(questionnaireId));

        It should_return_same_group_id_in_NewUpdateGroupCommand = () =>
            ((UpdateGroup)result).GroupId.ShouldEqual(Guid.Parse(groupId));

        It should_return_same_condition_in_NewUpdateGroupCommand = () =>
            ((UpdateGroup)result).Condition.ShouldEqual(condition);

        private static ICommand result;
        private static CommandDeserializer deserializer;
        private static string command;
        private static string title;
        private static string sanitizedTitle = "MAIN alert('hello world!')"; 
        private static string questionnaireId;
        private static string groupId;
        private static string propagationKind;
        private static string condition;
        private static string type;
    }
}
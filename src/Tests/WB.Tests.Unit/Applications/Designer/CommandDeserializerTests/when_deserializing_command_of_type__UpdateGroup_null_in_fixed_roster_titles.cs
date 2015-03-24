using System;
using System.Linq;
using Machine.Specifications;
using Ncqrs.Commanding;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.Tests.Unit.Applications.Designer.CommandDeserializerTests
{
    internal class when_deserializing_command_of_type__UpdateGroup_null_in_fixed_roster_titles : CommandDeserializerTestsContext
    {
        Establish context = () =>
        {
            type = "UpdateGroup";

            title = @"<b width='7'>MA<font color='red'>IN</font></b><img /><script>alert('hello world!')</script><script/>";
            questionnaireId = "11111111-1111-1111-1111-111111111111";
            groupId = "22222222-2222-2222-2222-222222222222";
            propagationKind = "AutoPropagated";
            rosterFixedTitles = "[]";

            command = string.Format(@"{{
                ""questionnaireId"": ""{0}"",
                ""groupId"": ""{1}"",
                ""title"": ""{2}"",
                ""propagationKind"": ""{3}"",
                ""rosterSizeSource"":""FixedTitles"",
                ""fixedRosterTitles"": {4}
            }}", questionnaireId, groupId, title, propagationKind, rosterFixedTitles);

            deserializer = CreateCommandDeserializer();
        };

        Because of = () =>
            result = deserializer.Deserialize(type, command);

        It should_return_NewUpdateGroupCommand = () =>
            result.ShouldBeOfExactType<UpdateGroupCommand>();

        It should_return_same_title_in_NewUpdateGroupCommand = () =>
            ((UpdateGroupCommand)result).Title.ShouldEqual(sanitizedTitle);

        It should_return_same_questionnaire_id_in_NewUpdateGroupCommand = () =>
            ((UpdateGroupCommand)result).QuestionnaireId.ShouldEqual(Guid.Parse(questionnaireId));

        It should_return_same_group_id_in_NewUpdateGroupCommand = () =>
            ((UpdateGroupCommand)result).GroupId.ShouldEqual(Guid.Parse(groupId));

        It should_return_0_fixed_roster_titles = () =>
            ((UpdateGroupCommand)result).FixedRosterTitles.Count().ShouldEqual(0);

        private static ICommand result;
        private static CommandDeserializer deserializer;
        private static string command;
        private static string title;
        private static string sanitizedTitle = "MAIN alert('hello world!')";
        private static string questionnaireId;
        private static string groupId;
        private static string propagationKind;
        private static string rosterFixedTitles;
        private static string type;
    }
}
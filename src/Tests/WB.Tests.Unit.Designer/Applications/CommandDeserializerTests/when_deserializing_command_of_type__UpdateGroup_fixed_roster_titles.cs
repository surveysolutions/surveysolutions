using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.Tests.Unit.Designer.Applications.CommandDeserializerTests
{
    internal class when_deserializing_command_of_type__UpdateGroup_fixed_roster_titles : CommandDeserializerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            type = "UpdateGroup";

            title = @"<b width='7'>MA<font color='red'>IN</font></b><img /><script> alert('hello world!')</script><script/>";
            questionnaireId = "11111111-1111-1111-1111-111111111111";
            groupId = "22222222-2222-2222-2222-222222222222";
            propagationKind = "AutoPropagated";
            rosterFixedTitles =
                @"[{""value"":1.0,""title"":""привет, <style>Мир!</script>""},{""value"":2.0,""title"":""<span>hi, <b><i>Hello!</b></span>""}]";

            command = string.Format(@"{{
                ""questionnaireId"": ""{0}"",
                ""groupId"": ""{1}"",
                ""title"": ""{2}"",
                ""propagationKind"": ""{3}"",
                ""rosterSizeSource"":""FixedTitles"",
                ""fixedRosterTitles"": {4}
            }}", questionnaireId, groupId, title, propagationKind, rosterFixedTitles);

            deserializer = CreateCommandDeserializer();
        }

        private void BecauseOf() =>
            result = deserializer.Deserialize(type, command);

        [NUnit.Framework.Test] public void should_return_NewUpdateGroupCommand () =>
            result.ShouldBeOfExactType<UpdateGroup>();

        [NUnit.Framework.Test] public void should_return_same_title_in_NewUpdateGroupCommand () =>
            ((UpdateGroup)result).Title.ShouldEqual(sanitizedTitle);

        [NUnit.Framework.Test] public void should_return_same_questionnaire_id_in_NewUpdateGroupCommand () =>
            ((UpdateGroup)result).QuestionnaireId.ShouldEqual(Guid.Parse(questionnaireId));

        [NUnit.Framework.Test] public void should_return_same_group_id_in_NewUpdateGroupCommand () =>
            ((UpdateGroup)result).GroupId.ShouldEqual(Guid.Parse(groupId));

        [NUnit.Framework.Test] public void should_return_2_fixed_roster_titles () =>
            ((UpdateGroup)result).FixedRosterTitles.Count().ShouldEqual(2);

        [NUnit.Framework.Test] public void should_return_sanizited_first_fixed_title () =>
            ((UpdateGroup)result).FixedRosterTitles[0].Title.ShouldEqual("привет, Мир!");

        [NUnit.Framework.Test] public void should_return_sanizited_second_fixed_title () =>
            ((UpdateGroup)result).FixedRosterTitles[1].Title.ShouldEqual("hi, Hello!");
        
        [NUnit.Framework.Test] public void should_return_first_fixed_roster_title_value_1_0 () =>
            ((UpdateGroup)result).FixedRosterTitles[0].Value.ShouldEqual("1.0");

        [NUnit.Framework.Test] public void should_return_second_fixed_roster_title_value_2_0 () =>
            ((UpdateGroup)result).FixedRosterTitles[1].Value.ShouldEqual("2.0");


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
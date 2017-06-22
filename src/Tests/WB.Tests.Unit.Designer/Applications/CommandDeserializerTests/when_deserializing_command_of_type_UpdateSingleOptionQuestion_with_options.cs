using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.Tests.Unit.Designer.Applications.CommandDeserializerTests
{
    internal class when_deserializing_command_of_type_UpdateSingleOptionQuestion_with_options : CommandDeserializerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            type = "UpdateSingleOptionQuestion";
            
            command = string.Format(@"{{
                ""questionnaireId"": ""2718d434ee18477cb64eb1138540b174"",
                ""groupId"": ""81e5e36c4423c5acad2bc16850b0989d"",
                ""commonQuestionParameters"": {{
                    ""title"": ""Title"",
                    ""variableLabel"": null,
                    ""enablementCondition"":  """",
                    ""hideIfDisabled"":  true,
                    ""instructions"":  """",
                }},
                ""type"": ""SingleOption"",
                ""mask"": null,
                ""validationExpression"":  """",
                ""validationMessage"":  """",
                ""isPreFilled"": false,
                ""scope"": ""Interviewer"",
                ""areAnswersOrdered"": false,
                ""maxAllowedAnswers"": null,
                ""linkedToQuestionId"": null,
                ""isFilteredCombobox"": false,
                ""cascadeFromQuestionId"": ""81e5e36c4423c5acad2bc16850b0989d"",
                ""options"": [{{
                    ""value"": 1,
                    ""parentValue"": 1,
                    ""title"": ""Street 1"",
                    ""id"": ""6146792ec78dd9367f19540979d971a7""
                }}]
            }}");

            deserializer = CreateCommandDeserializer();
            BecauseOf();
        }

        private void BecauseOf() =>
            result = deserializer.Deserialize(type, command);

        [NUnit.Framework.Test] public void should_return_UpdateSingleOptionQuestionCommand () =>
            result.ShouldBeOfExactType<UpdateSingleOptionQuestion>();

        private static ICommand result;
        private static CommandDeserializer deserializer;
        private static string command;
        private static string type;
    }
}
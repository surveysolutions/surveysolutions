using Machine.Specifications;
using Ncqrs.Commanding;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.SingleOption;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.Tests.Unit.Applications.Designer.CommandDeserializerTests
{
    internal class when_deserializing_command_of_type_UpdateSingleOptionQuestion_with_options : CommandDeserializerTestsContext
    {
        Establish context = () =>
        {
            type = "UpdateSingleOptionQuestion";
            
            command = string.Format(@"{{
                ""questionnaireId"": ""2718d434ee18477cb64eb1138540b174"",
                ""groupId"": ""81e5e36c4423c5acad2bc16850b0989d"",
                ""title"": ""Title"",
                ""type"": ""SingleOption"",
                ""variableLabel"": null,
                ""mask"": null,
                ""isMandatory"": false,
                ""enablementCondition"":  """",
                ""validationExpression"":  """",
                ""validationMessage"":  """",
                ""instructions"":  """",
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
        };

        Because of = () =>
            result = deserializer.Deserialize(type, command);

        It should_return_UpdateSingleOptionQuestionCommand = () =>
            result.ShouldBeOfExactType<UpdateSingleOptionQuestionCommand>();

        private static ICommand result;
        private static CommandDeserializer deserializer;
        private static string command;
        private static string type;
    }
}
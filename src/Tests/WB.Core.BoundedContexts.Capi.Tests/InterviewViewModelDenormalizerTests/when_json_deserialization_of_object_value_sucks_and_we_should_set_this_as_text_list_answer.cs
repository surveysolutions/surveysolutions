using System.Linq;
using Machine.Specifications;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;

namespace WB.Core.BoundedContexts.Capi.Tests.InterviewViewModelDenormalizerTests
{
    internal class when_json_deserialization_of_object_value_sucks_and_we_should_set_this_as_text_list_answer : TextListQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            model = CreateTextListQuestionViewModel();

            var payload = @"[
              {
                ""$type"": ""System.Tuple`2[[System.Decimal, mscorlib],[System.String, mscorlib]], mscorlib"",
                ""Item1"": 1.0,
                ""Item2"": ""ZUBEDA JUMA""
              },
              {
                ""$type"": ""System.Tuple`2[[System.Decimal, mscorlib],[System.String, mscorlib]], mscorlib"",
                ""Item1"": 2.0,
                ""Item2"": ""OMARI SHABANI""
              },
              {
                ""$type"": ""System.Tuple`2[[System.Decimal, mscorlib],[System.String, mscorlib]], mscorlib"",
                ""Item1"": 3.0,
                ""Item2"": ""AISHA RAMADHANI""
              },
              {
                ""$type"": ""System.Tuple`2[[System.Decimal, mscorlib],[System.String, mscorlib]], mscorlib"",
                ""Item1"": 4.0,
                ""Item2"": ""DONALD BAHATI""
              },
              {
                ""$type"": ""System.Tuple`2[[System.Decimal, mscorlib],[System.String, mscorlib]], mscorlib"",
                ""Item1"": 5.0,
                ""Item2"": ""MOHAMED AKIDA""
              },
              {
                ""$type"": ""System.Tuple`2[[System.Decimal, mscorlib],[System.String, mscorlib]], mscorlib"",
                ""Item1"": 6.0,
                ""Item2"": ""REHEMA MOHAMED""
              }]";

            answer = JsonConvert.DeserializeObject<object>(payload, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                NullValueHandling = NullValueHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal
            });

        };

        Because of = () =>
            model.SetAnswer(answer);

        It should_set_object_of_TextListAnswerViewModel_type_as_answer = () =>
            (model.AnswerObject).ShouldNotBeOfExactType<TextListAnswerViewModel[]>();

        It should_set_6_items_into_array = () =>
            model.ListAnswers.Count().ShouldEqual(6);

        It should_set_values_from_1_to_6_as_values = () =>
            model.ListAnswers.Select(x => x.Value).ShouldContainOnly(1m, 2m, 3m, 4m, 5m, 6m);

        It should_set_specified_6_names_as_answers = () =>
            model.ListAnswers.Select(x => x.Answer).ShouldContainOnly("REHEMA MOHAMED", "MOHAMED AKIDA", "ZUBEDA JUMA", "OMARI SHABANI", "AISHA RAMADHANI", "DONALD BAHATI");

        private static TextListQuestionViewModel model;
        private static object answer;
    }
}
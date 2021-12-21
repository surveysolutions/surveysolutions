using System;
using FluentAssertions;
using Main.Core.Documents;
using Newtonsoft.Json;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.GenericSubdomains.Utils.NewtonJsonUtilsTests
{
    internal class when_serializing_questionnaire_document_with_json_utils_to_old_format 
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericQuestion(questionId: Guid.Parse("55555555555555555555555555555555"), variableName: "DUNo", prefilled: true, isInteger: true),
                Create.Entity.TextQuestion(questionId: Guid.Parse("33333333333333333333333333333333"), variable: "Prov", preFilled: true)
            );
            questionnaire.Id = "questionnaireId";
            questionnaire.CreationDate = new DateTime(2015, 03, 22, 12, 55, 30);
            questionnaire.LastEntryDate = new DateTime(2015, 03, 22, 12, 57, 30);
            questionnaire.PublicKey = Guid.Parse("11111111111111111111111111111111");
            BecauseOf();
        }

        public void BecauseOf() =>
            result = JsonConvert.SerializeObject(questionnaire, JsonSerializerSettingsNewToOld);

        [NUnit.Framework.Test] public void should_return_not_null_result () =>
            result.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_return_correct_deserialize_result () =>
            result.Should().NotContain(", WB.Core.SharedKernels.Questionnaire");

       static QuestionnaireDocument questionnaire;
       static readonly JsonSerializerSettings JsonSerializerSettingsNewToOld = new JsonSerializerSettings()
       {
           TypeNameHandling = TypeNameHandling.Objects,
           NullValueHandling = NullValueHandling.Ignore,
           FloatParseHandling = FloatParseHandling.Decimal,
           Formatting = Formatting.None,
           SerializationBinder = new NewToOldAssemblyRedirectSerializationBinder()
       };
       static string result;
    }
}

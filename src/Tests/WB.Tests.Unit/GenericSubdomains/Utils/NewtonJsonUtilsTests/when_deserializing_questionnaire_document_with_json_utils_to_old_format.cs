using System;
using System.Net.Http;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.Native.Storage;

namespace WB.Tests.Unit.GenericSubdomains.Utils.NewtonJsonUtilsTests
{
    internal class when_serializing_questionnaire_document_with_json_utils_to_old_format : NewtonJsonUtilsTestContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.NumericQuestion(questionId: Guid.Parse("55555555555555555555555555555555"), variableName: "DUNo", prefilled: true, isInteger: true),
                Create.TextQuestion(questionId: Guid.Parse("33333333333333333333333333333333"), variable: "Prov", preFilled: true)
            );
            questionnaire.Id = "questionnaireId";
            questionnaire.CreationDate = new DateTime(2015, 03, 22, 12, 55, 30);
            questionnaire.LastEntryDate = new DateTime(2015, 03, 22, 12, 57, 30);
            questionnaire.PublicKey = Guid.Parse("11111111111111111111111111111111");

            _jsonSerializer = CreateNewtonJsonUtils(new JsonSerializerSettingsFactory());
        };

        Because of = () =>
            result = _jsonSerializer.Serialize(questionnaire, SerializationBinderSettings.NewToOld);

        It should_return_not_null_result = () =>
            result.ShouldNotBeNull();

        It should_return_correct_deserialize_result = () =>
            result.ShouldNotContain(", WB.Core.SharedKernels.Questionnaire");

       static QuestionnaireDocument questionnaire;
       static NewtonJsonSerializer _jsonSerializer;
       static string result;
    }
}
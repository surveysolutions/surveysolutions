using System;
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
            result.ShouldEqual(resultQuestionnaire);


        private const string resultQuestionnaire = @"{""$type"":""Main.Core.Documents.QuestionnaireDocument, Main.Core"",""Id"":""questionnaireId"",""Children"":[{""$type"":""Main.Core.Entities.SubEntities.Group, Main.Core"",""Children"":[{""$type"":""Main.Core.Entities.SubEntities.Question.NumericQuestion, Main.Core"",""IsInteger"":true,""Answers"":[],""Capital"":false,""Children"":[],""HideIfDisabled"":false,""Featured"":true,""PublicKey"":""55555555-5555-5555-5555-555555555555"",""QuestionScope"":0,""QuestionType"":4,""StataExportCaption"":""DUNo"",""ValidationConditions"":[]},{""$type"":""Main.Core.Entities.SubEntities.Question.TextQuestion, Main.Core"",""Answers"":[],""Capital"":false,""Children"":[],""HideIfDisabled"":false,""Featured"":true,""PublicKey"":""33333333-3333-3333-3333-333333333333"",""QuestionScope"":0,""QuestionText"":""Question T"",""QuestionType"":7,""StataExportCaption"":""Prov"",""ValidationConditions"":[]}],""ConditionExpression"":"""",""HideIfDisabled"":false,""Enabled"":true,""Description"":"""",""IsRoster"":false,""RosterSizeSource"":0,""RosterFixedTitles"":[],""FixedRosterTitles"":[],""PublicKey"":""00000000-0000-0000-0000-000000000000"",""Title"":""Chapter""}],""Macros"":{""$type"":""System.Collections.Generic.Dictionary`2[[System.Guid, mscorlib],[WB.Core.SharedKernels.SurveySolutions.Documents.Macro, Main.Core]], mscorlib""},""LookupTables"":{""$type"":""System.Collections.Generic.Dictionary`2[[System.Guid, mscorlib],[WB.Core.SharedKernels.SurveySolutions.Documents.LookupTable, Main.Core]], mscorlib""},""Attachments"":[],""ConditionExpression"":"""",""HideIfDisabled"":false,""CreationDate"":""2015-03-22T12:55:30"",""LastEntryDate"":""2015-03-22T12:57:30"",""IsDeleted"":false,""IsPublic"":false,""UsesCSharp"":false,""PublicKey"":""11111111-1111-1111-1111-111111111111"",""IsRoster"":false,""RosterSizeSource"":0,""FixedRosterTitles"":[],""SharedPersons"":[],""LastEventSequence"":0}";

        private static QuestionnaireDocument questionnaire;
        private static NewtonJsonSerializer _jsonSerializer;
        private static string result;
    }
}
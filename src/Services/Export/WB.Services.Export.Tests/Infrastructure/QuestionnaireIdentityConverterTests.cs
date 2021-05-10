using System;
using System.Collections.Generic;
using ApprovalTests;
using ApprovalTests.Reporters;
using ApprovalTests.Reporters.TestFrameworks;
using Newtonsoft.Json;
using NUnit.Framework;
using WB.Services.Export.Models;
using WB.Services.Infrastructure.EventSourcing.Json;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Tests.Infrastructure
{
    [UseReporter(typeof(DiffReporter), typeof(NUnitReporter))]
    public class QuestionnaireIdentityConverterTests
    {
        private JsonSerializerSettings jsonSerializerSettings = new()
        {
            Converters = new List<JsonConverter> {new QuestionnaireIdentityConverter()}
        };

        [Test]
        public void should_be_able_to_serialize()
        {
            QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(
                $"{Id.g1:N}$5");

            string json = JsonConvert.SerializeObject(questionnaireIdentity, jsonSerializerSettings);

            Approvals.Verify(json);
        }

        [Test]
        public void should_be_able_to_deserialize_old_args_format()
        {
            string json = ResourceHelper.ReadResourceFile("WB.Services.Export.Tests.Infrastructure.args.json");

            var deserializeObject = JsonConvert.DeserializeObject<DataExportProcessArgs>(
                json,
                jsonSerializerSettings);
            Assert.That(deserializeObject, Is.Not.Null,
                $"Should be able to deserialize {nameof(DataExportProcessArgs)}");
            Assert.That(deserializeObject.ExportSettings.QuestionnaireId.Id,
                Is.EqualTo(Guid.Parse("1949c957f8914b93be7837cf41baa0a9")));
            Assert.That(deserializeObject.ExportSettings.QuestionnaireId.Version, Is.EqualTo(1));
        }

        [Test]
        public void should_be_able_to_serialize_and_deserialize_args()
        {
            var dataExportProcessArgs = Create.Entity.DataExportProcessArgs();
            dataExportProcessArgs.ExportSettings.QuestionnaireId = new QuestionnaireIdentity(Id.g2, 5);

            var json = JsonConvert.SerializeObject(dataExportProcessArgs, jsonSerializerSettings);
            var deserialized = JsonConvert.DeserializeObject<DataExportProcessArgs>(json);
            
            Assert.That(deserialized.ExportSettings.QuestionnaireId.Id, Is.EqualTo(Id.g2));
            Assert.That(deserialized.ExportSettings.QuestionnaireId.Version, Is.EqualTo(5));
        }
    }
}

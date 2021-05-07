using Newtonsoft.Json;
using NUnit.Framework;
using WB.Services.Export.Models;
using WB.Services.Export.Services.Processing;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Tests.ExportJobTests
{
    public class DataExportJobsSerializationTest
    {
        private DataExportProcessArgs args;

        [SetUp]
        public void Deserialize()
        {
            this.args = JsonConvert.DeserializeObject<DataExportProcessArgs>(@"{
    ""ExportSettings"": {
        ""ExportFormat"": 1,
        ""QuestionnaireId"": {
            ""Id"": ""6158dd074d64498f8a50e5e9828fda23$101""
        },
        ""Status"": null,
        ""FromDate"": null,
        ""ToDate"": null,
        ""Tenant"": {
            ""BaseUrl"": ""http://localhost:5001/"",
            ""Id"": {
                ""Id"": ""021d7381-6fc1-4091-92cd-c077ae1ab8be""
            },
            ""Name"": ""devenv""
        },
        ""Translation"": null,
        ""NaturalId"": ""021d7381-6fc1-4091-92cd-c077ae1ab8be$All$Tabular$6158dd074d64498f8a50e5e9828fda23$101$EMPTY FROM DATE$EMPTY TO DATE$No translation""
    },
    ""Status"": {
        ""CreatedDate"": ""0001-01-01T00:00:00"",
        ""BeginDate"": null,
        ""EndDate"": null,
        ""LastUpdateDate"": ""0001-01-01T00:00:00"",
        ""Status"": 2,
        ""JobStatus"": 0,
        ""ProgressInPercents"": 42,
        ""IsRunning"": false,
        ""TimeEstimation"": null,
        ""Error"": null
    },
    ""ArchivePassword"": null,
    ""NaturalId"": ""NoExternal$021d7381-6fc1-4091-92cd-c077ae1ab8be$All$Tabular$6158dd074d64498f8a50e5e9828fda23$101$EMPTY FROM DATE$EMPTY TO DATE$No translation"",
    ""AccessToken"": ""access_token_long"",
    ""RefreshToken"": ""refresh_token_long"",
    ""StorageType"": null,
    ""ProcessId"": 0,
    ""ShouldDropTenantSchema"": false
}");
        }

        [Test]
        public void should_deserialize_exportSettings_Tenant_Id() => 
            Assert.That(args.ExportSettings.Tenant.Id.Id, Is.EqualTo("021d7381-6fc1-4091-92cd-c077ae1ab8be"));

        [Test]
        public void should_deserialize_exportSettings_QuestionnaireId() => 
            Assert.That(args.ExportSettings.QuestionnaireId.Id, Is.EqualTo("6158dd074d64498f8a50e5e9828fda23$101"));

        [Test]
        public void should_deserialize_exportSettings_ExportFormat() => 
            Assert.That(args.ExportSettings.ExportFormat, Is.EqualTo(DataExportFormat.Tabular));
        
        [Test]
        public void should_deserialize_AccessToken() => Assert.That(args.AccessToken, Is.EqualTo("access_token_long"));

        [Test]
        public void should_deserialize_RefreshToken() => Assert.That(args.RefreshToken, Is.EqualTo("refresh_token_long"));
        [Test]
        public void should_deserialize_Status_ProgressInPercents() => Assert.That(args.Status.ProgressInPercents, Is.EqualTo(42));
    }
}

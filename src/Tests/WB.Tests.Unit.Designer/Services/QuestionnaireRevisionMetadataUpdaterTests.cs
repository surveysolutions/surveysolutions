using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Revisions;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.Services
{
    public class QuestionnaireHistoryVersionsServiceTests
    {
        private QuestionnaireDocument questionnaire;
        private DesignerDbContext db;
        private Mock<ICommandService> commandService;
        private QuestionnaireHistoryVersionsService metadataUpdater;

        [SetUp]
        public void Setup()
        {
            this.questionnaire = new QuestionnaireDocument();

            this.db = Create.InMemoryDbContext();
            this.commandService = new Mock<ICommandService>();
            this.metadataUpdater = Create.QuestionnireHistoryVersionsService(dbContext: db, commandService: commandService.Object);
        }

        [TestCase("WB.Headquarters/18.0.1   (build 22323)    (DEBUG)", "18.0.1", "22323")]
        [TestCase("WB.Headquarters/1822.0 (build 2) (DEBUG)", "1822.0", "2")]
        [TestCase("WB.Headquarters/18222    (build 22323) (DEBUG)", "18222", "22323")]
        public async Task HqVersionVersionExtractorTest(string userAgent, string hqVersion, string hqBuild)
        {
            db.QuestionnaireChangeRecords.Add(Create.QuestionnaireChangeRecord(
                questionnaireId: this.questionnaire.PublicKey.FormatGuid(),
                action: QuestionnaireActionType.ImportToHq,
                sequence: 1));

            db.SaveChanges();

            // act
            await this.metadataUpdater.TrackQuestionnaireImportAsync(this.questionnaire, userAgent, Id.g1);

            this.commandService.Verify(c => c.Execute(It.Is<ImportQuestionnaireToHq>(cmd =>
                cmd.QuestionnaireId == this.questionnaire.PublicKey
                && cmd.Metadata.Hq.Build == hqBuild
                && cmd.Metadata.Hq.Version == hqVersion
            ), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task TrackQuestionnaireImportAsync_Should_return_revision_id()
        {
            var changeRecord = new QuestionnaireChangeRecord
            {
                QuestionnaireId = this.questionnaire.Id,
                Sequence = 2,
                QuestionnaireChangeRecordId = Id.g2.FormatGuid(),
                ActionType = QuestionnaireActionType.ImportToHq
            };

            this.db.QuestionnaireChangeRecords.Add(changeRecord);
            this.db.SaveChanges();

            // act
            var revision = await this.metadataUpdater.TrackQuestionnaireImportAsync(this.questionnaire, "", Id.gA);

            Assert.That(revision, Is.EqualTo(changeRecord.Sequence));
        }


        [Test]
        public async Task TrackQuestionnaireImportAsync_Should_return_last_revision_id_for_lot_of_changes()
        {
            for (int i = 5; i >= 1; i--)
            {
                var changeRecord = new QuestionnaireChangeRecord
                {
                    
                    QuestionnaireId = this.questionnaire.Id,
                    Sequence = i,
                    QuestionnaireChangeRecordId = System.Guid.NewGuid().FormatGuid(),
                    ActionType = QuestionnaireActionType.ImportToHq
                };

                this.db.QuestionnaireChangeRecords.Add(changeRecord);
            }

            this.db.SaveChanges();

            // act
            var revision = await this.metadataUpdater.TrackQuestionnaireImportAsync(this.questionnaire, "", Id.gA);

            Assert.That(revision, Is.EqualTo(5));
        }


        [Test]
        public async Task UpdateQuestionnaireMetadata_should_fill_metadata_values()
        {
            var changeRecord = new QuestionnaireChangeRecord
            {
                QuestionnaireId = this.questionnaire.Id,
                QuestionnaireChangeRecordId = Id.g2.FormatGuid(),
                Sequence = 2,
                ActionType = QuestionnaireActionType.ImportToHq
            };

            this.db.QuestionnaireChangeRecords.Add(changeRecord);
            this.db.SaveChanges();

            // act
            await this.metadataUpdater.UpdateQuestionnaireMetadataAsync(this.questionnaire.PublicKey, 2,
                new QuestionnaireRevisionMetaDataUpdate
                {
                    Comment = "Some comment",
                    HqHost = "fsb.ru",
                    HqTimeZone = 160,
                    HqImporterLogin = "Richard",
                    HqQuestionnaireVersion = 1
                });

            // assert
            var record = this.db.QuestionnaireChangeRecords
                .Single(r => r.QuestionnaireChangeRecordId == changeRecord.QuestionnaireChangeRecordId);

            Assert.That(record.Meta.Hq.HostName, Is.EqualTo("fsb.ru"));
            Assert.That(record.Meta.Comment, Is.EqualTo("Some comment"));
            Assert.That(record.Meta.Hq.ImporterLogin, Is.EqualTo("Richard"));
            Assert.That(record.Meta.Hq.QuestionnaireVersion, Is.EqualTo(1));
            Assert.That(record.Meta.Hq.TimeZoneMinutesOffset, Is.EqualTo(160));

            Assert.That(record.TargetItemTitle, Is.EqualTo("fsb.ru"), "Should also update target item title for history page");
        }
    }
}

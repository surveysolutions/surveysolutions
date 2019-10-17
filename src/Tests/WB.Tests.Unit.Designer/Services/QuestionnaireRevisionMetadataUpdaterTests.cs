using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.Services
{
    public class QuestionnaireRevisionMetadataUpdaterTests
    {
        private QuestionnaireDocument questionnaire;
        private DesignerDbContext db;
        private Mock<ICommandService> commandService;
        private QuestionnaireRevisionMetadataUpdater metadataUpdater;

        [SetUp]
        public void Setup()
        {
            this.questionnaire = new QuestionnaireDocument()
            {
            };

            this.db = Create.InMemoryDbContext();
            this.commandService = new Mock<ICommandService>();
            this.metadataUpdater = new QuestionnaireRevisionMetadataUpdater(this.commandService.Object, this.db);
        }

        [TestCase("WB.Headquarters/18.0.1   (build 22323)    (DEBUG)", "18.0.1", "22323")]
        [TestCase("WB.Headquarters/1822.0 (build 2) (DEBUG)", "1822.0", "2")]
        [TestCase("WB.Headquarters/18222    (build 22323) (DEBUG)", "18222", "22323")]
        public void HqVersionVersionExtractorTest(string userAgent, string hqVersion, string hqBuild)
        {
            // act
            this.metadataUpdater.LogInHistoryImportQuestionnaireToHq(this.questionnaire, userAgent, Id.g1);

            this.commandService.Verify(c => c.Execute(It.Is<ImportQuestionnaireToHq>(cmd =>
                cmd.QuestionnaireId == this.questionnaire.PublicKey
                && cmd.Metadata.HqBuild == hqBuild
                && cmd.Metadata.HqVersion == hqVersion
            ), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void LogInHistoryImportQuestionnaireToHq_Should_set_revision_id()
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
            this.metadataUpdater.LogInHistoryImportQuestionnaireToHq(this.questionnaire, "", Id.gA);

            Assert.That(questionnaire.Revision, Is.EqualTo(changeRecord.Sequence));
        }

        [Test]
        public void UpdateQuestionnaireMetadata_should_fill_metadata_values()
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
            this.metadataUpdater.UpdateQuestionnaireMetadata(this.questionnaire.PublicKey, 2,
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

            Assert.That(record.Meta.HqHostName, Is.EqualTo("fsb.ru"));
            Assert.That(record.Meta.Comment, Is.EqualTo("Some comment"));
            Assert.That(record.Meta.HqImporterLogin, Is.EqualTo("Richard"));
            Assert.That(record.Meta.QuestionnaireVersion, Is.EqualTo(1));
            Assert.That(record.Meta.HqTimeZoneMinutesOffset, Is.EqualTo(160));

            Assert.That(record.TargetItemTitle, Is.EqualTo("fsb.ru"), "Should also update target item title for history page");
        }
    }
}

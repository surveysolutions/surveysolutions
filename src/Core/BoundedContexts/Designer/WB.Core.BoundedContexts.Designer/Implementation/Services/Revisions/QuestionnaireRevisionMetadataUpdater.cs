using Main.Core.Documents;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class QuestionnaireRevisionMetadataUpdater : IQuestionnaireRevisionMetadataUpdater
    {
        private readonly ICommandService commandService;
        private readonly DesignerDbContext dbContext;

        public QuestionnaireRevisionMetadataUpdater(
            ICommandService commandService,
            DesignerDbContext dbContext)
        {
            this.commandService = commandService;
            this.dbContext = dbContext;
        }

        public void LogInHistoryImportQuestionnaireToHq(
            QuestionnaireDocument questionnaireDocument,
            string userAgent,
            Guid userId)
        {
            var meta = FromUserAgent(userAgent);

            var command = new ImportQuestionnaireToHq(userId, meta, questionnaireDocument);
            commandService.Execute(command);

            var revisionId = this.GetLastHistoryIdForAction(questionnaireDocument.PublicKey, QuestionnaireActionType.ImportToHq);

            if (revisionId != null)
            {
                questionnaireDocument.Revision = revisionId.Value;
            }
        }

        public void UpdateQuestionnaireMetadata(Guid questionnaire, int revision, QuestionnaireRevisionMetaDataUpdate metaData)
        {
            var record = this.dbContext.QuestionnaireChangeRecords
                .SingleOrDefault(r => r.QuestionnaireId == questionnaire.FormatGuid() 
                    && r.Sequence == revision);

            if (record.Meta == null)
            {
                record.Meta = new QuestionnaireChangeRecordMetadata();
            }

            record.Meta.HqHostName = metaData.HqHost ?? record.Meta.HqHostName;
            record.TargetItemTitle = record.Meta.HqHostName;
            record.Meta.Comment = metaData.Comment;
            record.Meta.HqTimeZoneMinutesOffset = metaData.HqTimeZone;
            record.Meta.HqImporterLogin = metaData.HqImporterLogin;
            record.Meta.QuestionnaireVersion = metaData.HqQuestionnaireVersion;

            this.dbContext.QuestionnaireChangeRecords.Update(record);
            this.dbContext.SaveChanges();
        }

        private int? GetLastHistoryIdForAction(Guid questionnaireId, QuestionnaireActionType actionType = QuestionnaireActionType.ImportToHq)
        {
            var sId = questionnaireId.FormatGuid();

            var record = this.dbContext.QuestionnaireChangeRecords
                .Where(q => q.QuestionnaireId == sId)
                .LastOrDefault(r => r.ActionType == actionType);

            if (record == null) return null;

            return record.Sequence;
        }

        private QuestionnaireChangeRecordMetadata FromUserAgent(string userAgent)
        {
            var versionInfo = GetHqVersionFromUserAgent(userAgent);

            var meta = new QuestionnaireChangeRecordMetadata
            {
                HqVersion = versionInfo.HasValue ? versionInfo.Value.version : null,
                HqBuild = versionInfo.HasValue ? versionInfo.Value.build : null,
            };

            return meta;
        }

        private static Regex hqVersion = new Regex(@"WB\.Headquarters/(?<version>[\d\.]+)\s+\(build\s+(?<build>\d+)",
            RegexOptions.Compiled);

        private (string version, string build)? GetHqVersionFromUserAgent(string userAgent)
        {
            var match = hqVersion.Match(userAgent);

            if (match.Success)
            {
                return (match.Groups["version"].Value, match.Groups["build"].Value);
            }

            return null;
        }
    }
}


using System;
using System.Linq;
using System.Text.RegularExpressions;
using Main.Core.Documents;
using Microsoft.AspNetCore.Http;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Implementation.Services
{
    public interface IQuestionnaireRevisionTagger
    {
        void LogInHistoryImportQuestionnaireToHq(QuestionnaireDocument questionnaireDocument, HttpRequest request, Guid userId);
        void UpdateQuestionnaireMetadata(Guid revision, QuestionnaireRevisionMetaDataUpdate metaData);
    }

    public class QuestionnaireRevisionTagger : IQuestionnaireRevisionTagger
    {
        private readonly ICommandService commandService;
        private readonly DesignerDbContext dbContext;

        public QuestionnaireRevisionTagger(
            ICommandService commandService,            
            DesignerDbContext dbContext)
        {
            this.commandService = commandService;
            this.dbContext = dbContext;
        }

        public void LogInHistoryImportQuestionnaireToHq(QuestionnaireDocument questionnaireDocument, HttpRequest request, Guid userId)
        {
            var meta = FromHttpRequest(request);

            var command = new ImportQuestionnaireToHq(userId, meta, questionnaireDocument);
            commandService.Execute(command);

            var revisionId = this.GetLastHistoryIdForAction(questionnaireDocument.PublicKey, QuestionnaireActionType.ImportToHq);

            if (revisionId != null)
            {
                questionnaireDocument.Revision = revisionId.Value;
            }
        }
        
        public void UpdateQuestionnaireMetadata(Guid revision, QuestionnaireRevisionMetaDataUpdate metaData)
        {
            var record = this.dbContext.QuestionnaireChangeRecords
                .SingleOrDefault(r => r.QuestionnaireChangeRecordId == revision.FormatGuid());

            if(record.Meta == null)
            {
                record.Meta = new QuestionnaireChangeRecordMetadata();
            }

            record.Meta.HqHostName = metaData.HqHost ?? record.Meta.HqHostName;
            record.TargetItemTitle = record.Meta.HqHostName;
            record.Meta.Comment = metaData.Comment;
            record.Meta.HqTimeZone = metaData.HqTimeZone;
            record.Meta.ImporterLogin = metaData.ImporterLogin;
            record.Meta.QuestionnaireVersion = metaData.QuestionnaireVersion;

            this.dbContext.QuestionnaireChangeRecords.Update(record);
            this.dbContext.SaveChanges();
        }

        private Guid? GetLastHistoryIdForAction(Guid questionnaireId, QuestionnaireActionType actionType = QuestionnaireActionType.ImportToHq)
        {
            var sId = questionnaireId.FormatGuid();

            var record = this.dbContext.QuestionnaireChangeRecords
                .Where(q => q.QuestionnaireId == sId)
                .LastOrDefault(r => r.ActionType == actionType);

            if (record == null) return null;

            return Guid.Parse(record.QuestionnaireChangeRecordId);
        }

        private QuestionnaireChangeRecordMetadata FromHttpRequest(HttpRequest Request)
        {
            var versionInfo = GetHqVersionFromUserAgent(Request);

            var meta = new QuestionnaireChangeRecordMetadata
            {
                HqVersion = versionInfo.HasValue ? versionInfo.Value.version : null,
                HqBuild = versionInfo.HasValue ? versionInfo.Value.build : null,
            };

            return meta;
        }                  
 

        static Regex hqVersion = new Regex(@"WB\.Headquarters/(?<version>[\d\.]+)\s+\(build\s+(?<build>\d+)", RegexOptions.Compiled);
        private (string version, string build)? GetHqVersionFromUserAgent(HttpRequest Request)
        {
            if (Request.Headers.TryGetValue("User-Agent", out var userAgents))
            {
                foreach (var ua in userAgents)
                {
                    var match = hqVersion.Match(ua);
                    if (match.Success)
                    {
                        return (match.Groups["version"].Value, match.Groups["build"].Value);
                    }
                }
            }

            return null;
        }
    }
}

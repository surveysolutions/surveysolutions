﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Main.Core.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class QuestionnaireHistoryVersionsService : IQuestionnaireHistoryVersionsService
    {
        private readonly DesignerDbContext dbContext;
        private readonly IEntitySerializer<QuestionnaireDocument> entitySerializer;
        private readonly IOptions<QuestionnaireHistorySettings> historySettings;
        private readonly IPatchApplier patchApplier;
        private readonly IPatchGenerator patchGenerator;

        public QuestionnaireHistoryVersionsService(DesignerDbContext dbContext,
            IEntitySerializer<QuestionnaireDocument> entitySerializer,
            IOptions<QuestionnaireHistorySettings> historySettings,
            IPatchApplier patchApplier,
            IPatchGenerator patchGenerator,
            ICommandService commandService)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.entitySerializer = entitySerializer;
            this.historySettings = historySettings;
            this.patchApplier = patchApplier;
            this.patchGenerator = patchGenerator;
            this.commandService = commandService;
        }

        public QuestionnaireDocument GetByHistoryVersion(Guid historyReferenceId)
        {
            var questionnaireChangeRecord = this.dbContext.QuestionnaireChangeRecords.Find(historyReferenceId.FormatGuid());
            if (questionnaireChangeRecord == null)
                return null;

            if (questionnaireChangeRecord.ResultingQuestionnaireDocument != null)
            {
                var resultingQuestionnaireDocument = questionnaireChangeRecord.ResultingQuestionnaireDocument;
                var questionnaireDocument = this.entitySerializer.Deserialize(resultingQuestionnaireDocument);
                return questionnaireDocument;
            }

            var history = (from h in this.dbContext.QuestionnaireChangeRecords
                           where h.Sequence >= questionnaireChangeRecord.Sequence
                              && h.QuestionnaireId == questionnaireChangeRecord.QuestionnaireId &&
                              (h.Patch != null || h.ResultingQuestionnaireDocument != null)
                        orderby h.Sequence descending 
                        select new
                        {
                            h.Sequence,
                            h.ResultingQuestionnaireDocument,
                            DiffWithPreviousVersion = h.Patch
                        }).ToList();

            var questionnaire = history.First().ResultingQuestionnaireDocument;
            foreach (var patch in history.Skip(1))
            {
                questionnaire = this.patchApplier.Apply(questionnaire, patch.DiffWithPreviousVersion);
            }

            return entitySerializer.Deserialize(questionnaire);
        }

        private void RemoveOldQuestionnaireHistory(string sQuestionnaireId, int maxHistoryDepth)
        {
            var oldChangeRecord = this.dbContext.QuestionnaireChangeRecords
                .Where(x => 
                    x.QuestionnaireId == sQuestionnaireId 
                    && x.ActionType != QuestionnaireActionType.ImportToHq)
                .OrderByDescending(x => x.Sequence)
                .Skip(maxHistoryDepth)
                .ToList();

            foreach (var questionnaireChangeRecord in oldChangeRecord)
            {
                questionnaireChangeRecord.Patch = null;
                questionnaireChangeRecord.ResultingQuestionnaireDocument = null;
            }
        }

        public void AddQuestionnaireChangeItem(
            Guid questionnaireId,
            Guid responsibleId,
            string userName,
            QuestionnaireActionType actionType,
            QuestionnaireItemType targetType,
            Guid targetId,
            string targetTitle,
            string targetNewTitle,
            int? affectedEntries,
            DateTime? targetDateTime,
            QuestionnaireDocument questionnaireDocument,
            QuestionnaireChangeReference reference = null,
            QuestionnaireChangeRecordMetadata meta = null)
        {
            var sQuestionnaireId = questionnaireId.FormatGuid();

            var maxSequenceByQuestionnaire = this.dbContext.QuestionnaireChangeRecords
                .Where(y => y.QuestionnaireId == sQuestionnaireId).Select(y => (int?) y.Sequence).Max();

            var previousChange = (from h in this.dbContext.QuestionnaireChangeRecords
                                 where h.QuestionnaireId == sQuestionnaireId && h.ResultingQuestionnaireDocument != null
                                orderby h.Sequence descending 
                                select h
                                    ).FirstOrDefault();

            if (previousChange != null && questionnaireDocument != null)
            {
                var previousVersion = previousChange.ResultingQuestionnaireDocument;
                var left = this.entitySerializer.Serialize(questionnaireDocument);
                var right = previousVersion;

                var patch = this.patchGenerator.Diff(left, right);
                previousChange.ResultingQuestionnaireDocument = null;
                previousChange.Patch = patch;
            }

            var questionnaireChangeItem = new QuestionnaireChangeRecord
            {
                QuestionnaireChangeRecordId = Guid.NewGuid().FormatGuid(),
                QuestionnaireId = questionnaireId.FormatGuid(),
                UserId = responsibleId,
                UserName = userName,
                Timestamp = DateTime.UtcNow,
                Sequence = maxSequenceByQuestionnaire + 1 ?? 0,
                ActionType = actionType,
                TargetItemId = targetId,
                TargetItemTitle = targetTitle,
                TargetItemType = targetType,
                TargetItemNewTitle = targetNewTitle,
                AffectedEntriesCount = affectedEntries,
                TargetItemDateTime = targetDateTime,
                Meta = meta
            };

            if (reference != null)
            {
                reference.QuestionnaireChangeRecord = questionnaireChangeItem;
                questionnaireChangeItem.References.Add(reference);
            }

            if (questionnaireDocument != null)
            {
                questionnaireChangeItem.ResultingQuestionnaireDocument = this.entitySerializer.Serialize(questionnaireDocument);
            }

            this.dbContext.QuestionnaireChangeRecords.Add(questionnaireChangeItem);
            
            // -1 is to take into account newly added change record that is not yet in DB
            this.RemoveOldQuestionnaireHistory(sQuestionnaireId, historySettings.Value.QuestionnaireChangeHistoryLimit - 1);
            this.dbContext.SaveChanges();
        }

        public string GetDiffWithLastStoredVersion(QuestionnaireDocument questionnaire)
        {
            var previousVersion = this.GetLastStoredQuestionnaireVersion(questionnaire);
            var left = this.entitySerializer.Serialize(previousVersion);
            var right = this.entitySerializer.Serialize(questionnaire);

            var patch = this.patchGenerator.Diff(left, right);
            return patch;
        }

        private QuestionnaireDocument GetLastStoredQuestionnaireVersion(QuestionnaireDocument questionnaireDocument)
        {
            if (questionnaireDocument == null)
                return null;

            var existingSnapshot =  
                (from h in this.dbContext.QuestionnaireChangeRecords
                 where h.QuestionnaireId == questionnaireDocument.Id
                orderby h.Sequence 
                select h.QuestionnaireChangeRecordId).FirstOrDefault();

            if (existingSnapshot == null)
                return null;

            var resultingQuestionnaireDocument = this.GetByHistoryVersion(Guid.Parse(existingSnapshot));

            return resultingQuestionnaireDocument;
        }

        public async Task UpdateQuestionnaireChangeRecordCommentAsync(string questionnaireChangeRecordId, string comment)
        {
            var item = await this.dbContext.QuestionnaireChangeRecords.FindAsync(questionnaireChangeRecordId);
            if (item == null) return;

            if (item.Meta == null)
                item.Meta = new QuestionnaireChangeRecordMetadata();

            item.Meta.Comment = comment;

            this.dbContext.Update(item);
            await this.dbContext.SaveChangesAsync();
        }


        public async Task<int> TrackQuestionnaireImportAsync(
            QuestionnaireDocument questionnaireDocument,
            string userAgent,
            Guid userId)
        {
            var meta = FromUserAgent(userAgent);

            var command = new ImportQuestionnaireToHq(userId, meta, questionnaireDocument);
            commandService.Execute(command);

            return await this.GetLastHistoryIdForActionAsync(questionnaireDocument.PublicKey, QuestionnaireActionType.ImportToHq);
        }

        public async Task UpdateQuestionnaireMetadataAsync(Guid questionnaire, int revision, QuestionnaireRevisionMetaDataUpdate metaData)
        {
            var record = await this.dbContext.QuestionnaireChangeRecords
                .SingleOrDefaultAsync(r => r.QuestionnaireId == questionnaire.FormatGuid()
                    && r.Sequence == revision);

            if (record.Meta == null)
            {
                record.Meta = new QuestionnaireChangeRecordMetadata();
            }

            record.Meta.Hq.HostName = metaData.HqHost ?? record.Meta.Hq.HostName;
            record.Meta.Hq.Comment = metaData.HqComment;
            record.Meta.Hq.TimeZoneMinutesOffset = metaData.HqTimeZone;
            record.Meta.Hq.ImporterLogin = metaData.HqImporterLogin;
            record.Meta.Hq.QuestionnaireVersion = metaData.HqQuestionnaireVersion;

            record.TargetItemTitle = record.Meta.Hq.HostName;

            this.dbContext.QuestionnaireChangeRecords.Update(record);
            await this.dbContext.SaveChangesAsync();
        }

        private async Task<int> GetLastHistoryIdForActionAsync(Guid questionnaireId, QuestionnaireActionType actionType = QuestionnaireActionType.ImportToHq)
        {
            var sId = questionnaireId.FormatGuid();

            var record = await this.dbContext.QuestionnaireChangeRecords
                .Where(q => q.QuestionnaireId == sId)
                .LastAsync(r => r.ActionType == actionType);

            return record.Sequence;
        }

        private QuestionnaireChangeRecordMetadata FromUserAgent(string userAgent)
        {
            var versionInfo = GetHqVersionFromUserAgent(userAgent);

            return new QuestionnaireChangeRecordMetadata
            {
                Hq = new HeadquarterMetadata
                {
                    Version = versionInfo.HasValue ? versionInfo.Value.version : null,
                    Build = versionInfo.HasValue ? versionInfo.Value.build : null,
                }
            };
        }

        private static Regex hqVersion = new Regex(@"WB\.Headquarters/(?<version>[\d\.]+)\s+\(build\s+(?<build>\d+)",
            RegexOptions.Compiled);
        private readonly ICommandService commandService;

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

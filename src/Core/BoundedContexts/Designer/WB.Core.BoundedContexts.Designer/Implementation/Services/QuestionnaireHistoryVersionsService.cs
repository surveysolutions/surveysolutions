using System;
using System.Linq;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class QuestionnaireHistoryVersionsService : IQuestionnaireHistoryVersionsService
    {
        private readonly IPlainStorageAccessor<QuestionnaireChangeRecord> questionnaireChangeItemStorage;
        private readonly IEntitySerializer<QuestionnaireDocument> entitySerializer;
        private readonly QuestionnaireHistorySettings historySettings;
        private readonly IPatchApplier patchApplier;
        private readonly IPatchGenerator patchGenerator;

        public QuestionnaireHistoryVersionsService(IPlainStorageAccessor<QuestionnaireChangeRecord> questionnaireChangeItemStorage,
            IEntitySerializer<QuestionnaireDocument> entitySerializer,
            QuestionnaireHistorySettings historySettings,
            IPatchApplier patchApplier,
            IPatchGenerator patchGenerator)
        {
            this.questionnaireChangeItemStorage = questionnaireChangeItemStorage;
            this.entitySerializer = entitySerializer;
            this.historySettings = historySettings;
            this.patchApplier = patchApplier;
            this.patchGenerator = patchGenerator;
        }

        public QuestionnaireDocument GetByHistoryVersion(Guid historyReferenceId)
        {
            var questionnaireChangeRecord = this.questionnaireChangeItemStorage.GetById(historyReferenceId.FormatGuid());
            if (questionnaireChangeRecord == null)
                return null;

            if (questionnaireChangeRecord.ResultingQuestionnaireDocument != null)
            {
                var resultingQuestionnaireDocument = questionnaireChangeRecord.ResultingQuestionnaireDocument;
                var questionnaireDocument = this.entitySerializer.Deserialize(resultingQuestionnaireDocument);
                return questionnaireDocument;
            }

            var history = this.questionnaireChangeItemStorage.Query(_ => (from h in _
                where h.Sequence >= questionnaireChangeRecord.Sequence
                      && h.QuestionnaireId == questionnaireChangeRecord.QuestionnaireId &&
                      (h.Patch != null || h.ResultingQuestionnaireDocument != null)
                orderby h.Sequence descending 
                select new
                {
                    h.Sequence,
                    h.ResultingQuestionnaireDocument,
                    DiffWithPreviousVersion = h.Patch
                }).ToList());

            var questionnaire = history.First().ResultingQuestionnaireDocument;
            foreach (var patch in history.Skip(1))
            {
                questionnaire = this.patchApplier.Apply(questionnaire, patch.DiffWithPreviousVersion);
            }

            return entitySerializer.Deserialize(questionnaire);
        }

        public void RemoveOldQuestionnaireHistory(string sQuestionnaireId, int? maxSequenceByQuestionnaire, int maxHistoryDepth)
        {
            var minSequence = (maxSequenceByQuestionnaire ?? 0) -
                              maxHistoryDepth + 2;
            if (minSequence < 0) return;

            var oldChangeRecord = this.questionnaireChangeItemStorage.Query(_ => _
                .Where(x => x.QuestionnaireId == sQuestionnaireId && x.Sequence < minSequence
                                                                  && (x.ResultingQuestionnaireDocument != null || x.Patch != null))
                .OrderBy(x => x.Sequence)
                .ToList());

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
            QuestionnaireChangeReference reference = null)
        {
            var sQuestionnaireId = questionnaireId.FormatGuid();

            var maxSequenceByQuestionnaire = this.questionnaireChangeItemStorage.Query(x => x
                .Where(y => y.QuestionnaireId == sQuestionnaireId).Select(y => (int?) y.Sequence).Max());

            var previousChange = this.questionnaireChangeItemStorage.Query(_ => (from h in _
                where h.Sequence == maxSequenceByQuestionnaire 
                      && h.QuestionnaireId == sQuestionnaireId
                select h).FirstOrDefault());

            if (previousChange?.ResultingQuestionnaireDocument != null && questionnaireDocument != null)
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

            this.questionnaireChangeItemStorage.Store(questionnaireChangeItem, questionnaireChangeItem.QuestionnaireChangeRecordId);

            this.RemoveOldQuestionnaireHistory(sQuestionnaireId, 
                maxSequenceByQuestionnaire, 
                historySettings.QuestionnaireChangeHistoryLimit);
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

            var existingSnapshot = this.questionnaireChangeItemStorage.Query(_ => 
                (from h in _
                where h.QuestionnaireId == questionnaireDocument.Id
                orderby h.Sequence 
                select h.QuestionnaireChangeRecordId).FirstOrDefault());

            if (existingSnapshot == null)
                return null;

            var resultingQuestionnaireDocument = this.GetByHistoryVersion(Guid.Parse(existingSnapshot));

            return resultingQuestionnaireDocument;
        }
    }
}

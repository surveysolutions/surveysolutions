using System;
using System.Collections.Generic;
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
        private readonly IPatchApplier patchApplier;
        private readonly IPatchGenerator patchGenerator;

        public QuestionnaireHistoryVersionsService(IPlainStorageAccessor<QuestionnaireChangeRecord> questionnaireChangeItemStorage,
            IEntitySerializer<QuestionnaireDocument> entitySerializer,
            IPatchApplier patchApplier,
            IPatchGenerator patchGenerator)
        {
            this.questionnaireChangeItemStorage = questionnaireChangeItemStorage;
            this.entitySerializer = entitySerializer;
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

            var existingSnapshot = this.questionnaireChangeItemStorage.Query(_ => (from h in _
                where h.Sequence == 
                       (from hh in _ 
                        where hh.ResultingQuestionnaireDocument != null 
                              && hh.Sequence < questionnaireChangeRecord.Sequence
                              && hh.QuestionnaireId == questionnaireChangeRecord.QuestionnaireId
                        select (int?)hh.Sequence).Max()
                      && h.QuestionnaireId == questionnaireChangeRecord.QuestionnaireId
                select new
                {
                    h.Sequence,
                    h.ResultingQuestionnaireDocument
                }).FirstOrDefault());

            if (existingSnapshot != null)
            {
                var patches = this.questionnaireChangeItemStorage.Query(_ => _
                    .Where(x => x.Sequence <= questionnaireChangeRecord.Sequence
                                && x.Sequence > existingSnapshot.Sequence
                                && x.QuestionnaireId == questionnaireChangeRecord.QuestionnaireId)
                    .OrderBy(x => x.Sequence)
                    .Select(x =>
                        new
                        {
                            DiffWithPrevisousVersion = x.DiffWithPreviousVersion
                        }));
                string result = existingSnapshot.ResultingQuestionnaireDocument;
                foreach (var patch in patches)
                {
                    result = this.patchApplier.Apply(result, patch.DiffWithPrevisousVersion);
                }

                return this.entitySerializer.Deserialize(result);
            }
            else
            {
                var entireHistory = this.questionnaireChangeItemStorage.Query(_ => (
                    from h in _
                    where h.QuestionnaireId == questionnaireChangeRecord.QuestionnaireId 
                          && h.Sequence <= questionnaireChangeRecord.Sequence
                          && h.DiffWithPreviousVersion != null
                    orderby h.Sequence
                    select new
                    {
                        DiffWithPrevisousVersion = h.DiffWithPreviousVersion
                    }).ToList());

                string fullHistoryResult = null;
                foreach (var historyPatch in entireHistory)
                {
                    fullHistoryResult =
                        this.patchApplier.Apply(fullHistoryResult, historyPatch.DiffWithPrevisousVersion);
                }

                return this.entitySerializer.Deserialize(fullHistoryResult);
            }
        }

        public void RemoveOldQuestionnaireHistory(string sQuestionnaireId, int? maxSequenceByQuestionnaire, int maxHistoryDepth)
        {
            var minSequence = (maxSequenceByQuestionnaire ?? 0) -
                              maxHistoryDepth + 2;
            if (minSequence < 0) return;

            List<string> changeRecordIdsToRemove = this.questionnaireChangeItemStorage.Query(_ => _
                .Where(x => x.QuestionnaireId == sQuestionnaireId && x.Sequence < minSequence
                                                                  && (x.ResultingQuestionnaireDocument != null || x.DiffWithPreviousVersion != null))
                .OrderBy(x => x.Sequence)
                .Select(x => x.QuestionnaireChangeRecordId)
                .ToList());

            if (changeRecordIdsToRemove.Count > 0)
            {
                string lastItem = changeRecordIdsToRemove.Last();
                var change = this.questionnaireChangeItemStorage.GetById(lastItem);

                QuestionnaireDocument resultingQuestionnaireForLastStoredRecord =
                    this.GetByHistoryVersion(Guid.Parse(change.QuestionnaireChangeRecordId));

                change.ResultingQuestionnaireDocument = this.entitySerializer.Serialize(resultingQuestionnaireForLastStoredRecord);
                change.DiffWithPreviousVersion = null;
                this.questionnaireChangeItemStorage.Store(change, change.QuestionnaireChangeRecordId);

                foreach (var changeRecordIdToRemove in changeRecordIdsToRemove.Where(x => x != lastItem))
                {
                    var itemToRemoveQuestionnaire = this.questionnaireChangeItemStorage.GetById(changeRecordIdToRemove);
                    itemToRemoveQuestionnaire.ResultingQuestionnaireDocument = null;
                    itemToRemoveQuestionnaire.DiffWithPreviousVersion = null;
                    this.questionnaireChangeItemStorage.Store(itemToRemoveQuestionnaire, changeRecordIdToRemove);
                }
            }
        }

        public string GetDiffWithPreviousStoredVersion(QuestionnaireDocument questionnaire)
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

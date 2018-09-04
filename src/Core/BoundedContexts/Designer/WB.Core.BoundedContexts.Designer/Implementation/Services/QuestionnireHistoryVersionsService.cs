using System;
using System.Linq;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class QuestionnireHistoryVersionsService : IQuestionnireHistoryVersionsService
    {
        private readonly IPlainStorageAccessor<QuestionnaireChangeRecord> questionnaireChangeItemStorage;
        private readonly IEntitySerializer<QuestionnaireDocument> entitySerializer;
        private readonly IPatchApplier patchApplier;

        public QuestionnireHistoryVersionsService(IPlainStorageAccessor<QuestionnaireChangeRecord> questionnaireChangeItemStorage,
            IEntitySerializer<QuestionnaireDocument> entitySerializer,
            IPatchApplier patchApplier)
        {
            this.questionnaireChangeItemStorage = questionnaireChangeItemStorage;
            this.entitySerializer = entitySerializer;
            this.patchApplier = patchApplier;
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
                    where h.QuestionnaireId == questionnaireChangeRecord.QuestionnaireId &&
                          h.Sequence <= questionnaireChangeRecord.Sequence
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

        public string GetResultingQuestionnaireDocument(QuestionnaireDocument questionnaireDocument)
        {
            if (questionnaireDocument == null)
                return null;

            return entitySerializer.Serialize(questionnaireDocument);
        }
    }
}

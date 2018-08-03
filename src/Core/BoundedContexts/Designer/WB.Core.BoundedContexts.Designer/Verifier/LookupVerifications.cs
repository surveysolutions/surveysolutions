using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Verifier
{
    public class LookupVerifications : AbstractVerifier, IPartialVerifier
    {
        private readonly ILookupTableService lookupTableService;
        private readonly IKeywordsProvider keywordsProvider;

        public LookupVerifications(ILookupTableService lookupTableService, IKeywordsProvider keywordsProvider)
        {
            this.lookupTableService = lookupTableService;
            this.keywordsProvider = keywordsProvider;
        }

        private IEnumerable<Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>> ErrorsVerifiers => new[]
        {
            CriticalForLookupTable(LookupTableNameIsKeyword, "WB0052", VerificationMessages.WB0052_LookupNameIsKeyword),
            CriticalForLookupTable(LookupTableHasInvalidName, "WB0024", VerificationMessages.WB0024_LookupHasInvalidName),
            CriticalForLookupTable(LookupTableHasEmptyName, "WB0025", VerificationMessages.WB0025_LookupHasEmptyName),
            CriticalForLookupTable(LookupTableHasEmptyContent, "WB0048", VerificationMessages.WB0048_LookupHasEmptyContent),
            CriticalForLookupTable(LookupTableHasInvalidHeaders, "WB0031", VerificationMessages.WB0031_LookupTableHasInvalidHeaders),
            CriticalForLookupTable(LookupTableMoreThan10Columns, "WB0043", VerificationMessages.WB0043_LookupTableMoreThan11Columns),
            CriticalForLookupTable(LookupTableMoreThan5000Rows, "WB0044", VerificationMessages.WB0044_LookupTableMoreThan5000Rows),
            CriticalForLookupTable(LookupTableNotUniqueRowcodeValues, "WB0047", VerificationMessages.WB0047_LookupTableNotUniqueRowcodeValues),
        };

        

        private static bool LookupTableHasEmptyName(Guid tableId, LookupTable table, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return string.IsNullOrWhiteSpace(table.TableName);
        }

        private bool LookupTableHasInvalidName(Guid tableId, LookupTable table, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return !IsVariableNameValid(table.TableName);
        }

        private bool LookupTableNameIsKeyword(Guid tableId, LookupTable table, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (string.IsNullOrWhiteSpace(table.FileName) || string.IsNullOrEmpty(table.TableName))
                return false;
            return keywordsProvider.IsReservedKeyword(table.TableName);
        }

        private bool LookupTableHasEmptyContent(Guid tableId, LookupTable table, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var lookupTableContent = this.lookupTableService.GetLookupTableContent(questionnaire.PublicKey, tableId);
            return lookupTableContent == null;
        }

        private bool LookupTableHasInvalidHeaders(LookupTable table, LookupTableContent tableContent, MultiLanguageQuestionnaireDocument questionnaire)
        {
            foreach (var tableContentVariableName in tableContent.VariableNames)
            {
                if (!IsVariableNameValid(tableContentVariableName))
                    return true;

                if (!string.IsNullOrWhiteSpace(tableContentVariableName) && this.keywordsProvider.IsReservedKeyword(tableContentVariableName))
                    return true;
            }

            return false;
        }

        private static bool LookupTableMoreThan10Columns(LookupTable table, LookupTableContent tableContent, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return tableContent.VariableNames.Length > 10;
        }

        private static bool LookupTableMoreThan5000Rows(LookupTable table, LookupTableContent tableContent, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return tableContent.Rows.Length > 5000;
        }

        private static bool LookupTableNotUniqueRowcodeValues(LookupTable table, LookupTableContent tableContent, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return tableContent.Rows.Select(x => x.RowCode).Distinct().Count() != tableContent.Rows.Count();
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> CriticalForLookupTable(
            Func<Guid, LookupTable, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return (questionnaire) => questionnaire
                .LookupTables
                .Where(entity => hasError(entity.Key, entity.Value, questionnaire))
                .Select(entity => QuestionnaireVerificationMessage.Critical(code, message, QuestionnaireEntityReference.CreateForLookupTable(entity.Key)));
        }

        private Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> CriticalForLookupTable(
            Func<LookupTable, LookupTableContent, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return (questionnaire) =>
                from lookupTable in questionnaire.LookupTables
                let lookupTableContent = this.lookupTableService.GetLookupTableContent(questionnaire.PublicKey, lookupTable.Key)
                where lookupTableContent != null
                where hasError(lookupTable.Value, lookupTableContent, questionnaire)
                select QuestionnaireVerificationMessage.Critical(code, message, QuestionnaireEntityReference.CreateForLookupTable(lookupTable.Key));
        }

        public IEnumerable<QuestionnaireVerificationMessage> Verify(MultiLanguageQuestionnaireDocument multiLanguageQuestionnaireDocument)
        {
            var verificationMessagesByQuestionnaire = new List<QuestionnaireVerificationMessage>();
            foreach (var verifier in ErrorsVerifiers)
            {
                verificationMessagesByQuestionnaire.AddRange(verifier.Invoke(multiLanguageQuestionnaireDocument));
            }
            return verificationMessagesByQuestionnaire;
        }
    }
}

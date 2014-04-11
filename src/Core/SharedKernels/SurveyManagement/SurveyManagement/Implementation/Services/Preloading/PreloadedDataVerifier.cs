using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Raven.Abstractions.Extensions;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Properties;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading
{
    internal class PreloadedDataVerifier : IPreloadedDataVerifier
    {
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage;
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireExportStructure> questionnaireExportStructureStorage;
        private readonly string[] columnsToExcluteFromQuestionMapping = new[] { "Id", "ParentId" };
        private readonly IDataFileExportService dataFileExportService;
        public PreloadedDataVerifier(
            IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage,
            IVersionedReadSideRepositoryReader<QuestionnaireExportStructure> questionnaireExportStructureStorage, IDataFileExportService dataFileExportService)
        {
            this.questionnaireDocumentVersionedStorage = questionnaireDocumentVersionedStorage;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.dataFileExportService = dataFileExportService;
        }

        public IEnumerable<PreloadedDataVerificationError> Verify(Guid questionnaireId, long version, PreloadedDataByFile[] data)
        {
            var questionnaire = questionnaireDocumentVersionedStorage.GetById(questionnaireId, version);
            var questionnaireExportStructure = questionnaireExportStructureStorage.GetById(questionnaireId, version);
            if (questionnaire == null || questionnaireExportStructure == null)
            {
                yield return new PreloadedDataVerificationError("PL0001", PreloadingVerificationMessages.PL0001_NoQuestionnaire);
                yield break;
            }

            if (data.Length > 0)
            {
                yield return new PreloadedDataVerificationError("PL0002", PreloadingVerificationMessages.PL0002_MoreThenOneLevel);
            }

            questionnaire.Questionnaire.ConnectChildrenWithParent();

            var errorsMessagess=
                from verifier in this.AtomicVerifiers
                let errors = verifier.Invoke(data, questionnaire.Questionnaire, questionnaireExportStructure)
                from error in errors
                select error;

            foreach (var preloadedDataVerificationError in errorsMessagess)
            {
                yield return preloadedDataVerificationError;
            }
        }

        private IEnumerable<Func<PreloadedDataByFile[],QuestionnaireDocument,QuestionnaireExportStructure, IEnumerable<PreloadedDataVerificationError>>> AtomicVerifiers
        {
            get
            {
                return new[]
                {
                    Verify(CoulmnWasntMappedOnQuestionInTemplate, "PL0003", PreloadingVerificationMessages.PL0003_ColumnWasntMappedOnQuestion, PreloadedDataVerificationReferenceType.Column),
                    Verifier(FileWasntMappedOnQuestionnaireLevel, "PL0004", PreloadingVerificationMessages.PL0004_FileWasntMappedRoster, PreloadedDataVerificationReferenceType.File)
                };
            }
        }

        private IEnumerable<string> CoulmnWasntMappedOnQuestionInTemplate(PreloadedDataByFile levelData, QuestionnaireDocument questionnaire, QuestionnaireExportStructure exportStructure)
        {
            var levelExportStructure = this.FindLevel(levelData, exportStructure);
            if(levelExportStructure==null)
                yield break;

            foreach (var columnName in levelData.Header)
            {
                if(columnsToExcluteFromQuestionMapping.Contains(columnName))
                    continue;
                if (!levelExportStructure.HeaderItems.Values.Any(headerItem => headerItem.ColumnNames.Contains(columnName)))
                    yield return columnName;
            }
        }

        private bool FileWasntMappedOnQuestionnaireLevel(PreloadedDataByFile levelData, QuestionnaireDocument questionnaire,
            QuestionnaireExportStructure exportStructure)
        {
            var levelExportStructure = this.FindLevel(levelData, exportStructure);
            if (levelExportStructure == null)
                return true;
            return false;
        }

        private HeaderStructureForLevel FindLevel(PreloadedDataByFile levelData, QuestionnaireExportStructure exportStructure)
        {
            return exportStructure.HeaderToLevelMap.Values.FirstOrDefault(header=>this.dataFileExportService.GetInterviewExportedDataFileName(header.LevelName)==levelData.FileName);
        }

        private static Func<PreloadedDataByFile[], QuestionnaireDocument, QuestionnaireExportStructure, IEnumerable<PreloadedDataVerificationError>> Verify(
            Func<PreloadedDataByFile, QuestionnaireDocument, QuestionnaireExportStructure, IEnumerable<string>> getErrors, string code, string message, PreloadedDataVerificationReferenceType type)
        {
            return (data, questionnaire, exportStructure) =>
                data.SelectMany(level => getErrors(level, questionnaire, exportStructure).Select(entity => new PreloadedDataVerificationError(code, message, new PreloadedDataVerificationReference(type, entity,level.FileName))));
        }

        private static Func<PreloadedDataByFile[], QuestionnaireDocument, QuestionnaireExportStructure, IEnumerable<PreloadedDataVerificationError>> Verifier(
            Func<PreloadedDataByFile, QuestionnaireDocument, QuestionnaireExportStructure, bool> hesErrors, string code, string message, PreloadedDataVerificationReferenceType type)
        {
            return (data, questionnaire, exportStructure) =>
                data.Where(level => hesErrors(level, questionnaire, exportStructure)).Select(
                    level =>
                        new PreloadedDataVerificationError(code, message,
                            new PreloadedDataVerificationReference(type, null, level.FileName)));
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser
{
    internal class ImportDataParsingService : IPreloadedDataService
    {
        private readonly QuestionnaireExportStructure exportStructure;
        private readonly Dictionary<ValueVector<Guid>, RosterScopeDescription> rosterScopes;
        private readonly QuestionnaireDocument questionnaireDocument;
        private readonly IQuestionDataParser dataParser;
        private readonly IUserViewFactory userViewFactory;
        private IPlainTransactionManager plainTransactionManager => ServiceLocator.Current.GetInstance<IPlainTransactionManagerProvider>().GetPlainTransactionManager();

        private Dictionary<string, IQuestion> questionsCache = null;
        private Dictionary<string, IQuestion> QuestionsCache {
            get {
                return this.questionsCache ??
                       (this.questionsCache =
                           this.questionnaireDocument.GetEntitiesByType<IQuestion>()
                               .ToDictionary(x => x.StataExportCaption.ToLower(), x => x));
            }
        }

        private Dictionary<Guid, IGroup> groupsCache = null;
        private Dictionary<Guid, IGroup> GroupsCache
        {
            get
            {
                return this.groupsCache ??
                       (this.groupsCache =
                           this.questionnaireDocument.GetEntitiesByType<IGroup>()
                               .ToDictionary(x => x.PublicKey, x => x));
            }
        }

        private Dictionary<string, IVariable> variablesCache = null;
        private Dictionary<string, IVariable> VariablesCache
        {
            get
            {
                return this.variablesCache ??
                       (this.variablesCache =
                           this.questionnaireDocument.GetEntitiesByType<IVariable>()
                               .ToDictionary(x => x.Name, x => x));
            }
        }

        public ImportDataParsingService(QuestionnaireExportStructure exportStructure,
            Dictionary<ValueVector<Guid>, RosterScopeDescription> rosterScopes,
            QuestionnaireDocument questionnaireDocument, 
            IQuestionDataParser dataParser,
            IUserViewFactory userViewFactory)
        {
            this.exportStructure = exportStructure;
            this.rosterScopes = rosterScopes;
            this.questionnaireDocument = questionnaireDocument;
            this.dataParser = dataParser;
            this.userViewFactory = userViewFactory;
        }
        
        public HeaderStructureForLevel FindLevelInPreloadedData(string levelFileName)
        {
            var levelNameWithoutExtension = Path.GetFileNameWithoutExtension(levelFileName);
            return
                this.exportStructure.HeaderToLevelMap.Values.FirstOrDefault(
                    header => string.Equals(levelNameWithoutExtension, header.LevelName, StringComparison.OrdinalIgnoreCase));
        }

        public PreloadedDataByFile GetParentDataFile(string levelFileName, PreloadedDataByFile[] allLevels)
        {
            var levelExportStructure = this.FindLevelInPreloadedData(levelFileName);
            if (levelExportStructure == null)
                return null;

            if (!this.rosterScopes.ContainsKey(levelExportStructure.LevelScopeVector))
                return null;

            var rosterScopeDescription = this.rosterScopes[levelExportStructure.LevelScopeVector];

            var parentLevel =
                this.exportStructure.HeaderToLevelMap.Values.FirstOrDefault(
                    l =>
                        l.LevelScopeVector.Length == rosterScopeDescription.ScopeVector.Length - 1 &&
                          rosterScopeDescription.ScopeVector.Take(l.LevelScopeVector.Length).SequenceEqual(l.LevelScopeVector));
            if (parentLevel == null)
                return null;

            return this.GetDataFileByLevelName(allLevels, parentLevel.LevelName);
        }

        public int[] GetAvailableIdListForParent(PreloadedDataByFile parentDataFile, ValueVector<Guid> levelScopeVector, string[] parentIdValues, PreloadedDataByFile[] allLevels)
        {
            if (parentIdValues == null || parentIdValues.Length == 0)
                return null;

            var idIndexInParentDataFile = this.GetIdColumnIndex(parentDataFile);
            if (idIndexInParentDataFile < 0)
                return null;
            var parentIdColumnIndexesOfParentDataFile = this.GetParentIdColumnIndexes(parentDataFile)?? new int[0];
            var row = parentDataFile.Content.FirstOrDefault(record =>
                            record[idIndexInParentDataFile] == parentIdValues.First() &&
                            parentIdColumnIndexesOfParentDataFile.Select(x => record[x]).SequenceEqual(parentIdValues.Skip(1)));
            if (row == null)
                return null;

            if (!this.rosterScopes.ContainsKey(levelScopeVector))
                return null;

            RosterScopeDescription rosterScopeDescription = this.rosterScopes[levelScopeVector];
            if (rosterScopeDescription.Type == RosterScopeType.Fixed)
            {
                return this.GroupsCache.ContainsKey(levelScopeVector.Last()) 
                    ? this.GroupsCache[levelScopeVector.Last()].FixedRosterTitles.Select(x => Convert.ToInt32(x.Value)).ToArray() 
                    : null;
            }

            HeaderStructureForLevel levelExportStructure = this.FindLevelInPreloadedData(parentDataFile.FileName);
            if (levelExportStructure == null)
                return null;

            var rosterSizeQuestion = this.questionnaireDocument.FirstOrDefault<IQuestion>(q => q.PublicKey == levelScopeVector.Last());
            while (!levelExportStructure.ContainsQuestion(rosterSizeQuestion.PublicKey))
            {
                parentDataFile = this.GetParentDataFile(parentDataFile.FileName, allLevels);
                levelExportStructure = this.FindLevelInPreloadedData(parentDataFile.FileName);

                row = parentDataFile.Content.FirstOrDefault(record => record[idIndexInParentDataFile] == parentIdValues.First()) ??
                      parentDataFile.Content.FirstOrDefault(record => record[idIndexInParentDataFile] == parentIdValues.Last());
            }

            var rosterSizeAnswer = this.BuildAnswerByVariableName(levelExportStructure, rosterSizeQuestion.StataExportCaption,
                parentDataFile.Header, row);

            if (rosterSizeAnswer == null)
                return new int[0];

            if (rosterScopeDescription.Type == RosterScopeType.Numeric)
            {
                return Enumerable.Range(0, ((NumericIntegerAnswer) rosterSizeAnswer).Value).ToArray();
            }

            if (rosterScopeDescription.Type == RosterScopeType.MultyOption)
            {
                var multiOptionAnswer = rosterSizeAnswer as CategoricalFixedMultiOptionAnswer;
                if (multiOptionAnswer != null)
                    return multiOptionAnswer.CheckedValues.ToArray();

                var yesNoAnswer = rosterSizeAnswer as YesNoAnswer;
                if(yesNoAnswer != null)
                    return yesNoAnswer.CheckedOptions.Where(v=>v.Yes).Select(v => v.Value).ToArray();
            }

            if (rosterScopeDescription.Type == RosterScopeType.TextList)
            {
                return ((TextListAnswer) rosterSizeAnswer).Rows
                    .Select(a => a.Value).ToArray();
            }
            return null;
        }

        public ValueParsingResult ParseQuestionInLevel(string answer, string columnName, HeaderStructureForLevel level,
            out object parsedValue)
        {
            parsedValue = null;
            var exportedQuestion =
                level.HeaderItems.Values.FirstOrDefault(
                    h => h.ColumnNames.Any(x => x.Equals(columnName.Trim(), StringComparison.InvariantCulture)));
            if (exportedQuestion == null)
                return ValueParsingResult.OK;

            AbstractAnswer parsedSingleColumnAnswer;
            return this.dataParser.TryParse(answer, columnName, this.GetQuestionByVariableName(exportedQuestion.VariableName),
                out parsedValue, out parsedSingleColumnAnswer);
        }

        public int GetIdColumnIndex(PreloadedDataByFile dataFile)
        {
            return this.GetColumnIndexByHeaderName(dataFile, ServiceColumns.Id);
        }

        public int GetColumnIndexByHeaderName(PreloadedDataByFile dataFile, string columnName)
        {
            if (dataFile == null)
                return -1;

            return dataFile.Header.ToList().FindIndex(header => string.Equals(header, columnName, StringComparison.InvariantCultureIgnoreCase));
        }

        public int[] GetParentIdColumnIndexes(PreloadedDataByFile dataFile)
        {
            var levelExportStructure = this.FindLevelInPreloadedData(dataFile.FileName);
            if (levelExportStructure == null || levelExportStructure.LevelScopeVector == null ||
                levelExportStructure.LevelScopeVector.Length == 0)
                return null;

            var columnIndexOfParentIdindexMap = new Dictionary<int,int>();
            var listOfAvailableParentIdIndexes = levelExportStructure.LevelScopeVector.Select((l, i) => i + 1).ToArray();

            for (int i = 0; i < dataFile.Header.Length; i++)
            {
                var columnName = dataFile.Header[i];
                if (!columnName.StartsWith(ServiceColumns.ParentId, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                var parentNumberString = columnName.Substring(ServiceColumns.ParentId.Length);
                int parentNumber;
                if (int.TryParse(parentNumberString, out parentNumber))
                {
                    if (listOfAvailableParentIdIndexes.Contains(parentNumber))
                        columnIndexOfParentIdindexMap.Add(i, parentNumber);
                }
            }
            if (columnIndexOfParentIdindexMap.Values.Distinct().Count() != levelExportStructure.LevelScopeVector.Length)
                return null;
            return columnIndexOfParentIdindexMap.OrderBy(x => x.Value).Select(x => x.Key).ToArray();
        }

        public PreloadedDataByFile GetTopLevelData(PreloadedDataByFile[] allLevels)
        {
            var topLevelExportData =
                this.exportStructure.HeaderToLevelMap.Values.FirstOrDefault(l => l.LevelScopeVector.Length == 0);
            if (topLevelExportData == null)
                return null;

            return this.GetDataFileByLevelName(allLevels, topLevelExportData.LevelName);
        }

        public AssignmentPreloadedDataRecord[] CreatePreloadedDataDtosFromPanelData(PreloadedDataByFile[] allLevels)
        {
            var topLevelData = this.GetTopLevelData(allLevels);
            
            if (topLevelData == null)
                return null;

            var supervisorsCache = new Dictionary<string, Guid>();
            var interviewersCache = new Dictionary<string, UserView>();
            var idColumnIndex = this.GetIdColumnIndex(topLevelData);
            var responsibleNameIndex = this.GetResponsibleNameIndex(topLevelData);
            var quantityIndex = this.GetQuantityIndex(topLevelData);
            var result = new List<AssignmentPreloadedDataRecord>();
            
            foreach (var topLevelRow in topLevelData.Content)
            {
                var rowId = topLevelRow[idColumnIndex];
                var answersByTopLevel = this.BuildAnswerForLevel(topLevelRow, topLevelData.Header, topLevelData.FileName);
                var levels = new List<PreloadedLevelDto> { new PreloadedLevelDto(new decimal[0], answersByTopLevel) };
                var answersinsideRosters = this.GetHierarchicalAnswersByLevelName(topLevelData.FileName, new[] { rowId }, allLevels);
                levels.AddRange(answersinsideRosters);

                var responsibleName = this.CheckAndGetSupervisorNameForLevel(topLevelRow, responsibleNameIndex);
                int? quantity = this.CheckAndGetQuantityForLevel(topLevelRow, quantityIndex);

                Guid? interviewerId = null;
                Guid? supervisorId = null;
                var interviewer = this.GetInterviewerIdAndUpdateCache(interviewersCache, responsibleName);
                if (interviewer != null)
                {
                    interviewerId = interviewer.PublicKey;
                    supervisorId = interviewer.Supervisor.Id;
                }
                else
                {
                    supervisorId = this.GetSupervisorIdAndUpdateCache(supervisorsCache, responsibleName);
                }

                result.Add(new AssignmentPreloadedDataRecord
                {
                    PreloadedDataDto = new PreloadedDataDto(levels.ToArray()),
                    SupervisorId = supervisorId,
                    InterviewerId = interviewerId,
                    Quantity = quantity
                });
            }
            return result.ToArray();
        }

        public AssignmentPreloadedDataRecord[] CreatePreloadedDataDtoFromAssignmentData(PreloadedDataByFile sampleDataFile)
        {
            var result = new List<AssignmentPreloadedDataRecord>();
            var supervisorsCache = new Dictionary<string, Guid>();

            var interviewersCache = new Dictionary<string, UserView>();

            var responsibleNameIndex = this.GetResponsibleNameIndex(sampleDataFile);
            var quantityIndex = this.GetQuantityIndex(sampleDataFile);
            var topLevelFileName = this.GetValidFileNameForTopLevelQuestionnaire();
            foreach (var contentRow in sampleDataFile.Content)
            {
                var answersToFeaturedQuestions = this.BuildAnswerForLevel(contentRow, sampleDataFile.Header, topLevelFileName);

                int? quantity = this.CheckAndGetQuantityForLevel(contentRow, quantityIndex); 

                var responsibleName = this.CheckAndGetSupervisorNameForLevel(contentRow, responsibleNameIndex);
                Guid? interviewerId = null;
                Guid? supervisorId = null;
                var interviewer = this.GetInterviewerIdAndUpdateCache(interviewersCache, responsibleName);
                if (interviewer != null)
                {
                    interviewerId = interviewer.PublicKey;
                    supervisorId = interviewer.Supervisor.Id;
                }
                else
                {
                    supervisorId = this.GetSupervisorIdAndUpdateCache(supervisorsCache, responsibleName);
                }

                result.Add(new AssignmentPreloadedDataRecord
                {
                        PreloadedDataDto = new PreloadedDataDto(new[] { new PreloadedLevelDto(new decimal[0], answersToFeaturedQuestions)}),
                        SupervisorId = supervisorId,
                        InterviewerId = interviewerId,
                        Quantity = quantity
                });
            }
            return result.ToArray();
        }

        private int GetResponsibleNameIndex(PreloadedDataByFile dataFile)
        {
            return dataFile.Header.ToList().FindIndex(header => string.Equals(header, ServiceColumns.ResponsibleColumnName, StringComparison.InvariantCultureIgnoreCase));
        }

        private int GetQuantityIndex(PreloadedDataByFile dataFile)
        {
            return dataFile.Header.ToList().FindIndex(header => string.Equals(header, ServiceColumns.AssignmentsCountColumnName, StringComparison.InvariantCultureIgnoreCase));
        }

        private string CheckAndGetSupervisorNameForLevel(string[] row, int supervisorNameIndex)
        {
            return (supervisorNameIndex >= 0) ? row[supervisorNameIndex] : string.Empty;
        }

        private int? CheckAndGetQuantityForLevel(string[] row, int quantityIndex)
        {
            const int defaultQuantityValue = 1;

            if (quantityIndex < 0)
                return defaultQuantityValue;

            var quantityString = row[quantityIndex].Trim();
            if (quantityString == "-1" || quantityString == "INF")
                return null;

            if (int.TryParse(quantityString, out int quantity))
                return quantity;

            return defaultQuantityValue;
        }

        public string GetValidFileNameForTopLevelQuestionnaire()
        {
            return this.exportStructure.HeaderToLevelMap.Values.FirstOrDefault(level => level.LevelScopeVector.Count == 0).LevelName;
        }

        public bool IsQuestionRosterSize(string variableName)
        {
            if (!this.QuestionsCache.ContainsKey(variableName))
                return false;
            var question = this.QuestionsCache[variableName];

            return this.GroupsCache.Values.Any(g => g.RosterSizeQuestionId == question.PublicKey);
        }

        public bool IsRosterSizeQuestionForLongRoster(Guid questionId)
        {
            IEnumerable<IGroup> rosters = this.GroupsCache.Values.Where(g => g.RosterSizeQuestionId == questionId);
            foreach (var roster in rosters)
            {
                if (roster.Children.Count > Constants.MaxAmountOfItemsInLongRoster)
                {
                    return false;
                }

                if (GetParentGroups(roster).Any(x => x.IsRoster))
                {
                    return false;
                }

                var hasNestedRosters = roster.Find<IGroup>(group => group.IsRoster).Any();

                if (hasNestedRosters)
                    return false;
            }
            return true;
        }

        public IEnumerable<string> GetAllParentColumnNamesForLevel(ValueVector<Guid> levelScopeVector)
        {
            return exportStructure.GetAllParentColumnNamesForLevel(levelScopeVector);
        }

        public bool IsVariableColumn(string columnName)
        {
            return VariablesCache.ContainsKey(columnName);
        }

        private IEnumerable<IGroup> GetParentGroups(IGroup roster)
        {
            var parent = roster.GetParent();
            while (!(parent is IQuestionnaireDocument))
            {
                yield return (IGroup)parent;
                parent = parent.GetParent();
            }
        }

        private PreloadedDataByFile GetDataFileByLevelName(PreloadedDataByFile[] allLevels, string name)
        {
            return allLevels.FirstOrDefault(l => string.Equals(Path.GetFileNameWithoutExtension(l.FileName),name, StringComparison.OrdinalIgnoreCase));
        }

        private PreloadedLevelDto[] GetHierarchicalAnswersByLevelName(string levelName, string[] parentIds, PreloadedDataByFile[] rosterData)
        {
            var result = new List<PreloadedLevelDto>();
            var childFiles = this.GetChildDataFiles(levelName, rosterData);

            foreach (var preloadedDataByFile in childFiles)
            {
                var parentIdColumnIndexes = this.GetParentIdColumnIndexes(preloadedDataByFile);
                var idColumnIndex = this.GetIdColumnIndex(preloadedDataByFile);
                var childRecordsOfCurrentRow =
                    preloadedDataByFile.Content.Where(
                        record => parentIdColumnIndexes.Select(parentIdColumnIndex => record[parentIdColumnIndex]).SequenceEqual(parentIds))
                        .ToArray();

                foreach (var rosterRow in childRecordsOfCurrentRow)
                {
                    var rosterAnswers = this.BuildAnswerForLevel(rosterRow, preloadedDataByFile.Header, preloadedDataByFile.FileName);

                    var newParentIds = this.ExtendParentIdsVectorWithCurrentRecordIdOnAFirstPlace(parentIds, rosterRow[idColumnIndex]);
                    var newRosterVetor = this.CreateRosterVectorFromParentIds(newParentIds);
                 
                    result.Add(new PreloadedLevelDto(newRosterVetor, rosterAnswers));
                    result.AddRange(this.GetHierarchicalAnswersByLevelName(preloadedDataByFile.FileName, newParentIds, rosterData));
                }
            }

            return result.ToArray();
        }

        private string[] ExtendParentIdsVectorWithCurrentRecordIdOnAFirstPlace(string[] parentIds, string currentRecordId)
        {
            var newParentIds = new string[parentIds.Length + 1];
            newParentIds[0] = currentRecordId;
            for (int i = 0; i < parentIds.Length; i++)
            {
                newParentIds[i + 1] = parentIds[i];
            }
            return newParentIds;
        }

        private decimal[] CreateRosterVectorFromParentIds(string[] parentIds)
        {
            var result = new List<decimal>();
            for (int i = 0; i < parentIds.Length-1; i++)
            {
                result.Add(decimal.Parse(parentIds[i]));
            }
            result.Reverse();
            return result.ToArray();
        }

        private Dictionary<Guid, AbstractAnswer> BuildAnswerForLevel(string[] row, string[] header, string levelFileName)
        {
            var levelExportStructure = this.FindLevelInPreloadedData(levelFileName);
            if (levelExportStructure == null)
                return null;
            var result = new Dictionary<Guid, AbstractAnswer>();

            var rowWithoutMissingValues = row.Select(v =>
                  v?.Replace(ExportFormatSettings.MissingNumericQuestionValue, string.Empty)
                    .Replace(ExportFormatSettings.MissingStringQuestionValue, string.Empty))
                .ToArray();

            foreach (var exportedHeaderItem in levelExportStructure.HeaderItems.Values)
            {
                var parsedAnswer = this.BuildAnswerByVariableName(levelExportStructure, exportedHeaderItem.VariableName, header, rowWithoutMissingValues);

                if (parsedAnswer!=null)
                    result.Add(exportedHeaderItem.PublicKey, parsedAnswer);
            }
            return result;
        }

        private AbstractAnswer BuildAnswerByVariableName(HeaderStructureForLevel levelExportStructure, string variableName, string[] header, string[] row)
        {
            var exportedHeaderItem =
                levelExportStructure.HeaderItems.Values.FirstOrDefault(
                    h => string.Equals(h.VariableName, variableName, StringComparison.OrdinalIgnoreCase));
            if (exportedHeaderItem == null)
                return null;

            var headerIndexes = new List<Tuple<string, string>>();

            for (int i = 0; i < header.Length; i++)
            {
                if (exportedHeaderItem.ColumnNames.Contains(header[i]) && !string.IsNullOrEmpty(row[i]))
                    headerIndexes.Add(new Tuple<string, string>(header[i], row[i]));
            }
            if (!headerIndexes.Any())
                return null;

            var question = this.GetQuestionByVariableName(exportedHeaderItem.VariableName);
            
            return this.dataParser.BuildAnswerFromStringArray(headerIndexes.ToArray(), question);
        }

        private PreloadedDataByFile[] GetChildDataFiles(string levelFileName, PreloadedDataByFile[] allLevels)
        {
            var levelExportStructure = this.FindLevelInPreloadedData(levelFileName);
            if (levelExportStructure == null)
                return new PreloadedDataByFile[0];
            IEnumerable<RosterScopeDescription> children;

            if (levelExportStructure.LevelScopeVector.Length == 0)
                children =
                    this.rosterScopes.Values.Where(
                        scope => scope.ScopeVector.Length == 1);
            else
                children =
                    this.rosterScopes.Values.Where(
                        scope =>
                            scope.ScopeVector.Length == levelExportStructure.LevelScopeVector.Length + 1 &&
                                scope.ScopeVector.Take(levelExportStructure.LevelScopeVector.Length)
                                    .SequenceEqual(levelExportStructure.LevelScopeVector) &&
                                this.exportStructure.HeaderToLevelMap.ContainsKey(levelExportStructure.LevelScopeVector));

            return
                children
                    .Select(scope => this.exportStructure.HeaderToLevelMap[scope.ScopeVector]).Select(
                        child =>
                            this.GetDataFileByLevelName(allLevels, child.LevelName)).Where(file => file != null).ToArray();
        }

        public IQuestion GetQuestionByVariableName(string variableName)
        {
            var varName = variableName.ToLower();
            return this.QuestionsCache.ContainsKey(varName) ? this.QuestionsCache[varName] : null;
        }

        protected UserView GetUserByName(string userName)
        {
            return this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                var user = this.userViewFactory.GetUser(new UserViewInputModel(UserName: userName, UserEmail: null));
                if (user == null || user.IsArchived)
                    return null;
                return user;
            });
        }

        private Guid? GetSupervisorIdAndUpdateCache(Dictionary<string, Guid> cache, string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var userNameLowerCase = name.ToLower();
            if (cache.ContainsKey(userNameLowerCase))
                return cache[userNameLowerCase];

            var user = this.GetUserByName(userNameLowerCase);//assuming that user exists
            if (user == null || !user.IsSupervisor()) throw new Exception($"Supervisor with name '{name}' does not exists");

            cache.Add(userNameLowerCase, user.PublicKey);
            return user.PublicKey;
        }

        private UserView GetInterviewerIdAndUpdateCache(Dictionary<string, UserView> interviewerCache, string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var userNameLowerCase = name.ToLower();
            if (interviewerCache.ContainsKey(userNameLowerCase))
                return interviewerCache[userNameLowerCase];

            var user = this.GetUserByName(userNameLowerCase);//assuming that user exists
            if (user == null || !user.Roles.Contains(UserRoles.Interviewer)) return null;

            interviewerCache.Add(userNameLowerCase, user);
            return user;
        }
    }
}

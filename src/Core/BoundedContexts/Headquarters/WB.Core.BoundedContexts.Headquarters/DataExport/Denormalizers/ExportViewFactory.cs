using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.V4.CustomFunctions;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Infrastructure.Native.Sanitizer;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers
{
    [Obsolete("KP-11815")]
    internal class ExportViewFactory : IExportViewFactory
    {
        private const string GeneratedTitleExportFormat = "{0}__{1}";
        
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IExportQuestionService exportQuestionService;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IRosterStructureService rosterStructureService;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage;

        public ExportViewFactory(
            IFileSystemAccessor fileSystemAccessor,
            IExportQuestionService exportQuestionService,
            IQuestionnaireStorage questionnaireStorage,
            IRosterStructureService rosterStructureService,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.exportQuestionService = exportQuestionService;
            this.questionnaireStorage = questionnaireStorage;
            this.rosterStructureService = rosterStructureService;
            this.questionnaireBrowseItemStorage = questionnaireBrowseItemStorage;
        }

        public QuestionnaireExportStructure CreateQuestionnaireExportStructure(Guid id, long version)
        {
            return CreateQuestionnaireExportStructure(new QuestionnaireIdentity(id, version));
        }

        public QuestionnaireExportStructure CreateQuestionnaireExportStructure(QuestionnaireIdentity id)
        {
            QuestionnaireDocument questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(id);
            if (questionnaire == null)
                return null;

            return CreateQuestionnaireExportStructure(questionnaire, id);
        }

        public QuestionnaireExportStructure CreateQuestionnaireExportStructure(QuestionnaireDocument questionnaire, QuestionnaireIdentity id)
        {
            var result = new QuestionnaireExportStructure
            {
                QuestionnaireId = id.QuestionnaireId,
                Version = id.Version
            };

            var questionnaireBrowseItem = questionnaireBrowseItemStorage.GetById(id.ToString());
            var supportVariables = questionnaireBrowseItem != null && questionnaireBrowseItem.AllowExportVariables;

            var rosterScopes = this.rosterStructureService.GetRosterScopes(questionnaire);
            var maxValuesForRosterSizeQuestions = GetMaxValuesForRosterSizeQuestions(questionnaire);

            result.HeaderToLevelMap.Add(new ValueVector<Guid>(),
                this.BuildHeaderByTemplate(questionnaire, supportVariables, new ValueVector<Guid>(), rosterScopes, maxValuesForRosterSizeQuestions));

            foreach (var rosterScopeDescription in rosterScopes)
            {
                result.HeaderToLevelMap.Add(rosterScopeDescription.Key,
                    this.BuildHeaderByTemplate(questionnaire, supportVariables, rosterScopeDescription.Key, rosterScopes, maxValuesForRosterSizeQuestions));
            }

            return result;
        }

        public InterviewDataExportView CreateInterviewDataExportView(QuestionnaireExportStructure exportStructure,
            InterviewData interview, IQuestionnaire questionnaire)
        {
            var interviewDataExportLevelViews = new List<InterviewDataExportLevelView>();

            foreach (var exportStructureForLevel in exportStructure.HeaderToLevelMap.Values)
            {
                var interviewDataExportRecords = this.BuildRecordsForHeader(interview, exportStructureForLevel, questionnaire);

                var interviewDataExportLevelView = new InterviewDataExportLevelView(
                    exportStructureForLevel.LevelScopeVector, 
                    exportStructureForLevel.LevelName,
                    interviewDataExportRecords);

                interviewDataExportLevelViews.Add(interviewDataExportLevelView);
            }

            return new InterviewDataExportView(interview.InterviewId, interviewDataExportLevelViews.ToArray());
        }

        private InterviewDataExportRecord[] BuildRecordsForHeader(InterviewData interview,
            HeaderStructureForLevel headerStructureForLevel, IQuestionnaire questionnaire)
        {
            var dataRecords = new List<InterviewDataExportRecord>();

            var interviewDataByLevels = this.GetLevelsFromInterview(interview, headerStructureForLevel.LevelScopeVector);

            foreach (InterviewLevel dataByLevel in interviewDataByLevels)
            {
                var vectorLength = dataByLevel.RosterVector.Length;

                string recordId = interview.InterviewId.FormatGuid();
                
                string[] parentRecordIds = new string[vectorLength];
                string[] systemVariableValues = Array.Empty<string>();

                if (vectorLength == 0)
                    systemVariableValues = this.GetSystemValues(interview, ServiceColumns.SystemVariables.Values);
                else
                {
                    var rosterIndexAdjustment = headerStructureForLevel.LevelScopeVector
                        .Select(x => (questionnaire.IsQuestion(x) && questionnaire.IsQuestionInteger(x)) ? 1 : 0)
                        .ToArray();
                    
                    recordId = (dataByLevel.RosterVector.Last() + rosterIndexAdjustment.Last())
                        .ToString(CultureInfo.InvariantCulture);

                    parentRecordIds[0] = interview.InterviewId.FormatGuid();
                    for (int i = 0; i < vectorLength - 1; i++)
                    {
                        parentRecordIds[i + 1] = (dataByLevel.RosterVector[i] + rosterIndexAdjustment[i]).ToString(CultureInfo.InvariantCulture);
                    }

                    parentRecordIds = parentRecordIds.Reverse().ToArray();
                }

                string[] referenceValues = Array.Empty<string>();

                if (headerStructureForLevel.IsTextListScope)
                {
                    referenceValues = new[]
                    {
                        this.GetTextValueForTextListQuestion(interview, dataByLevel.RosterVector, headerStructureForLevel.LevelScopeVector.Last())
                    };
                }

                string[][] questionsForExport = this.GetExportValues(dataByLevel, headerStructureForLevel);

                dataRecords.Add(new InterviewDataExportRecord(recordId,
                    referenceValues,
                    parentRecordIds,
                    systemVariableValues)
                {
                    Answers = questionsForExport
                });
            }

            return dataRecords.ToArray();
        }

        private string CreateLevelIdFromPropagationVector(decimal[] vector)
        {
            return vector.Length == 0 ? "#" : vector.CreateLeveKeyFromPropagationVector();
        }

        private string[] GetSystemValues(InterviewData interview, IEnumerable<ServiceVariable> variables)
        {
            List<string> values = new List<string>();

            foreach (var header in variables)
            {
                values.Add(this.GetSystemValue(interview, header));
            }
            return values.ToArray();

        }

        private string GetSystemValue(InterviewData interview, ServiceVariable serviceVariable)
        {
            switch (serviceVariable.VariableType)
            {
                case ServiceVariableType.InterviewRandom:
                    return interview.InterviewId.GetRandomDouble().ToString(CultureInfo.InvariantCulture);
                case ServiceVariableType.InterviewKey:
                    return interview.InterviewKey ?? string.Empty;
            }

            return String.Empty;
        }

        private string GetTextValueForTextListQuestion(InterviewData interview, decimal[] rosterVector, Guid id)
        {
            decimal itemToSearch = rosterVector.Last();

            for (var i = 1; i <= rosterVector.Length; i++)
            {
                var levelForVector =
                    interview.Levels.GetOrNull(
                        this.CreateLevelIdFromPropagationVector(rosterVector.Take(rosterVector.Length - i).ToArray()));

                var questionToCheck = levelForVector?.QuestionsSearchCache.GetOrNull(id);

                if (questionToCheck == null)
                    continue;


                InterviewTextListAnswer item = null;
                if (questionToCheck.Answer is InterviewTextListAnswers interviewTextListAnswers)
                    item = interviewTextListAnswers.Answers.FirstOrDefault(a => a.Value == itemToSearch);
                if (questionToCheck.Answer is InterviewTextListAnswer[] interviewTextListAnswer)
                    item = interviewTextListAnswer.FirstOrDefault(a => a.Value == itemToSearch);

                return item != null ? item.Answer : string.Empty;
            }

            return string.Empty;
        }

        private string[][] GetExportValues(InterviewLevel interviewLevel, HeaderStructureForLevel headerStructureForLevel)
        {
            var result = new List<string[]>();
            foreach (var headerItem in headerStructureForLevel.HeaderItems.Values)
            {
                var questionHeaderItem = headerItem as ExportedQuestionHeaderItem;
                var variableHeaderItem = headerItem as ExportedVariableHeaderItem;

                if (questionHeaderItem != null)
                { 
                    var question = interviewLevel.QuestionsSearchCache.ContainsKey(headerItem.PublicKey) 
                        ? interviewLevel.QuestionsSearchCache[headerItem.PublicKey] 
                        : null;
                    var exportedQuestion = exportQuestionService.GetExportedQuestion(question, questionHeaderItem);
                    result.Add(exportedQuestion);
                }
                else if (variableHeaderItem != null)
                {
                    var variable = interviewLevel.Variables.ContainsKey(headerItem.PublicKey)
                        ? interviewLevel.Variables[headerItem.PublicKey]
                        : null;
                    var isDisabled = interviewLevel.DisabledVariables.Contains(headerItem.PublicKey);
                    var exportedVariable = exportQuestionService.GetExportedVariable(variable, variableHeaderItem, isDisabled);
                    result.Add(exportedVariable);
                }
                else
                {
                    throw  new ArgumentException("Unknown export header");
                }
            }
            return result.ToArray();
        }

        private List<InterviewLevel> GetLevelsFromInterview(InterviewData interview, ValueVector<Guid> levelVector)
        {
            if (!levelVector.Any())
            {
                var levels = interview.Levels.Values.Where(level => level.RosterScope.Equals(new ValueVector<Guid>())).ToList();
                return levels.Any() 
                    ? levels 
                    : new List<InterviewLevel> {new  InterviewLevel {RosterVector = new decimal[0]}} ;
            }
            return interview.Levels.Values.Where(level => level.RosterScope.Equals(levelVector)).ToList();
        }

        protected HeaderStructureForLevel CreateHeaderStructureForLevel(
            string levelTitle,
            IEnumerable<IGroup> groupsInLevel,
            QuestionnaireDocument questionnaire,
            bool supportVariables,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions,
            ValueVector<Guid> levelVector)
        {
            var headerStructureForLevel = new HeaderStructureForLevel();
            headerStructureForLevel.LevelScopeVector = levelVector;
            headerStructureForLevel.LevelIdColumnName = (levelVector == null || levelVector.Length == 0) ? ServiceColumns.InterviewId : string.Format(ServiceColumns.IdSuffixFormat, levelTitle);

            headerStructureForLevel.LevelName = levelTitle;

            foreach (var rootGroup in groupsInLevel)
            {
                this.FillHeaderWithQuestionsInsideGroup(headerStructureForLevel, rootGroup, questionnaire, supportVariables,
                    maxValuesForRosterSizeQuestions);
            }

            return headerStructureForLevel;
        }

        protected ExportedVariableHeaderItem CreateExportedVariableHeaderItem(IVariable variable)
        {
            var exportedHeaderItem = new ExportedVariableHeaderItem();

            exportedHeaderItem.PublicKey = variable.PublicKey;
            exportedHeaderItem.VariableType = variable.Type;
            exportedHeaderItem.VariableName = variable.Name;

            exportedHeaderItem.ColumnHeaders = new List<HeaderColumn>(){
                new HeaderColumn()
                {
                    Name = variable.Name,
                    Title = variable.Label,
                    ExportType = GetGtorageType(variable) 
                }};

            return exportedHeaderItem;
        }

        private ExportValueType GetGtorageType(IVariable variable)
        {
            switch (variable.Type)
            {
                case VariableType.Boolean:
                    return ExportValueType.Boolean;
                case VariableType.DateTime:
                    return ExportValueType.DateTime;
                case VariableType.LongInteger:
                    return ExportValueType.Numeric;
                case VariableType.Double:
                    return ExportValueType.Numeric;
                case VariableType.String:
                    return ExportValueType.String;
                default:
                    return ExportValueType.Unknown;
            }
        }

        protected ExportedQuestionHeaderItem CreateExportedQuestionHeaderItem(IQuestion question, int? lengthOfRosterVectorWhichNeedToBeExported)
        {
            var exportedHeaderItem = new ExportedQuestionHeaderItem();

            exportedHeaderItem.PublicKey = question.PublicKey;
            exportedHeaderItem.QuestionType = question.QuestionType;
            exportedHeaderItem.IsIdentifyingQuestion = question.Featured;

            if (question is IMultyOptionsQuestion)
            {
                var multioptionQuestion = (IMultyOptionsQuestion) question;
                if (multioptionQuestion.LinkedToQuestionId.HasValue || multioptionQuestion.LinkedToRosterId.HasValue)
                {
                    exportedHeaderItem.QuestionSubType = QuestionSubtype.MultyOption_Linked;
                }
                else if (multioptionQuestion.YesNoView)
                {
                    exportedHeaderItem.QuestionSubType = multioptionQuestion.AreAnswersOrdered
                        ? QuestionSubtype.MultyOption_YesNoOrdered
                        : QuestionSubtype.MultyOption_YesNo;
                }
                else if(multioptionQuestion.AreAnswersOrdered)
                {
                    exportedHeaderItem.QuestionSubType = QuestionSubtype.MultyOption_Ordered;
                }
            }

            if (question is DateTimeQuestion dateTimeQuestion)
            {
                if (dateTimeQuestion.IsTimestamp)
                    exportedHeaderItem.QuestionSubType = QuestionSubtype.DateTime_Timestamp;
            }

            if (question is SingleQuestion singleQuestion)
            {
                if (singleQuestion.LinkedToQuestionId.HasValue || singleQuestion.LinkedToRosterId.HasValue)
                    exportedHeaderItem.QuestionSubType = QuestionSubtype.SingleOption_Linked;
            }

            exportedHeaderItem.VariableName = question.StataExportCaption;
            
            exportedHeaderItem.ColumnHeaders = new List<HeaderColumn>()
            {
                new HeaderColumn
                {
                    Name = question.StataExportCaption,
                    Title = string.IsNullOrEmpty(question.VariableLabel) ? question.QuestionText.RemoveHtmlTags() : question.VariableLabel,
                    ExportType = GetGtorageType(question, exportedHeaderItem.QuestionSubType)
                }
            };

            exportedHeaderItem.Labels = new List<LabelItem>();
            if (question.Answers != null)
            {
                foreach (var answer in question.Answers)
                {
                    exportedHeaderItem.Labels.Add(new LabelItem(answer));
                }
            }

            if (lengthOfRosterVectorWhichNeedToBeExported.HasValue)
            {
                exportedHeaderItem.LengthOfRosterVectorWhichNeedToBeExported = lengthOfRosterVectorWhichNeedToBeExported;
            }

            return exportedHeaderItem;
        }

        private ExportValueType GetGtorageType(IQuestion question, QuestionSubtype? questionSubType = null)
        {
            switch (question.QuestionType)
            {
                case QuestionType.Area:
                case QuestionType.Audio:
                case QuestionType.Text:
                case QuestionType.TextList:
                case QuestionType.QRBarcode:
                case QuestionType.Multimedia:
                    return ExportValueType.String;
                case QuestionType.Numeric:
                    return ExportValueType.Numeric;
                case QuestionType.DateTime:
                {
                    return (questionSubType != null && questionSubType == QuestionSubtype.DateTime_Timestamp) ? ExportValueType.DateTime :  ExportValueType.Date;
                }
                    
                case QuestionType.MultyOption:
                case QuestionType.SingleOption:
                {
                    return (question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue) ? ExportValueType.String : ExportValueType.NumericInt;
                }
                default:
                        return ExportValueType.Unknown;
            }
        }

        private ExportedQuestionHeaderItem CreateExportedQuestionHeaderForMultiColumnItem(IQuestion question, int columnCount,
            int? lengthOfRosterVectorWhichNeedToBeExported)
        {
            var exportedHeaderItem = this.CreateExportedQuestionHeaderItem(question, lengthOfRosterVectorWhichNeedToBeExported);
            this.ThrowIfQuestionIsNotMultiSelectOrTextList(question);

            exportedHeaderItem.ColumnValues = new int[columnCount];
            exportedHeaderItem.ColumnHeaders = new List<HeaderColumn>();

            for (int i = 0; i < columnCount; i++)
            {
                HeaderColumn headerColumn = new HeaderColumn();

                if (!IsQuestionLinked(question) && question is IMultyOptionsQuestion)
                {
                    var columnValue = int.Parse(question.Answers[i].AnswerValue);
              
                    headerColumn.Name = string.Format(GeneratedTitleExportFormat,
                        question.StataExportCaption, DecimalToHeaderConverter.ToHeader(columnValue));

                    exportedHeaderItem.ColumnValues[i] = columnValue;
                }
                else
                {
                    headerColumn.Name = string.Format(GeneratedTitleExportFormat, question.StataExportCaption, i);
                }

                if (!IsQuestionLinked(question))
                {
                    var questionLabel =
                        string.IsNullOrEmpty(question.VariableLabel) ? question.QuestionText : question.VariableLabel;

                    if (question is IMultyOptionsQuestion)
                    {
                        headerColumn.Title = $"{questionLabel}:{question.Answers[i].AnswerText}";
                    }
                    if (question is ITextListQuestion)
                    {
                        headerColumn.Title = $"{questionLabel}:{i}";
                    }
                }

                headerColumn.ExportType = GetGtorageType(question);
                exportedHeaderItem.ColumnHeaders.Add(headerColumn);
            }
            return exportedHeaderItem;
        }

        private static bool IsQuestionLinked(IQuestion question)
        {
            return question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue;
        }

        private void ThrowIfQuestionIsNotMultiSelectOrTextList(IQuestion question)
        {
            if (question.QuestionType != QuestionType.MultyOption && question.QuestionType != QuestionType.TextList)
                throw new InvalidOperationException(string.Format(
                    "question '{1}' with type '{0}' can't be exported as more then one column",
                    question.QuestionType, question.QuestionText));
        }

        private static Dictionary<Guid, int> GetMaxValuesForRosterSizeQuestions(QuestionnaireDocument document)
        {
            var rosterGroups = document.Find<IGroup>(@group => @group.IsRoster && @group.RosterSizeQuestionId.HasValue);

            var fixedRosterGroups =
                document.Find<IGroup>(@group => @group.IsRoster && @group.RosterSizeSource == RosterSizeSourceType.FixedTitles);

            IEnumerable<IMultyOptionsQuestion> rosterSizeMultyOptionQuestions =
                rosterGroups.Select(@group => document.Find<IMultyOptionsQuestion>(@group.RosterSizeQuestionId.Value))
                    .Where(question => question != null).Distinct();

            IEnumerable<ITextListQuestion> rosterSizeTextListQuestions =
                rosterGroups.Select(@group => document.Find<ITextListQuestion>(@group.RosterSizeQuestionId.Value))
                    .Where(question => question != null).Distinct();

            var collectedMaxValues = new Dictionary<Guid, int>();

            foreach (IMultyOptionsQuestion rosterSizeMultyOptionQuestion in rosterSizeMultyOptionQuestions)
            {
                collectedMaxValues.Add(rosterSizeMultyOptionQuestion.PublicKey, rosterSizeMultyOptionQuestion.Answers.Count);
            }

            foreach (ITextListQuestion rosterSizeTextListQuestion in rosterSizeTextListQuestions)
            {
                collectedMaxValues.Add(rosterSizeTextListQuestion.PublicKey,
                    rosterSizeTextListQuestion.MaxAnswerCount ?? TextListQuestion.MaxAnswerCountLimit);
            }

            foreach (IGroup fixedRosterGroup in fixedRosterGroups)
            {
                collectedMaxValues.Add(fixedRosterGroup.PublicKey, fixedRosterGroup.FixedRosterTitles.Length);
            }

            return collectedMaxValues;
        }

        private HeaderStructureForLevel BuildHeaderByTemplate(QuestionnaireDocument questionnaire, bool supportVariables, ValueVector<Guid> levelVector,
            Dictionary<ValueVector<Guid>, RosterScopeDescription> rosterScopes, Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            IEnumerable<IGroup> rootGroups = this.GetRootGroupsForLevel(questionnaire, rosterScopes, levelVector);

            if (!rootGroups.Any())
                throw new InvalidOperationException("level is absent in template");

            var firstRootGroup = rootGroups.First();
            var levelTitle = firstRootGroup.VariableName ?? this.fileSystemAccessor.MakeStataCompatibleFileName(firstRootGroup.Title);

            var structures = this.CreateHeaderStructureForLevel(levelTitle, rootGroups, questionnaire, supportVariables,
                maxValuesForRosterSizeQuestions, levelVector);

            if (rosterScopes.ContainsKey(levelVector) &&
                rosterScopes[levelVector].Type == RosterScopeType.TextList)
            {
                structures.IsTextListScope = true;
                structures.ReferencedNames = new string[] { rosterScopes[levelVector].SizeQuestionTitle };
            }

            return structures;
        }

        private IEnumerable<IGroup> GetRootGroupsForLevel(QuestionnaireDocument questionnaire,
            Dictionary<ValueVector<Guid>, RosterScopeDescription> rosterScopes, ValueVector<Guid> levelVector)
        {
            if (!levelVector.Any())
            {
                yield return questionnaire;
                yield break;
            }

            var rootGroupsForLevel = this.GetRootGroupsByLevelIdOrThrow(rosterScopes, levelVector);

            foreach (var rootGroup in rootGroupsForLevel)
            {
                yield return questionnaire.FirstOrDefault<IGroup>(group => group.PublicKey == rootGroup);
            }
        }

        private HashSet<Guid> GetRootGroupsByLevelIdOrThrow(Dictionary<ValueVector<Guid>, RosterScopeDescription> rosterScopes,
            ValueVector<Guid> levelVector)
        {
            if (!rosterScopes.ContainsKey(levelVector))
                throw new InvalidOperationException("level is absent in template");

            return new HashSet<Guid>(rosterScopes[levelVector].RosterIdToRosterTitleQuestionIdMap.Keys);
        }

        private void FillHeaderWithQuestionsInsideGroup(HeaderStructureForLevel headerStructureForLevel, 
            IGroup @group,
            QuestionnaireDocument questionnaire,
            bool supportVariables,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            if (@group.RosterSizeSource == RosterSizeSourceType.FixedTitles && headerStructureForLevel.LevelLabels == null)
            {
                headerStructureForLevel.LevelLabels =
                    @group.FixedRosterTitles.Select(title => new LabelItem() { Caption = title.Value.ToString(CultureInfo.InvariantCulture), Title = title.Title })
                        .ToArray();
            }

            foreach (var groupChild in @group.Children)
            {
                if (groupChild is IQuestion question)
                {
                    if (this.IsQuestionMultiOption(question))
                    {
                        if (question.LinkedToRosterId.HasValue)
                            this.AddHeadersForLinkedMultiOptions(headerStructureForLevel.HeaderItems, question, questionnaire, maxValuesForRosterSizeQuestions);
                        else if (question.LinkedToQuestionId.HasValue)
                        {
                            var linkToQuestion =
                                questionnaire.FirstOrDefault<IQuestion>(
                                    x => x.PublicKey == question.LinkedToQuestionId.Value);

                            if (linkToQuestion.QuestionType == QuestionType.TextList)
                            {
                                this.AddHeadersForLinkedToListMultiOptions(headerStructureForLevel.HeaderItems, question, linkToQuestion, questionnaire);
                            }
                            else
                                this.AddHeadersForLinkedMultiOptions(headerStructureForLevel.HeaderItems, question, questionnaire, maxValuesForRosterSizeQuestions);
                        }

                        else this.AddHeadersForMultiOptions(headerStructureForLevel.HeaderItems, question, questionnaire);
                    }
                    else if (this.IsQuestionTextList(question))
                    {
                        this.AddHeadersForTextList(headerStructureForLevel.HeaderItems, question, questionnaire);
                    }
                    else
                    {
                        if (question is GpsCoordinateQuestion)
                            this.AddHeadersForGpsQuestion(headerStructureForLevel.HeaderItems, question,
                                questionnaire);
                        else
                            this.AddHeaderForSingleColumnExportQuestion(headerStructureForLevel.HeaderItems, question,
                                questionnaire);
                    }
                    continue;
                }

                if (groupChild is IVariable variable)
                {
                    if (supportVariables)
                        AddHeadersForVariable(headerStructureForLevel.HeaderItems, variable);

                    continue;
                }

                if (groupChild is IGroup innerGroup)
                {
                    if (innerGroup.IsRoster)
                        continue;
                    this.FillHeaderWithQuestionsInsideGroup(headerStructureForLevel, innerGroup, questionnaire, supportVariables,
                        maxValuesForRosterSizeQuestions);
                }
            }
        }

        private bool IsQuestionMultiOption(IQuestion question)
        {
            return question is IMultyOptionsQuestion;
        }

        private bool IsQuestionTextList(IQuestion question)
        {
            return question is ITextListQuestion;
        }

        private void AddHeadersForVariable(IDictionary<Guid, IExportedHeaderItem> headerItems, IVariable variable)
        {
            headerItems.Add(variable.PublicKey, this.CreateExportedVariableHeaderItem(variable));
        }

        private void AddHeadersForLinkedMultiOptions(IDictionary<Guid, IExportedHeaderItem> headerItems, IQuestion question,
            QuestionnaireDocument questionnaire,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            headerItems.Add(question.PublicKey,
                this.CreateExportedQuestionHeaderForMultiColumnItem(question,
                    this.GetRosterSizeForLinkedQuestion(question, questionnaire, maxValuesForRosterSizeQuestions),
                    this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire)));
        }

        protected void AddHeaderForSingleColumnExportQuestion(IDictionary<Guid, IExportedHeaderItem> headerItems, IQuestion question,
            QuestionnaireDocument questionnaire)
        {
            headerItems.Add(question.PublicKey,
                this.CreateExportedQuestionHeaderItem(question,
                    this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire)));
        }

        protected void AddHeadersForMultiOptions(IDictionary<Guid, IExportedHeaderItem> headerItems, IQuestion question,
            QuestionnaireDocument questionnaire)
        {
            headerItems.Add(question.PublicKey,
                this.CreateExportedQuestionHeaderForMultiColumnItem(question, question.Answers.Count,
                    this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire)));
        }

        protected void AddHeadersForTextList(IDictionary<Guid, IExportedHeaderItem> headerItems, IQuestion question,
            QuestionnaireDocument questionnaire)
        {
            var textListQuestion = question as ITextListQuestion;
            var maxCount = (textListQuestion == null ? null : textListQuestion.MaxAnswerCount) ?? TextListQuestion.MaxAnswerCountLimit;
            headerItems.Add(question.PublicKey,
                this.CreateExportedQuestionHeaderForMultiColumnItem(question, maxCount,
                    this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire)));
        }

        private void AddHeadersForLinkedToListMultiOptions(IDictionary<Guid, IExportedHeaderItem> headerItems, 
            IQuestion question,
            IQuestion linkToTextListQuestion,
            QuestionnaireDocument questionnaire)
        {
            var textListQuestion = linkToTextListQuestion as ITextListQuestion;
            var maxCount = (textListQuestion == null ? null : textListQuestion.MaxAnswerCount) ?? TextListQuestion.MaxAnswerCountLimit;

            headerItems.Add(question.PublicKey,
                 this.CreateExportedQuestionHeaderForMultiColumnItem(question, maxCount,
                     this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire)));
        }

        protected void AddHeadersForGpsQuestion(IDictionary<Guid, IExportedHeaderItem> headerItems, IQuestion question,
            QuestionnaireDocument questionnaire)
        {
            var gpsColumns = GeoPosition.PropertyNames;
            var gpsQuestionExportHeader = this.CreateExportedQuestionHeaderItem(question,
                this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire));

            gpsQuestionExportHeader.ColumnHeaders = new List<HeaderColumn>();
            
            var questionLabel = string.IsNullOrEmpty(question.VariableLabel)
                ? question.QuestionText
                : question.VariableLabel;

            foreach (var column in gpsColumns)
            {
                gpsQuestionExportHeader.ColumnHeaders.Add(new HeaderColumn()
                {
                    Name = string.Format(GeneratedTitleExportFormat, question.StataExportCaption, column),
                    Title = $"{questionLabel}: {column}",
                    ExportType = string.Compare(column, "timestamp", StringComparison.OrdinalIgnoreCase) == 0 ? ExportValueType.DateTime : ExportValueType.Numeric
                });
            }

            headerItems.Add(question.PublicKey, gpsQuestionExportHeader);
        }

        private int GetRosterSizeForLinkedQuestion(IQuestion question, QuestionnaireDocument questionnaire,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            Guid rosterSizeQuestionId =
                this.GetRosterSizeSourcesForEntity(GetReferencedByLinkedQuestionEntity(question, questionnaire)).Last();

            if (!maxValuesForRosterSizeQuestions.ContainsKey(rosterSizeQuestionId))
                return SharedKernels.SurveySolutions.Documents.Constants.MaxRosterRowCount;

            return maxValuesForRosterSizeQuestions[rosterSizeQuestionId];
        }

        private IComposite GetReferencedByLinkedQuestionEntity(IQuestion question, QuestionnaireDocument questionnaire)
        {
            if (question.LinkedToQuestionId.HasValue)
            {
                return questionnaire.Find<IQuestion>(question.LinkedToQuestionId.Value);
            }

            if (question.LinkedToRosterId.HasValue)
            {
                return questionnaire.Find<IGroup>(question.LinkedToRosterId.Value);
            }
            return null;
        }

        public ValueVector<Guid> GetRosterSizeSourcesForEntity(IComposite entity)
        {
            var rosterSizes = new List<Guid>();
            while (!(entity is IQuestionnaireDocument))
            {
                var group = entity as IGroup;
                if (group != null)
                {
                    if (group.IsRoster)
                        rosterSizes.Add(group.RosterSizeQuestionId ?? group.PublicKey);

                }
                entity = entity.GetParent();
            }
            rosterSizes.Reverse();
            return rosterSizes.ToArray();
        }

        private int? GetLengthOfRosterVectorWhichNeedToBeExported(IQuestion question, QuestionnaireDocument questionnaire)
        {
            var referencedByLinkedQuestionEntity = GetReferencedByLinkedQuestionEntity(question, questionnaire);
            if (referencedByLinkedQuestionEntity == null)
                return null;

            return
                this.GetLengthOfRosterVectorWhichNeedToBeExported(this.GetRosterSizeSourcesForEntity(question),
                this.GetRosterSizeSourcesForEntity(referencedByLinkedQuestionEntity));
        }

        private int GetLengthOfRosterVectorWhichNeedToBeExported(ValueVector<Guid> scopeOfLinkedQuestion,
            ValueVector<Guid> scopeOfReferenceQuestion)
        {
            if (scopeOfLinkedQuestion.Length > scopeOfReferenceQuestion.Length)
            {
                return 1;
            }

            for (int i = 0; i < Math.Min(scopeOfLinkedQuestion.Length, scopeOfReferenceQuestion.Length); i++)
            {
                if (scopeOfReferenceQuestion[i] != scopeOfLinkedQuestion[i])
                {
                    return scopeOfReferenceQuestion.Length - i;
                }
            }

            if (scopeOfLinkedQuestion.Length == scopeOfReferenceQuestion.Length)
            {
                return 1;
            }

            return scopeOfReferenceQuestion.Length - scopeOfLinkedQuestion.Length;
        }
    }
}

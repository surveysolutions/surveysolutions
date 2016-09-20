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
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.V4.CustomFunctions;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers
{
    internal class ExportViewFactory : IExportViewFactory
    {
        private const string GeneratedTitleExportFormat = "{0}__{1}";
        
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IExportQuestionService exportQuestionService;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public ExportViewFactory(
            IFileSystemAccessor fileSystemAccessor,
            IExportQuestionService exportQuestionService,
            IQuestionnaireStorage questionnaireStorage)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.exportQuestionService = exportQuestionService;
            this.questionnaireStorage = questionnaireStorage;
        }

        public QuestionnaireExportStructure CreateQuestionnaireExportStructure(Guid id, long version)
        {
            return CreateQuestionnaireExportStructure(new QuestionnaireIdentity(id, version));
        }

        public QuestionnaireExportStructure CreateQuestionnaireExportStructure(QuestionnaireIdentity id)
        {
            var result = new QuestionnaireExportStructure();

            result.QuestionnaireId = id.QuestionnaireId;
            result.Version = id.Version;

            var rosterScopes = this.questionnaireStorage.GetQuestionnaire(id, null).GetRosterScopes();

            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(id);
            questionnaire.ConnectChildrenWithParent();

            var maxValuesForRosterSizeQuestions = GetMaxValuesForRosterSizeQuestions(questionnaire);

            result.HeaderToLevelMap.Add(new ValueVector<Guid>(),
                this.BuildHeaderByTemplate(questionnaire, new ValueVector<Guid>(), rosterScopes, maxValuesForRosterSizeQuestions));

            foreach (var rosterScopeDescription in rosterScopes)
            {
                result.HeaderToLevelMap.Add(rosterScopeDescription.Key,
                    this.BuildHeaderByTemplate(questionnaire, rosterScopeDescription.Key, rosterScopes, maxValuesForRosterSizeQuestions));
            }

            return result;
        }

        public InterviewDataExportView CreateInterviewDataExportView(QuestionnaireExportStructure exportStructure, InterviewData interview)
        {
            var interviewDataExportLevelViews = exportStructure.HeaderToLevelMap.Values.Select(
                exportStructureForLevel =>
                    new InterviewDataExportLevelView(exportStructureForLevel.LevelScopeVector,
                        exportStructureForLevel.LevelName,
                        this.BuildRecordsForHeader(interview, exportStructureForLevel))).ToArray();

            return new InterviewDataExportView(interview.InterviewId, interviewDataExportLevelViews);
        }

        private InterviewDataExportRecord[] BuildRecordsForHeader(InterviewData interview, HeaderStructureForLevel headerStructureForLevel)
        {
            var dataRecords = new List<InterviewDataExportRecord>();

            var answersSeparator = ExportFileSettings.NotReadableAnswersSeparator.ToString();
            var interviewDataByLevels = this.GetLevelsFromInterview(interview, headerStructureForLevel.LevelScopeVector);

            foreach (InterviewLevel dataByLevel in interviewDataByLevels)
            {
                var vectorLength = dataByLevel.RosterVector.Length;

                string recordId = vectorLength == 0
                    ? interview.InterviewId.FormatGuid()
                    : dataByLevel.RosterVector.Last().ToString(CultureInfo.InvariantCulture);

                string[] systemVariableValues = new string[0];
                if (vectorLength == 0)
                    systemVariableValues = this.GetSystemValues(interview, ServiceColumns.SystemVariables);

                string[] parentRecordIds = new string[dataByLevel.RosterVector.Length];
                if (parentRecordIds.Length > 0)
                {
                    parentRecordIds[0] = interview.InterviewId.FormatGuid();
                    for (int i = 0; i < dataByLevel.RosterVector.Length - 1; i++)
                    {
                        parentRecordIds[i + 1] = dataByLevel.RosterVector[i].ToString(CultureInfo.InvariantCulture);
                    }

                    parentRecordIds = parentRecordIds.Reverse().ToArray();
                }

                string[] referenceValues = new string[0];

                if (headerStructureForLevel.IsTextListScope)
                {
                    referenceValues = new string[]
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
                    Answers = questionsForExport.Select(x => string.Join(answersSeparator,x.Select(s => s.Replace(answersSeparator, "")))).ToArray()
                });
            }

            return dataRecords.ToArray();
        }

        private string CreateLevelIdFromPropagationVector(decimal[] vector)
        {
            return vector.Length == 0 ? "#" : vector.CreateLeveKeyFromPropagationVector();
        }

        private string[] GetSystemValues(InterviewData interview, ServiceVariable[] variables)
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
                case ServiceVariableType.InterviewRandom :
                        return interview.InterviewId.GetRandomDouble().ToString(CultureInfo.InvariantCulture);
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

                var interviewTextListAnswer = questionToCheck.Answer as InterviewTextListAnswers;

                if (interviewTextListAnswer == null)
                    return string.Empty;
                var item = interviewTextListAnswer.Answers.SingleOrDefault(a => a.Value == itemToSearch);

                return item != null ? item.Answer : string.Empty;
            }

            return string.Empty;
        }

        private string[][] GetExportValues(InterviewLevel interviewLevel, HeaderStructureForLevel headerStructureForLevel)
        {
            var result = new List<string[]>();
            foreach (var headerItem in headerStructureForLevel.HeaderItems.Values)
            {
                var question = interviewLevel.QuestionsSearchCache.ContainsKey(headerItem.PublicKey) ? interviewLevel.QuestionsSearchCache[headerItem.PublicKey] : null;
                var exportedQuestion = exportQuestionService.GetExportedQuestion(question, headerItem);
                result.Add(exportedQuestion);
            }
            return result.ToArray();
        }

        private IEnumerable<InterviewLevel> GetLevelsFromInterview(InterviewData interview, ValueVector<Guid> levelVector)
        {
            if (!levelVector.Any())
                return interview.Levels.Values.Where(level => level.ScopeVectors.ContainsKey(new ValueVector<Guid>()));
            return interview.Levels.Values.Where(level => level.ScopeVectors.ContainsKey(levelVector));
        }

        protected HeaderStructureForLevel CreateHeaderStructureForLevel(
            string levelTitle,
            IEnumerable<IGroup> groupsInLevel,
            QuestionnaireDocument questionnaire,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions,
            ValueVector<Guid> levelVector)
        {
            var headerStructureForLevel = new HeaderStructureForLevel();
            headerStructureForLevel.LevelScopeVector = levelVector;
            headerStructureForLevel.LevelIdColumnName = ServiceColumns.Id;

            headerStructureForLevel.LevelName = levelTitle;

            foreach (var rootGroup in groupsInLevel)
            {
                this.FillHeaderWithQuestionsInsideGroup(headerStructureForLevel, rootGroup, questionnaire,
                    maxValuesForRosterSizeQuestions);
            }

            return headerStructureForLevel;
        }

        protected ExportedHeaderItem CreateExportedHeaderItem(IQuestion question, int? lengthOfRosterVectorWhichNeedToBeExported)
        {
            var exportedHeaderItem = new ExportedHeaderItem();

            exportedHeaderItem.PublicKey = question.PublicKey;
            exportedHeaderItem.QuestionType = question.QuestionType;
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

            var dateTimeQuestion = question as DateTimeQuestion;
            if (dateTimeQuestion != null)
            {
                if (dateTimeQuestion.IsTimestamp)
                    exportedHeaderItem.QuestionSubType = QuestionSubtype.DateTime_Timestamp;
            }

            exportedHeaderItem.VariableName = question.StataExportCaption;
            exportedHeaderItem.Titles = new[]
            { string.IsNullOrEmpty(question.VariableLabel) ? question.QuestionText : question.VariableLabel };
            exportedHeaderItem.ColumnNames = new string[] { question.StataExportCaption };

            exportedHeaderItem.Labels = new Dictionary<Guid, LabelItem>();
            if (question.Answers != null)
            {
                foreach (var answer in question.Answers)
                {
                    exportedHeaderItem.Labels.Add(answer.PublicKey, new LabelItem(answer));
                }
            }

            if (lengthOfRosterVectorWhichNeedToBeExported.HasValue)
            {
                exportedHeaderItem.LengthOfRosterVectorWhichNeedToBeExported = lengthOfRosterVectorWhichNeedToBeExported;
            }

            return exportedHeaderItem;
        }

        protected ExportedHeaderItem CreateExportedHeaderItem(IQuestion question, int columnCount,
            int? lengthOfRosterVectorWhichNeedToBeExported)
        {
            var exportedHeaderItem = this.CreateExportedHeaderItem(question, lengthOfRosterVectorWhichNeedToBeExported);
            this.ThrowIfQuestionIsNotMultiSelectOrTextList(question);

            exportedHeaderItem.ColumnNames = new string[columnCount];
            exportedHeaderItem.ColumnValues = new decimal[columnCount];
            exportedHeaderItem.Titles = new string[columnCount];

            for (int i = 0; i < columnCount; i++)
            {
                if (!IsQuestionLinked(question) && question is IMultyOptionsQuestion)
                {
                    var columnValue = decimal.Parse(question.Answers[i].AnswerValue);

                    exportedHeaderItem.ColumnNames[i] = string.Format(GeneratedTitleExportFormat, 
                        question.StataExportCaption, DecimalToHeaderConverter.ToHeader(columnValue)); 
                        
                    exportedHeaderItem.ColumnValues[i] = columnValue;
                }
                else
                {
                    exportedHeaderItem.ColumnNames[i] = string.Format(GeneratedTitleExportFormat, question.StataExportCaption, i);
                }

                if (!IsQuestionLinked(question))
                {
                    var questionLabel =
                        string.IsNullOrEmpty(question.VariableLabel) ? question.QuestionText : question.VariableLabel;
                    if (question is IMultyOptionsQuestion)
                        exportedHeaderItem.Titles[i] += $"{questionLabel}:{question.Answers[i].AnswerText}";
                    if (question is ITextListQuestion)
                        exportedHeaderItem.Titles[i] += $"{questionLabel}:{i}";
                }
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

        private HeaderStructureForLevel BuildHeaderByTemplate(QuestionnaireDocument questionnaire, ValueVector<Guid> levelVector,
            Dictionary<ValueVector<Guid>, RosterScopeDescription> rosterScopes, Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            var rootGroups = this.GetRootGroupsForLevel(questionnaire, rosterScopes, levelVector);

            if (!rootGroups.Any())
                throw new InvalidOperationException("level is absent in template");

            var firstRootGroup = rootGroups.First();
            var levelTitle = firstRootGroup.VariableName ?? this.fileSystemAccessor.MakeStataCompatibleFileName(firstRootGroup.Title);

            var structures = this.CreateHeaderStructureForLevel(levelTitle, rootGroups, questionnaire,
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

        private void FillHeaderWithQuestionsInsideGroup(HeaderStructureForLevel headerStructureForLevel, IGroup @group,
            QuestionnaireDocument questionnaire,
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
                var question = groupChild as IQuestion;
                if (question != null)
                {
                    if (this.IsQuestionMultiOption(question))
                    {
                        if (question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue)
                            this.AddHeadersForLinkedMultiOptions(headerStructureForLevel.HeaderItems, question, questionnaire, maxValuesForRosterSizeQuestions);

                        else this.AddHeadersForMultiOptions(headerStructureForLevel.HeaderItems, question, questionnaire);
                    }
                    else if (this.IsQuestionTextList(question))
                    {
                        this.AddHeadersForTextList(headerStructureForLevel.HeaderItems, question,
                            questionnaire);
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

                var innerGroup = groupChild as IGroup;
                if (innerGroup != null)
                {
                    if (innerGroup.IsRoster)
                        continue;
                    this.FillHeaderWithQuestionsInsideGroup(headerStructureForLevel, innerGroup, questionnaire,
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

        private void AddHeadersForLinkedMultiOptions(IDictionary<Guid, ExportedHeaderItem> headerItems, IQuestion question,
            QuestionnaireDocument questionnaire,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            headerItems.Add(question.PublicKey,
                this.CreateExportedHeaderItem(question,
                    this.GetRosterSizeForLinkedQuestion(question, questionnaire, maxValuesForRosterSizeQuestions),
                    this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire)));
        }

        protected void AddHeaderForSingleColumnExportQuestion(IDictionary<Guid, ExportedHeaderItem> headerItems, IQuestion question,
            QuestionnaireDocument questionnaire)
        {
            headerItems.Add(question.PublicKey,
                this.CreateExportedHeaderItem(question,
                    this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire)));
        }

        protected void AddHeadersForMultiOptions(IDictionary<Guid, ExportedHeaderItem> headerItems, IQuestion question,
            QuestionnaireDocument questionnaire)
        {
            headerItems.Add(question.PublicKey,
                this.CreateExportedHeaderItem(question, question.Answers.Count,
                    this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire)));
        }

        protected void AddHeadersForTextList(IDictionary<Guid, ExportedHeaderItem> headerItems, IQuestion question,
            QuestionnaireDocument questionnaire)
        {
            var textListQuestion = question as ITextListQuestion;
            var maxCount = (textListQuestion == null ? null : textListQuestion.MaxAnswerCount) ?? TextListQuestion.MaxAnswerCountLimit;
            headerItems.Add(question.PublicKey,
                this.CreateExportedHeaderItem(question, maxCount,
                    this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire)));
        }

        protected void AddHeadersForGpsQuestion(IDictionary<Guid, ExportedHeaderItem> headerItems, IQuestion question,
            QuestionnaireDocument questionnaire)
        {
            var gpsColumns = GeoPosition.PropertyNames;
            var gpsQuestionExportHeader = this.CreateExportedHeaderItem(question,
                this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire));
            gpsQuestionExportHeader.ColumnNames = new string[gpsColumns.Length];
            gpsQuestionExportHeader.Titles = new string[gpsColumns.Length];

            var questionLabel = string.IsNullOrEmpty(question.VariableLabel)
                ? question.QuestionText
                : question.VariableLabel;
            for (int i = 0; i < gpsColumns.Length; i++)
            {
                gpsQuestionExportHeader.ColumnNames[i] = string.Format(GeneratedTitleExportFormat, question.StataExportCaption,
                    gpsColumns[i]);

                gpsQuestionExportHeader.Titles[i] += $"{questionLabel}: {gpsColumns[i]}";
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

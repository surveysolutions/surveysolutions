using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Questionnaire
{
    public class QuestionnaireExportStructureFactory : IQuestionnaireExportStructureFactory
    {
        private const string GeneratedTitleExportFormat = "{0}__{1}";
        private readonly IQuestionnaireStorage questionnaireStorage;

        public QuestionnaireExportStructureFactory(IQuestionnaireStorage questionnaireStorage)
        {
            this.questionnaireStorage = questionnaireStorage;
        }

        public async Task<QuestionnaireExportStructure> GetQuestionnaireExportStructureAsync(TenantInfo tenant,
            QuestionnaireId questionnaireId,
            Guid? translation)
        {
            var questionnaire = await this.questionnaireStorage.GetQuestionnaireAsync(questionnaireId, translation);
            if (questionnaire == null) throw new InvalidOperationException("questionnaire must be not null.");
            
            return CreateQuestionnaireExportStructure(questionnaire);
        }

        public QuestionnaireExportStructure CreateQuestionnaireExportStructure(QuestionnaireDocument questionnaire)
        {
            var result = new QuestionnaireExportStructure
            (
                questionnaireId : questionnaire.Id
            );

            var rosterScopes = this.GetRosterScopes(questionnaire);
            var maxValuesForRosterSizeQuestions = GetMaxValuesForRosterSizeQuestions(questionnaire);

            result.HeaderToLevelMap.Add(new ValueVector<Guid>(),
                this.BuildHeaderByTemplate(questionnaire, true, new ValueVector<Guid>(), rosterScopes, maxValuesForRosterSizeQuestions));

            foreach (var rosterScopeDescription in rosterScopes)
            {
                result.HeaderToLevelMap.Add(rosterScopeDescription.Key,
                    this.BuildHeaderByTemplate(questionnaire, true, rosterScopeDescription.Key, rosterScopes, maxValuesForRosterSizeQuestions));
            }

            return result;
        }

        Dictionary<ValueVector<Guid>, RosterScopeDescription> GetRosterScopes(QuestionnaireDocument document)
        {
            var rosterScopesFiller = new RosterScopesFiller(document);
            
            return rosterScopesFiller.FillRosterScopes();
        }

        private HeaderStructureForLevel CreateHeaderStructureForLevel(
            string levelTitle,
            IEnumerable<Group> groupsInLevel,
            QuestionnaireDocument questionnaire,
            bool supportVariables,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions,
            ValueVector<Guid> levelVector)
        {
            var headerStructureForLevel = new HeaderStructureForLevel(
                levelScopeVector : levelVector,
                levelIdColumnName: (levelVector == null || levelVector.Length == 0) ? ServiceColumns.InterviewId : string.Format(ServiceColumns.IdSuffixFormat, levelTitle),
                levelName: levelTitle);

            foreach (var rootGroup in groupsInLevel)
            {
                this.FillHeaderWithQuestionsInsideGroup(headerStructureForLevel, rootGroup, questionnaire, supportVariables,
                    maxValuesForRosterSizeQuestions);
            }

            return headerStructureForLevel;
        }

        private ExportedVariableHeaderItem CreateExportedVariableHeaderItem(Variable variable)
        {
            var exportedHeaderItem = new ExportedVariableHeaderItem();

            exportedHeaderItem.PublicKey = variable.PublicKey;
            exportedHeaderItem.VariableType = variable.Type;
            exportedHeaderItem.VariableName = variable.Name;

            exportedHeaderItem.ColumnHeaders = new List<HeaderColumn>(){
                new HeaderColumn()
                {
                    Name = variable.Name,
                    Title = GetVariableLabel(variable),
                    ExportType = GetStorageType(variable)
                }};

            return exportedHeaderItem;
        }

        private string GetVariableLabel(Variable variable)
        {
            return !string.IsNullOrEmpty(variable.Label) ? variable.Label : $"Calculated variable of type {Enum.GetName(typeof(VariableType), variable.Type)}";
        }

        private ExportValueType GetStorageType(Variable variable)
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

        private ExportedQuestionHeaderItem CreateExportedQuestionHeaderItem(Question question, QuestionnaireDocument questionnaire,
            HeaderStructureForLevel headerStructureForLevel, int? lengthOfRosterVectorWhichNeedToBeExported)
        {
            var exportedHeaderItem = new ExportedQuestionHeaderItem();

            exportedHeaderItem.PublicKey = question.PublicKey;
            exportedHeaderItem.QuestionType = question.QuestionType;
            exportedHeaderItem.IsIdentifyingQuestion = question.Featured;

            if (question.QuestionType == QuestionType.MultyOption)
            {
                var multiOptionQuestion = (MultyOptionsQuestion)question;
                if (multiOptionQuestion.LinkedToQuestionId.HasValue)
                {
                    var sourceQuestion = questionnaire.Find<Question>(multiOptionQuestion.LinkedToQuestionId.Value);
                    if(sourceQuestion == null)
                        throw new InvalidOperationException($"Source question {multiOptionQuestion.LinkedToQuestionId.Value} was not found.");

                    exportedHeaderItem.QuestionSubType = this.GetRosterSizeSourcesForEntity(sourceQuestion).Length > 1
                        ? QuestionSubtype.MultiOptionLinkedNestedLevel
                        : QuestionSubtype.MultiOptionLinkedFirstLevel;
                }
                else if (multiOptionQuestion.LinkedToRosterId.HasValue)
                {
                    var sourceRoster = questionnaire.Find<Group>(multiOptionQuestion.LinkedToRosterId.Value);
                    if (sourceRoster == null)
                        throw new InvalidOperationException($"Source roster {multiOptionQuestion.LinkedToRosterId.Value} was not found.");
                    exportedHeaderItem.QuestionSubType = this.GetRosterSizeSourcesForEntity(sourceRoster).Length > 1
                        ? QuestionSubtype.MultiOptionLinkedNestedLevel
                        : QuestionSubtype.MultiOptionLinkedFirstLevel;
                }
                else if (multiOptionQuestion.YesNoView)
                {
                    exportedHeaderItem.QuestionSubType = multiOptionQuestion.AreAnswersOrdered
                        ? QuestionSubtype.MultiOptionYesNoOrdered
                        : QuestionSubtype.MultiOptionYesNo;
                }
                else if (multiOptionQuestion.AreAnswersOrdered)
                {
                    exportedHeaderItem.QuestionSubType = QuestionSubtype.MultiOptionOrdered;
                }
                else if (multiOptionQuestion.IsFilteredCombobox ?? false)
                {
                    exportedHeaderItem.QuestionSubType = QuestionSubtype.MultyOption_Combobox;
                }
            }

            if (question is DateTimeQuestion dateTimeQuestion)
            {
                if (dateTimeQuestion.IsTimestamp)
                    exportedHeaderItem.QuestionSubType = QuestionSubtype.DateTimeTimestamp;
            }

            if (question is SingleQuestion singleQuestion)
            {
                if (singleQuestion.LinkedToQuestionId.HasValue)
                {
                    var sourceQuestion = questionnaire.Find<Question>(singleQuestion.LinkedToQuestionId.Value);
                    if(sourceQuestion == null)
                        throw new InvalidOperationException($"Source question {singleQuestion.LinkedToQuestionId.Value} was not found.");

                    exportedHeaderItem.QuestionSubType = this.GetRosterSizeSourcesForEntity(sourceQuestion).Length > 1
                            ? QuestionSubtype.SingleOptionLinkedNestedLevel
                            : QuestionSubtype.SingleOptionLinkedFirstLevel;
                    
                }
                else if (singleQuestion.LinkedToRosterId.HasValue)
                {
                    var sourceRoster = questionnaire.Find<Group>(singleQuestion.LinkedToRosterId.Value);
                    if (sourceRoster == null)
                        throw new InvalidOperationException($"Source roster {singleQuestion.LinkedToRosterId.Value} was not found.");
                    exportedHeaderItem.QuestionSubType = this.GetRosterSizeSourcesForEntity(sourceRoster).Length > 1
                        ? QuestionSubtype.SingleOptionLinkedNestedLevel
                        : QuestionSubtype.SingleOptionLinkedFirstLevel;
                }
            }

            if (question is NumericQuestion numericQuestion)
            {
                if (numericQuestion.IsInteger)
                    exportedHeaderItem.QuestionSubType = QuestionSubtype.NumericInteger;
            }

            exportedHeaderItem.VariableName = question.VariableName;

            exportedHeaderItem.ColumnHeaders = new List<HeaderColumn>()
            {
                new HeaderColumn
                {
                    Name = question.VariableName,
                    Title = string.IsNullOrEmpty(question.VariableLabel) ? question.QuestionText.RemoveHtmlTags() : question.VariableLabel,
                    ExportType = GetStorageType(question, exportedHeaderItem.QuestionSubType)
                }
            };

            exportedHeaderItem.Labels = new List<LabelItem>();
            if (question is ICategoricalQuestion categoricalQuestion && categoricalQuestion.CategoriesId.HasValue)
            {
                var categories = questionnaire.Categories.FirstOrDefault(c => c.Id == categoricalQuestion.CategoriesId.Value);
                var isMultiWithoutPredefineCategories = question is MultyOptionsQuestion multyOptionsQuestion
                                                        && multyOptionsQuestion.IsFilteredCombobox != true;

                if (categories != null && !isMultiWithoutPredefineCategories)
                {
                    exportedHeaderItem.LabelReferenceId = categoricalQuestion.CategoriesId;

                    if (!headerStructureForLevel.ReusableLabels.ContainsKey(categoricalQuestion.CategoriesId.Value))
                    {
                        headerStructureForLevel.ReusableLabels[categoricalQuestion.CategoriesId.Value] =
                            new ReusableLabels
                            (
                                name : categories.Name,
                                labels : categories.Values.Select(o => new LabelItem(o.Id.ToString(), o.Text)).ToArray()
                            );
                    }
                }
            }
            else if (question.Answers != null)
            {
                var isMultiFilteredCombo = question is MultyOptionsQuestion multyOptionsQuestion && multyOptionsQuestion.IsFilteredCombobox == true;

                if (isMultiFilteredCombo)
                {
                    exportedHeaderItem.LabelReferenceId = question.PublicKey;

                    headerStructureForLevel.ReusableLabels[question.PublicKey] =
                        new ReusableLabels
                        (
                            name : question.VariableName,
                            labels : question.Answers.Select(a => new LabelItem(a.AnswerValue, a.AnswerText)).ToArray()
                        );
                }
                else
                {
                    foreach (var answer in question.Answers)
                    {
                        exportedHeaderItem.Labels.Add(new LabelItem(answer));
                    }
                }
            }

            if (lengthOfRosterVectorWhichNeedToBeExported.HasValue)
            {
                exportedHeaderItem.LengthOfRosterVectorWhichNeedToBeExported = lengthOfRosterVectorWhichNeedToBeExported;
            }

            return exportedHeaderItem;
        }

        private ExportValueType GetStorageType(Question question, QuestionSubtype? questionSubType)
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
                    return questionSubType == QuestionSubtype.NumericInteger ? ExportValueType.NumericInt : ExportValueType.Numeric;
                case QuestionType.DateTime:
                {
                    return (questionSubType != null && questionSubType == QuestionSubtype.DateTimeTimestamp) 
                        ? ExportValueType.DateTime 
                        : ExportValueType.Date;
                }
                case QuestionType.MultyOption:
                {
                    return questionSubType == QuestionSubtype.MultiOptionLinkedNestedLevel 
                        ? ExportValueType.String 
                        : ExportValueType.NumericInt;
                }
                case QuestionType.SingleOption:
                {
                    return questionSubType == QuestionSubtype.SingleOptionLinkedNestedLevel 
                        ? ExportValueType.String 
                        : ExportValueType.NumericInt;
                }
                default:
                    return ExportValueType.Unknown;
            }
        }

        private ExportedQuestionHeaderItem CreateExportedQuestionHeaderForMultiColumnItem(Question question, int columnCount,
            QuestionnaireDocument questionnaire,
            int? lengthOfRosterVectorWhichNeedToBeExported,
            HeaderStructureForLevel headerStructureForLevel)
        {
            var isQuestionLinked = IsQuestionLinked(question);
            var asCategorical = question as MultyOptionsQuestion;
            var isMultiCombobox = asCategorical?.IsFilteredCombobox ?? false;

            var exportedHeaderItem = this.CreateExportedQuestionHeaderItem(question, questionnaire, headerStructureForLevel, lengthOfRosterVectorWhichNeedToBeExported);
            this.ThrowIfQuestionIsNotMultiSelectOrTextList(question);

            exportedHeaderItem.ColumnValues = new int[columnCount];
            exportedHeaderItem.ColumnHeaders = new List<HeaderColumn>();

            for (int i = 0; i < columnCount; i++)
            {
                HeaderColumn headerColumn = new HeaderColumn();

                
                if (isQuestionLinked || isMultiCombobox || asCategorical == null)
                {
                    headerColumn.Name = string.Format(GeneratedTitleExportFormat, question.VariableName, i);
                }
                else
                {
                    var columnValue = asCategorical.CategoriesId.HasValue 
                        ? questionnaire.Categories.First(c => c.Id == asCategorical.CategoriesId.Value).Values[i].Id
                        : int.Parse(question.Answers[i].AnswerValue);

                    headerColumn.Name = string.Format(GeneratedTitleExportFormat,
                        question.VariableName, DecimalToHeaderConverter.ToHeader(columnValue));

                    exportedHeaderItem.ColumnValues[i] = columnValue;
                }

                if (!isQuestionLinked)
                {
                    var questionLabel = string.IsNullOrEmpty(question.VariableLabel) ? question.QuestionText : question.VariableLabel;

                    if (question.QuestionType == QuestionType.MultyOption)
                    {
                        var optionText = asCategorical?.IsFilteredCombobox ?? false
                            ? i.ToString()
                            : asCategorical?.CategoriesId.HasValue ?? false
                                ? questionnaire.Categories.First(c => c.Id == asCategorical!.CategoriesId!.Value).Values[i].Text
                                : question.Answers[i].AnswerText;

                        headerColumn.Title = $"{questionLabel}:{optionText}";
                    }
                    if (question.QuestionType == QuestionType.TextList)
                    {
                        headerColumn.Title = $"{questionLabel}:{i}";
                    }
                }

                headerColumn.ExportType = GetStorageType(question, exportedHeaderItem.QuestionSubType);
                exportedHeaderItem.ColumnHeaders.Add(headerColumn);
            }
            return exportedHeaderItem;
        }

        private static bool IsQuestionLinked(Question question)
        {
            return question.IsQuestionLinked();
        }

        private void ThrowIfQuestionIsNotMultiSelectOrTextList(Question question)
        {
            if (question.QuestionType != QuestionType.MultyOption && question.QuestionType != QuestionType.TextList)
                throw new InvalidOperationException(string.Format(
                    "question '{1}' with type '{0}' can't be exported as more then one column",
                    question.QuestionType, question.QuestionText));
        }

        private static Dictionary<Guid, int> GetMaxValuesForRosterSizeQuestions(QuestionnaireDocument document)
        {
            var rosterGroups = document.Find<Group>(@group => group.IsRoster && @group.RosterSizeQuestionId.HasValue).ToList();

            var fixedRosterGroups =
                document.Find<Group>(@group => @group.IsRoster && group.IsFixedRoster);

            IEnumerable<MultyOptionsQuestion?> rosterSizeMultyOptionQuestions =
                rosterGroups
                    .Select(@group => document.Find<MultyOptionsQuestion>(@group.RosterSizeQuestionId!.Value))
                    .Where(question => question != null)
                    .Distinct();

            IEnumerable<TextListQuestion?> rosterSizeTextListQuestions =
                rosterGroups
                    .Select(@group => document.Find<TextListQuestion>(@group.RosterSizeQuestionId!.Value))
                    .Where(question => question != null)
                    .Distinct();

            var collectedMaxValues = new Dictionary<Guid, int>();

            foreach (MultyOptionsQuestion? rosterSizeMultyOptionQuestion in rosterSizeMultyOptionQuestions)
            {
                if (rosterSizeMultyOptionQuestion != null)
                {
                    if (rosterSizeMultyOptionQuestion.CategoriesId.HasValue)
                    {
                        collectedMaxValues.Add(rosterSizeMultyOptionQuestion.PublicKey,
                            document.Categories.First(c => c.Id == rosterSizeMultyOptionQuestion.CategoriesId.Value)
                                .Values.Length);
                    }
                    else
                        collectedMaxValues.Add(rosterSizeMultyOptionQuestion.PublicKey, rosterSizeMultyOptionQuestion.Answers.Count);
                }
            }

            foreach (TextListQuestion? rosterSizeTextListQuestion in rosterSizeTextListQuestions)
            {
                if(rosterSizeTextListQuestion != null)
                    collectedMaxValues.Add(rosterSizeTextListQuestion.PublicKey,
                        rosterSizeTextListQuestion.MaxAnswerCount ?? Constants.MaxLongRosterRowCount);
            }

            foreach (Group fixedRosterGroup in fixedRosterGroups)
            {
                collectedMaxValues.Add(fixedRosterGroup.PublicKey, fixedRosterGroup.FixedRosterTitles.Length);
            }

            return collectedMaxValues;
        }

        private HeaderStructureForLevel BuildHeaderByTemplate(QuestionnaireDocument questionnaire, bool supportVariables, ValueVector<Guid> levelVector,
            Dictionary<ValueVector<Guid>, RosterScopeDescription> rosterScopes, Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            var rootGroups = this.GetRootGroupsForLevel(questionnaire, rosterScopes, levelVector).ToList();

            if (!rootGroups.Any())
                throw new InvalidOperationException("level is absent in template");

            var firstRootGroup = rootGroups.First();

            var levelTitle = firstRootGroup.VariableName ?? firstRootGroup.Title.MakeStataCompatibleFileName();

            var structures = this.CreateHeaderStructureForLevel(levelTitle, rootGroups, questionnaire, supportVariables,
                maxValuesForRosterSizeQuestions, levelVector);

            if (rosterScopes.ContainsKey(levelVector) &&
                rosterScopes[levelVector].Type == RosterScopeType.TextList)
            {
                structures.IsTextListScope = true;
                structures.ReferencedNames = new[] { rosterScopes[levelVector].SizeQuestionTitle };
            }

            return structures;
        }

        private IEnumerable<Group> GetRootGroupsForLevel(QuestionnaireDocument questionnaire,
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
                yield return questionnaire.FirstOrDefault<Group>(group => group.PublicKey == rootGroup);
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
            Group @group,
            QuestionnaireDocument questionnaire,
            bool supportVariables,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            if (@group.IsFixedRoster && headerStructureForLevel.LevelLabels == null)
            {
                headerStructureForLevel.LevelLabels =
                    @group.FixedRosterTitles.Select(title => new LabelItem(title.Value.ToString(CultureInfo.InvariantCulture), title.Title))
                        .ToArray();
            }
            else if (@group.IsRoster && headerStructureForLevel.LevelLabels == null)
            {
                var trigger = questionnaire.FirstOrDefault<Question>(x => x.PublicKey == @group.RosterSizeQuestionId);
                if (trigger.QuestionType == QuestionType.MultyOption)
                {
                    headerStructureForLevel.LevelLabels =
                        trigger.Answers.Select(title => new LabelItem(title.AnswerValue, title.AnswerText))
                            .ToArray();
                }
            }

            foreach (var groupChild in @group.Children)
            {
                if (groupChild.IsExportable == false) continue;

                if (groupChild is Question question)
                {
                    if (this.IsQuestionMultiOption(question))
                    {
                        if (question.LinkedToRosterId.HasValue)
                        {
                            this.AddHeadersForLinkedMultiOptions(headerStructureForLevel, question, questionnaire, maxValuesForRosterSizeQuestions);
                        }
                        else if (question.LinkedToQuestionId.HasValue)
                        {
                            var linkToQuestion = questionnaire.FirstOrDefault<Question>(x => x.PublicKey == question.LinkedToQuestionId.Value);

                            if (linkToQuestion.QuestionType == QuestionType.TextList)
                                this.AddHeadersForLinkedToListMultiOptions(headerStructureForLevel, question, linkToQuestion, questionnaire);
                            else
                                this.AddHeadersForLinkedMultiOptions(headerStructureForLevel, question, questionnaire, maxValuesForRosterSizeQuestions);
                        }
                        else
                        {
                            this.AddHeadersForMultiOptions(headerStructureForLevel, question, questionnaire);
                        }
                    }
                    else if (this.IsQuestionTextList(question))
                    {
                        this.AddHeadersForTextList(headerStructureForLevel, question, questionnaire);
                    }
                    else if (question is GpsCoordinateQuestion)
                    {
                        this.AddHeadersForGpsQuestion(headerStructureForLevel, question, questionnaire);
                    }
                    else
                    {
                        this.AddHeaderForSingleColumnExportQuestion(headerStructureForLevel, question, questionnaire);
                    }
                    continue;
                }

                if (groupChild is Variable variable && variable.DoNotExport == false)
                {
                    if (supportVariables)
                        AddHeadersForVariable(headerStructureForLevel.HeaderItems, variable);

                    continue;
                }

                if (groupChild is Group innerGroup)
                {
                    if (innerGroup.IsRoster)
                        continue;
                    this.FillHeaderWithQuestionsInsideGroup(headerStructureForLevel, innerGroup, questionnaire, supportVariables,
                        maxValuesForRosterSizeQuestions);
                }
            }
        }

        private bool IsQuestionMultiOption(Question question)
        {
            return question.QuestionType == QuestionType.MultyOption;
        }

        private bool IsQuestionTextList(Question question)
        {
            return question.QuestionType == QuestionType.TextList;
        }

        private void AddHeadersForVariable(IDictionary<Guid, IExportedHeaderItem> headerItems, Variable variable)
        {
            headerItems.Add(variable.PublicKey, this.CreateExportedVariableHeaderItem(variable));
        }

        private void AddHeadersForLinkedMultiOptions(HeaderStructureForLevel headerStructureForLevel, Question question,
            QuestionnaireDocument questionnaire,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            headerStructureForLevel.HeaderItems.Add(question.PublicKey,
                this.CreateExportedQuestionHeaderForMultiColumnItem(question,
                    this.GetRostersSizeForLinkedQuestion(question, questionnaire, maxValuesForRosterSizeQuestions),
                    questionnaire,
                    this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire),
                    headerStructureForLevel));
        }

        private void AddHeaderForSingleColumnExportQuestion(HeaderStructureForLevel headerStructureForLevel, Question question,
            QuestionnaireDocument questionnaire)
        {
            var lengthOfRosterVectorWhichNeedToBeExported = this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire);
            var headerItem = this.CreateExportedQuestionHeaderItem(question, questionnaire, headerStructureForLevel, lengthOfRosterVectorWhichNeedToBeExported);
            headerStructureForLevel.HeaderItems.Add(question.PublicKey, headerItem);
        }

        private void AddHeadersForMultiOptions(HeaderStructureForLevel headerStructureForLevel, Question question,
            QuestionnaireDocument questionnaire)
        {
            var columnCount = GetColumnsCountForMultiOptionQuestion(question, questionnaire);

            headerStructureForLevel.HeaderItems.Add(question.PublicKey,
                this.CreateExportedQuestionHeaderForMultiColumnItem(question, columnCount,
                    questionnaire,
                    this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire),
                    headerStructureForLevel));
        }

        private int GetColumnsCountForMultiOptionQuestion(Question question, QuestionnaireDocument questionnaire)
        {
            MultyOptionsQuestion? typedQuestion = question as MultyOptionsQuestion;
            if (typedQuestion == null) return question.Answers.Count;
            
            var optionCount = typedQuestion.CategoriesId != null
                ? questionnaire.Categories.First(c => c.Id == typedQuestion.CategoriesId.Value).Values.Length
                : question.Answers.Count;

            if (typedQuestion.IsFilteredCombobox ?? false)
                return Math.Min(typedQuestion.MaxAllowedAnswers ?? Constants.MaxLongRosterRowCount, optionCount);

            return optionCount;
        }

        private void AddHeadersForTextList(HeaderStructureForLevel headerStructureForLevel, Question question,
            QuestionnaireDocument questionnaire)
        {
            var textListQuestion = question as TextListQuestion;
            var maxCount = textListQuestion?.MaxAnswerCount ?? Constants.MaxLongRosterRowCount;
            headerStructureForLevel.HeaderItems.Add(question.PublicKey,
                this.CreateExportedQuestionHeaderForMultiColumnItem(question, maxCount,
                    questionnaire,
                    this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire),
                    headerStructureForLevel));
        }

        private void AddHeadersForLinkedToListMultiOptions(HeaderStructureForLevel headerStructureForLevel,
            Question question,
            Question linkToTextListQuestion,
            QuestionnaireDocument questionnaire)
        {
            var textListQuestion = linkToTextListQuestion as TextListQuestion;
            var maxCount = textListQuestion?.MaxAnswerCount ?? Constants.MaxLongRosterRowCount;

            headerStructureForLevel.HeaderItems.Add(question.PublicKey,
                 this.CreateExportedQuestionHeaderForMultiColumnItem(question, maxCount,
                     questionnaire,
                     this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire),
                     headerStructureForLevel));
        }

        private void AddHeadersForGpsQuestion(HeaderStructureForLevel headerStructureForLevel, Question question,
            QuestionnaireDocument questionnaire)
        {
            var gpsColumns = GeoPosition.PropertyNames;
            var gpsQuestionExportHeader = this.CreateExportedQuestionHeaderItem(question,
                questionnaire,
                headerStructureForLevel,
                this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire));

            gpsQuestionExportHeader.ColumnHeaders = new List<HeaderColumn>();

            var questionLabel = string.IsNullOrEmpty(question.VariableLabel)
                ? question.QuestionText
                : question.VariableLabel;

            foreach (var column in gpsColumns)
            {
                gpsQuestionExportHeader.ColumnHeaders.Add(new HeaderColumn()
                {
                    Name = string.Format(GeneratedTitleExportFormat, question.VariableName, column),
                    Title = $"{questionLabel}: {column}",
                    ExportType = string.Compare(column, "timestamp", StringComparison.OrdinalIgnoreCase) == 0 ? ExportValueType.DateTime : ExportValueType.Numeric
                });
            }

            headerStructureForLevel.HeaderItems.Add(question.PublicKey, gpsQuestionExportHeader);
        }

        private int GetRostersSizeForLinkedQuestion(Question question, QuestionnaireDocument questionnaire,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            var referencedByLinkedQuestionEntity = GetReferencedByLinkedQuestionEntity(question, questionnaire);
            if(referencedByLinkedQuestionEntity == null) throw new InvalidOperationException("Referenced entity was not found.");
            var rosterVectorReferencedQuestion = this.GetRosterSizeSourcesForEntity(referencedByLinkedQuestionEntity);
            var rosterVectorLinkedQuestion = this.GetRosterSizeSourcesForEntity(question);
            var linkedVectorScope = FindLinkedVectorScope(rosterVectorLinkedQuestion, rosterVectorReferencedQuestion);

            var sizes = linkedVectorScope.Select(vectorValue => maxValuesForRosterSizeQuestions.ContainsKey(vectorValue)
                                                                    ? maxValuesForRosterSizeQuestions[vectorValue]
                                                                    : Constants.MaxRosterRowCount);
            var size = sizes.Aggregate(1, (a, b) => a * b);

            return Math.Min(size, Constants.MaxSizeOfLinkedQuestion);
        }

        private ValueVector<Guid> FindLinkedVectorScope(ValueVector<Guid> rosterVectorLinkedQuestion, ValueVector<Guid> rosterVectorReferencedQuestion)
        {
            if (rosterVectorLinkedQuestion.Length == 0)
                return rosterVectorReferencedQuestion;

            if (rosterVectorLinkedQuestion.Equals(rosterVectorReferencedQuestion))
                return new ValueVector<Guid>(rosterVectorLinkedQuestion.Skip(rosterVectorLinkedQuestion.Length - 1));

            int? commonIndex = null;

            for (int i = 0; i < rosterVectorReferencedQuestion.Count && i < rosterVectorLinkedQuestion.Count; i++)
            {
                if (rosterVectorReferencedQuestion[i] == rosterVectorLinkedQuestion[i])
                {
                    commonIndex = i;
                    continue;
                }

                break;
            }

            if (!commonIndex.HasValue)
                return rosterVectorReferencedQuestion;

            if (rosterVectorReferencedQuestion.Length == commonIndex.Value + 1)
                return rosterVectorReferencedQuestion;

            return new ValueVector<Guid>(rosterVectorReferencedQuestion.Skip(commonIndex.Value + 1));
        }

        private IQuestionnaireEntity? GetReferencedByLinkedQuestionEntity(Question question, QuestionnaireDocument questionnaire)
        {
            if (question.LinkedToQuestionId.HasValue)
            {
                return questionnaire.Find<Question>(question.LinkedToQuestionId.Value);
            }

            if (question.LinkedToRosterId.HasValue)
            {
                return questionnaire.Find<Group>(question.LinkedToRosterId.Value);
            }
            return null;
        }

        private ValueVector<Guid> GetRosterSizeSourcesForEntity(IQuestionnaireEntity entityFor)
        {
            var rosterSizes = new List<Guid>();
            IQuestionnaireEntity? entity = entityFor;
            while (!(entity is QuestionnaireDocument))
            {
                if (entity is Group group)
                {
                    if (group.IsRoster)
                        rosterSizes.Add(group.RosterSizeQuestionId ?? group.PublicKey);
                }

                entity = entity?.GetParent();
            }

            rosterSizes.Reverse();

            return new ValueVector<Guid>(rosterSizes);
        }

        private int? GetLengthOfRosterVectorWhichNeedToBeExported(Question question, QuestionnaireDocument questionnaire)
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


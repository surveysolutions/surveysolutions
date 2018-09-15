using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Export.Tenant;
using WB.Services.Export.Utils;

namespace WB.Services.Export.Questionnaire
{
    internal class QuestionnaireExportStructureFactory: IQuestionnaireExportStructureFactory
    {
        private readonly ICache cache;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private const string GeneratedTitleExportFormat = "{0}__{1}";

        public QuestionnaireExportStructureFactory(ICache cache, IQuestionnaireStorage questionnaireStorage)
        {
            this.cache = cache;
            this.questionnaireStorage = questionnaireStorage;
        }

        public async Task<QuestionnaireExportStructure> GetQuestionnaireExportStructure(QuestionnaireId questionnaireId,
            TenantInfo tenant)
        {
            var cachedQuestionnaireExportStructure = this.cache.Get(questionnaireId, tenant.Id);
            if (cachedQuestionnaireExportStructure == null)
            {
                var questionnaire = await this.questionnaireStorage.GetQuestionnaireAsync(tenant, questionnaireId);

                cachedQuestionnaireExportStructure = CreateQuestionnaireExportStructure(questionnaire);

                if (cachedQuestionnaireExportStructure == null)
                    return null;

                this.cache.Set(questionnaire.Id, cachedQuestionnaireExportStructure, tenant.Id);
            }

            return (QuestionnaireExportStructure) cachedQuestionnaireExportStructure;
        }

        private QuestionnaireExportStructure CreateQuestionnaireExportStructure(QuestionnaireDocument questionnaire)
        {
            var result = new QuestionnaireExportStructure
            {
                QuestionnaireId = questionnaire.Id
            };

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
            rosterScopesFiller.FillRosterScopes();
            return rosterScopesFiller.Result;
        }

        private HeaderStructureForLevel CreateHeaderStructureForLevel(
            string levelTitle,
            IEnumerable<Group> groupsInLevel,
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

        protected ExportedVariableHeaderItem CreateExportedVariableHeaderItem(Variable variable)
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
                    ExportType = GetStorageType(variable) 
                }};

            return exportedHeaderItem;
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

        protected ExportedQuestionHeaderItem CreateExportedQuestionHeaderItem(Question question, int? lengthOfRosterVectorWhichNeedToBeExported)
        {
            var exportedHeaderItem = new ExportedQuestionHeaderItem();

            exportedHeaderItem.PublicKey = question.PublicKey;
            exportedHeaderItem.QuestionType = question.QuestionType;
            exportedHeaderItem.IsIdentifyingQuestion = question.Featured;

            if (question.QuestionType == QuestionType.MultyOption)
            {
                var multioptionQuestion = (MultyOptionsQuestion) question;
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

        private ExportValueType GetStorageType(Question question, QuestionSubtype? questionSubType = null)
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
                    bool isLinked = questionSubType == QuestionSubtype.MultyOption_Linked ||
                                    questionSubType == QuestionSubtype.SingleOption_Linked;
                    return isLinked ? ExportValueType.String : ExportValueType.NumericInt;
                }
                default:
                        return ExportValueType.Unknown;
            }
        }

        private ExportedQuestionHeaderItem CreateExportedQuestionHeaderForMultiColumnItem(Question question, int columnCount,
            int? lengthOfRosterVectorWhichNeedToBeExported)
        {
            var exportedHeaderItem = this.CreateExportedQuestionHeaderItem(question, lengthOfRosterVectorWhichNeedToBeExported);
            this.ThrowIfQuestionIsNotMultiSelectOrTextList(question);

            exportedHeaderItem.ColumnValues = new int[columnCount];
            exportedHeaderItem.ColumnHeaders = new List<HeaderColumn>();

            for (int i = 0; i < columnCount; i++)
            {
                HeaderColumn headerColumn = new HeaderColumn();

                if (!IsQuestionLinked(question) && question is MultyOptionsQuestion)
                {
                    var columnValue = int.Parse(question.Answers[i].AnswerValue);
              
                    headerColumn.Name = string.Format(GeneratedTitleExportFormat,
                        question.VariableName, DecimalToHeaderConverter.ToHeader(columnValue));

                    exportedHeaderItem.ColumnValues[i] = columnValue;
                }
                else
                {
                    headerColumn.Name = string.Format(GeneratedTitleExportFormat, question.VariableName, i);
                }

                if (!IsQuestionLinked(question))
                {
                    var questionLabel =
                        string.IsNullOrEmpty(question.VariableLabel) ? question.QuestionText : question.VariableLabel;

                    if (question.QuestionType == QuestionType.MultyOption)
                    {
                        headerColumn.Title = $"{questionLabel}:{question.Answers[i].AnswerText}";
                    }
                    if (question.QuestionType == QuestionType.TextList)
                    {
                        headerColumn.Title = $"{questionLabel}:{i}";
                    }
                }

                headerColumn.ExportType = GetStorageType(question);
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
            var rosterGroups = document.Find<Group>(@group => group.IsRoster && @group.RosterSizeQuestionId.HasValue);

            var fixedRosterGroups =
                document.Find<Group>(@group => @group.IsRoster && group.IsFixedRoster);

            IEnumerable<MultyOptionsQuestion> rosterSizeMultyOptionQuestions =
                rosterGroups.Select(@group => document.Find<MultyOptionsQuestion>(@group.RosterSizeQuestionId.Value))
                    .Where(question => question != null).Distinct();

            IEnumerable<TextListQuestion> rosterSizeTextListQuestions =
                rosterGroups.Select(@group => document.Find<TextListQuestion>(@group.RosterSizeQuestionId.Value))
                    .Where(question => question != null).Distinct();

            var collectedMaxValues = new Dictionary<Guid, int>();

            foreach (MultyOptionsQuestion rosterSizeMultyOptionQuestion in rosterSizeMultyOptionQuestions)
            {
                collectedMaxValues.Add(rosterSizeMultyOptionQuestion.PublicKey, rosterSizeMultyOptionQuestion.Answers.Count);
            }

            foreach (TextListQuestion rosterSizeTextListQuestion in rosterSizeTextListQuestions)
            {
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
            IEnumerable<Group> rootGroups = this.GetRootGroupsForLevel(questionnaire, rosterScopes, levelVector);

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
                structures.ReferencedNames = new string[] { rosterScopes[levelVector].SizeQuestionTitle };
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
                    @group.FixedRosterTitles.Select(title => new LabelItem() { Caption = title.Value.ToString(CultureInfo.InvariantCulture), Title = title.Title })
                        .ToArray();
            }

            foreach (var groupChild in @group.Children)
            {
                if (groupChild is Question question)
                {
                    if (this.IsQuestionMultiOption(question))
                    {
                        if (question.LinkedToRosterId.HasValue)
                            this.AddHeadersForLinkedMultiOptions(headerStructureForLevel.HeaderItems, question, questionnaire, maxValuesForRosterSizeQuestions);
                        else if (question.LinkedToQuestionId.HasValue)
                        {
                            var linkToQuestion =
                                questionnaire.FirstOrDefault<Question>(
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

                if (groupChild is Variable variable)
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

        private void AddHeadersForLinkedMultiOptions(IDictionary<Guid, IExportedHeaderItem> headerItems, Question question,
            QuestionnaireDocument questionnaire,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            headerItems.Add(question.PublicKey,
                this.CreateExportedQuestionHeaderForMultiColumnItem(question,
                    this.GetRosterSizeForLinkedQuestion(question, questionnaire, maxValuesForRosterSizeQuestions),
                    this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire)));
        }

        protected void AddHeaderForSingleColumnExportQuestion(IDictionary<Guid, IExportedHeaderItem> headerItems, Question question,
            QuestionnaireDocument questionnaire)
        {
            headerItems.Add(question.PublicKey,
                this.CreateExportedQuestionHeaderItem(question,
                    this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire)));
        }

        protected void AddHeadersForMultiOptions(IDictionary<Guid, IExportedHeaderItem> headerItems, Question question,
            QuestionnaireDocument questionnaire)
        {
            headerItems.Add(question.PublicKey,
                this.CreateExportedQuestionHeaderForMultiColumnItem(question, question.Answers.Count,
                    this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire)));
        }

        protected void AddHeadersForTextList(IDictionary<Guid, IExportedHeaderItem> headerItems, Question question,
            QuestionnaireDocument questionnaire)
        {
            var textListQuestion = question as TextListQuestion;
            var maxCount = textListQuestion?.MaxAnswerCount ?? Constants.MaxLongRosterRowCount;
            headerItems.Add(question.PublicKey,
                this.CreateExportedQuestionHeaderForMultiColumnItem(question, maxCount,
                    this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire)));
        }

        private void AddHeadersForLinkedToListMultiOptions(IDictionary<Guid, IExportedHeaderItem> headerItems, 
            Question question,
            Question linkToTextListQuestion,
            QuestionnaireDocument questionnaire)
        {
            var textListQuestion = linkToTextListQuestion as TextListQuestion;
            var maxCount = (textListQuestion == null ? null : textListQuestion.MaxAnswerCount) ?? Constants.MaxLongRosterRowCount;

            headerItems.Add(question.PublicKey,
                 this.CreateExportedQuestionHeaderForMultiColumnItem(question, maxCount,
                     this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire)));
        }

        protected void AddHeadersForGpsQuestion(IDictionary<Guid, IExportedHeaderItem> headerItems, Question question,
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
                    Name = string.Format(GeneratedTitleExportFormat, question.VariableName, column),
                    Title = $"{questionLabel}: {column}",
                    ExportType = string.Compare(column, "timestamp", StringComparison.OrdinalIgnoreCase) == 0 ? ExportValueType.DateTime : ExportValueType.Numeric
                });
            }

            headerItems.Add(question.PublicKey, gpsQuestionExportHeader);
        }

        private int GetRosterSizeForLinkedQuestion(Question question, QuestionnaireDocument questionnaire,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            Guid rosterSizeQuestionId =
                this.GetRosterSizeSourcesForEntity(GetReferencedByLinkedQuestionEntity(question, questionnaire)).Last();

            if (!maxValuesForRosterSizeQuestions.ContainsKey(rosterSizeQuestionId))
                return Constants.MaxRosterRowCount;

            return maxValuesForRosterSizeQuestions[rosterSizeQuestionId];
        }

        private IQuestionnaireEntity GetReferencedByLinkedQuestionEntity(Question question, QuestionnaireDocument questionnaire)
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

        public ValueVector<Guid> GetRosterSizeSourcesForEntity(IQuestionnaireEntity entity)
        {
            var rosterSizes = new List<Guid>();
            while (!(entity is QuestionnaireDocument))
            {
                if (entity is Group group)
                {
                    if (group.IsRoster)
                        rosterSizes.Add(group.RosterSizeQuestionId ?? group.PublicKey);

                }
                entity = entity.GetParent();
            }
            rosterSizes.Reverse();
            return rosterSizes.ToArray();
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


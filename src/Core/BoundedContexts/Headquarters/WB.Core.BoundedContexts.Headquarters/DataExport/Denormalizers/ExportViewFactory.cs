﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Infrastructure.Native.Questionnaire;
using WB.Infrastructure.Native.Sanitizer;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers
{
    internal class ExportViewFactory : IExportViewFactory
    {
        private const string GeneratedTitleExportFormat = "{0}__{1}";
        
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IRosterStructureService rosterStructureService;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage;
        private readonly IReusableCategoriesStorage reusableCategoriesStorage;

        public ExportViewFactory(
            IFileSystemAccessor fileSystemAccessor,
            IQuestionnaireStorage questionnaireStorage,
            IRosterStructureService rosterStructureService,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage,
            IReusableCategoriesStorage reusableCategoriesStorage)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.questionnaireStorage = questionnaireStorage;
            this.rosterStructureService = rosterStructureService;
            this.questionnaireBrowseItemStorage = questionnaireBrowseItemStorage;
            this.reusableCategoriesStorage = reusableCategoriesStorage;
        }

        public QuestionnaireExportStructure CreateQuestionnaireExportStructure(QuestionnaireIdentity id)
        {
            QuestionnaireDocument questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(id);
            if (questionnaire == null)
                return null;

            return CreateQuestionnaireExportStructure(questionnaire, id);
        }

        private QuestionnaireExportStructure CreateQuestionnaireExportStructure(QuestionnaireDocument questionnaire, QuestionnaireIdentity id)
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
                this.BuildHeaderByTemplate(questionnaire, supportVariables, new ValueVector<Guid>(), rosterScopes, maxValuesForRosterSizeQuestions, id));

            foreach (var rosterScopeDescription in rosterScopes)
            {
                result.HeaderToLevelMap.Add(rosterScopeDescription.Key,
                    this.BuildHeaderByTemplate(questionnaire, supportVariables, rosterScopeDescription.Key, rosterScopes, maxValuesForRosterSizeQuestions, id));
            }

            return result;
        }

        protected HeaderStructureForLevel CreateHeaderStructureForLevel(
            string levelTitle,
            IEnumerable<IGroup> groupsInLevel,
            QuestionnaireDocument questionnaire,
            bool supportVariables,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions,
            ValueVector<Guid> levelVector,
            QuestionnaireIdentity id)
        {
            var headerStructureForLevel = new HeaderStructureForLevel();
            headerStructureForLevel.LevelScopeVector = levelVector;
            headerStructureForLevel.LevelIdColumnName = (levelVector == null || levelVector.Length == 0) ? ServiceColumns.InterviewId : string.Format(ServiceColumns.IdSuffixFormat, levelTitle);

            headerStructureForLevel.LevelName = levelTitle;

            foreach (var rootGroup in groupsInLevel)
            {
                this.FillHeaderWithQuestionsInsideGroup(headerStructureForLevel, rootGroup, questionnaire, supportVariables,
                    maxValuesForRosterSizeQuestions, id);
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
                    ExportType = GetStorageType(variable) 
                }};

            return exportedHeaderItem;
        }

        private ExportValueType GetStorageType(IVariable variable)
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

        private ExportedQuestionHeaderItem CreateExportedQuestionHeaderForMultiColumnItem(IQuestion question, 
            int columnCount, int? lengthOfRosterVectorWhichNeedToBeExported, QuestionnaireIdentity id)
        {
            var exportedHeaderItem = this.CreateExportedQuestionHeaderItem(question, lengthOfRosterVectorWhichNeedToBeExported);
            this.ThrowIfQuestionIsNotMultiSelectOrTextList(question);

            exportedHeaderItem.ColumnValues = new int[columnCount];
            exportedHeaderItem.ColumnHeaders = new List<HeaderColumn>();

            CategoriesItem[] reusableOptions = null;
            var multi = question as IMultyOptionsQuestion;
            if (multi != null && multi.CategoriesId.HasValue)
            {
                reusableOptions = reusableCategoriesStorage.GetOptions(id, multi.CategoriesId.Value).ToArray();
            }


            for (int i = 0; i < columnCount; i++)
            {
                HeaderColumn headerColumn = new HeaderColumn();
                if (!IsQuestionLinked(question) && multi != null)
                {
                    var columnValue = multi.CategoriesId.HasValue 
                        ? reusableOptions[i].Id
                        : int.Parse(question.Answers[i].AnswerValue);
              
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

                    if (multi != null)
                    {
                        var optionText = multi?.IsFilteredCombobox ?? false
                            ? i.ToString()
                            : multi?.CategoriesId.HasValue ?? false
                                ? reusableOptions[i].Text
                                : question.Answers[i].AnswerText;

                        headerColumn.Title = $"{questionLabel}:{optionText}";
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
            Dictionary<ValueVector<Guid>, RosterScopeDescription> rosterScopes, Dictionary<Guid, int> maxValuesForRosterSizeQuestions, QuestionnaireIdentity id)
        {
            IEnumerable<IGroup> rootGroups = this.GetRootGroupsForLevel(questionnaire, rosterScopes, levelVector);

            if (!rootGroups.Any())
                throw new InvalidOperationException("level is absent in template");

            var firstRootGroup = rootGroups.First();
            var levelTitle = firstRootGroup.VariableName ?? this.fileSystemAccessor.MakeStataCompatibleFileName(firstRootGroup.Title);

            var structures = this.CreateHeaderStructureForLevel(levelTitle, rootGroups, questionnaire, supportVariables,
                maxValuesForRosterSizeQuestions, levelVector, id);

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
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions,
            QuestionnaireIdentity id)
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
                    if (question is IMultyOptionsQuestion multyOptionsQuestion)
                    {
                        if (multyOptionsQuestion.LinkedToRosterId.HasValue)
                            this.AddHeadersForLinkedMultiOptions(headerStructureForLevel.HeaderItems, 
                                multyOptionsQuestion, questionnaire, maxValuesForRosterSizeQuestions, id);
                        else if (multyOptionsQuestion.LinkedToQuestionId.HasValue)
                        {
                            var linkToQuestion =
                                questionnaire.FirstOrDefault<IQuestion>(
                                    x => x.PublicKey == multyOptionsQuestion.LinkedToQuestionId.Value);

                            if (linkToQuestion.QuestionType == QuestionType.TextList)
                            {
                                this.AddHeadersForLinkedToListMultiOptions(headerStructureForLevel.HeaderItems, 
                                    multyOptionsQuestion, linkToQuestion, questionnaire, id);
                            }
                            else
                                this.AddHeadersForLinkedMultiOptions(headerStructureForLevel.HeaderItems, 
                                    multyOptionsQuestion, questionnaire, maxValuesForRosterSizeQuestions, id);
                        }

                        else this.AddHeadersForMultiOptions(headerStructureForLevel.HeaderItems, multyOptionsQuestion, questionnaire, id);
                    }
                    else if (question is ITextListQuestion textListQuestion)
                    {
                        this.AddHeadersForTextList(headerStructureForLevel.HeaderItems, textListQuestion, 
                            questionnaire, id);
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
                        maxValuesForRosterSizeQuestions, id);
                }
            }
        }

        private void AddHeadersForVariable(IDictionary<Guid, IExportedHeaderItem> headerItems, IVariable variable)
        {
            headerItems.Add(variable.PublicKey, this.CreateExportedVariableHeaderItem(variable));
        }

        private void AddHeadersForLinkedMultiOptions(IDictionary<Guid, IExportedHeaderItem> headerItems, IQuestion question,
            QuestionnaireDocument questionnaire,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions,
            QuestionnaireIdentity id)
        {
            headerItems.Add(question.PublicKey,
                this.CreateExportedQuestionHeaderForMultiColumnItem(question,
                    this.GetRosterSizeForLinkedQuestion(question, questionnaire, maxValuesForRosterSizeQuestions),
                    this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire), id));
        }

        protected void AddHeaderForSingleColumnExportQuestion(IDictionary<Guid, IExportedHeaderItem> headerItems, IQuestion question,
            QuestionnaireDocument questionnaire)
        {
            headerItems.Add(question.PublicKey,
                this.CreateExportedQuestionHeaderItem(question,
                    this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire)));
        }

        protected void AddHeadersForMultiOptions(IDictionary<Guid, IExportedHeaderItem> headerItems, 
            IMultyOptionsQuestion question, QuestionnaireDocument questionnaire, QuestionnaireIdentity id)
        {
            var columnCount = GetColumnsCountForMultiOptionQuestion(question, questionnaire, id);
            
            headerItems.Add(question.PublicKey,
                this.CreateExportedQuestionHeaderForMultiColumnItem(question, columnCount,
                    this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire), id));
        }
        
        private int GetColumnsCountForMultiOptionQuestion(IMultyOptionsQuestion question, 
            QuestionnaireDocument questionnaire, QuestionnaireIdentity id)
        {
            var optionCount = question.CategoriesId != null
                ? reusableCategoriesStorage.GetOptions(id, question.CategoriesId.Value).Count()
                : question.Answers.Count;

            if (question.IsFilteredCombobox ?? false)
                return Math.Min(question.MaxAllowedAnswers ?? Constants.MaxLongRosterRowCount, optionCount);

            return optionCount;
        }

        protected void AddHeadersForTextList(IDictionary<Guid, IExportedHeaderItem> headerItems, 
            ITextListQuestion textListQuestion, QuestionnaireDocument questionnaire, QuestionnaireIdentity id)
        {
            var maxCount = textListQuestion.MaxAnswerCount ?? TextListQuestion.MaxAnswerCountLimit;
            headerItems.Add(textListQuestion.PublicKey,
                this.CreateExportedQuestionHeaderForMultiColumnItem(textListQuestion, maxCount,
                    this.GetLengthOfRosterVectorWhichNeedToBeExported(textListQuestion, questionnaire), id));
        }

        private void AddHeadersForLinkedToListMultiOptions(IDictionary<Guid, IExportedHeaderItem> headerItems, 
            IQuestion question,
            IQuestion linkToTextListQuestion,
            QuestionnaireDocument questionnaire,
            QuestionnaireIdentity id)
        {
            var textListQuestion = linkToTextListQuestion as ITextListQuestion;
            var maxCount = (textListQuestion == null ? null : textListQuestion.MaxAnswerCount) ?? TextListQuestion.MaxAnswerCountLimit;

            headerItems.Add(question.PublicKey,
                 this.CreateExportedQuestionHeaderForMultiColumnItem(question, maxCount,
                     this.GetLengthOfRosterVectorWhichNeedToBeExported(question, questionnaire), id));
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
            return new ValueVector<Guid>(rosterSizes);
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

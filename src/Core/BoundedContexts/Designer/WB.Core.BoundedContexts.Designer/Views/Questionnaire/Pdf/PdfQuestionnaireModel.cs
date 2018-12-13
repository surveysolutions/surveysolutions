using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Infrastructure.Native.Sanitizer;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    public class PdfQuestionnaireModel
    {
        public class ModificationStatisticsByUser
        {
            public Guid UserId { get; set; }
            public string Name { get; set; }
            public DateTime? Date { get; set; }

            public DateTime? On => this.Date;
            public string By => this.Name;
        }

        public class GroupStatistics
        {
            public int GroupsCount { get; set; } = 0;
            public int RostersCount { get; set; } = 0;
            public int QuestionsCount { get; set; } = 0;
            public int StaticTextsCount { get; set; } = 0;
            public int VariablesCount { get; set; } = 0;
        }

        public class QuestionnaireStatistics : GroupStatistics
        {
            public int SectionsCount { get; set; } = 0;
            public int QuestionsWithEnablingConditionsCount { get; set; } = 0;
            public int QuestionsWithValidationConditionsCount { get; set; } = 0;
        }

        public class EntityWithLongCondition
        {
            public  int Index { get; set; }
            public Guid Id { get; set; }
            public string VariableName { get; set; }
            public string Title { get; set; }
            public string EnablementCondition { get; set; }
        }

        public class EntityWithLongValidation
        {
            public int Index { get; set; }
            public Guid Id { get; set; }
            public string VariableName { get; set; }
            public string Title { get; set; }
            public List<ValidationCondition> ValidationConditions { get; set; }
        }

        private readonly QuestionnaireDocument questionnaire;
        public PdfSettings Settings { get; }

        public ModificationStatisticsByUser Created { get; set; }
        public ModificationStatisticsByUser LastModified { get; set; }
        public ModificationStatisticsByUser Requested { get; set; }

        public QuestionnaireStatistics Statistics { get; set; } = new QuestionnaireStatistics();
        internal List<IComposite> AllItems;

        public PdfQuestionnaireModel(QuestionnaireDocument questionnaire, PdfSettings settings)
        {
            this.questionnaire = questionnaire;
            this.Settings = settings;
            this.Metadata = this.questionnaire.Metadata ?? new QuestionnaireMetaInfo();
        }

        public List<IQuestion> QuestionsWithLongOptionsList { get; internal set; }

        public List<IQuestion> QuestionsWithLongSpecialValuesList { get; internal set; }

        public List<IQuestion> QuestionsWithLongInstructions { get; internal set; }

        public List<IQuestion> QuestionsWithLongOptionsFilterExpression { get; internal set; }

        public List<EntityWithLongValidation> ItemsWithLongValidations { get; internal set; }

        public List<EntityWithLongCondition> ItemsWithLongConditions { get; internal set; }

        public List<IVariable> VariableWithLongExpressions { get; internal set; }

        public string Title => this.questionnaire.Title;
        public QuestionnaireMetaInfo Metadata { get; internal set; } 
        public IEnumerable<Guid> SectionIds => this.questionnaire.Children.Select(x => x.PublicKey).ToList();
        public IEnumerable<ModificationStatisticsByUser> SharedPersons { get; set; }

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
           => this.Find(condition).FirstOrDefault();

        public IEnumerable<T> Find<T>() where T : class
           => this.AllItems.Where(x => x is T).Cast<T>();

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
            => this.Find<T>().Where(condition);

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            return this.AllItems.FirstOrDefault(x => x is T && x.PublicKey == publicKey) as T;
        }

        public IEnumerable<IComposite> GetChildren(Guid groupId)
        {
            return this.Find<Group>(groupId).Children;
        }

        public bool IsQuestion(IComposite item)
        {
            return item is IQuestion;
        }

        public bool IsGroup(IComposite item)
        {
            return item is IGroup;
        }

        public bool IsRoster(IComposite item)
        {
            return this.IsGroup(item) && ((IGroup) item).IsRoster;
        }

        public bool IsStaticText(IComposite item)
        {
            return item is IStaticText;
        }

        public bool IsVariable(IComposite item)
        {
            return item is IVariable;
        }

        public string GetBreadcrumbsForGroup(Guid groupId)
        {
            var parents = this.GetAllParentGroupsStartingFromBottom(this.Find<Group>(groupId), this.questionnaire).ToList();
            parents.Reverse();
            return string.Join(" / ", parents.Select(x => x.Title));
        }

        private IEnumerable<IGroup> GetAllParentGroupsStartingFromBottom(IGroup group, QuestionnaireDocument document)
        {
            var startGroupId = group.PublicKey;
            while (group != document)
            {
                if (group.PublicKey != startGroupId)
                {
                    yield return group;
                }
                group = (IGroup)group.GetParent();
            }
        }

        public GroupStatistics GetGroupStatistics(Guid groupId)
        {
            var statistics = new GroupStatistics();
            var childItems = this.Find<Group>(groupId).TreeToEnumerable<IComposite>(g => g.Children).Where(x => x.PublicKey != groupId);
            return this.FillStatistics(childItems, statistics);
        }

        internal GroupStatistics FillStatistics(IEnumerable<IComposite> items, GroupStatistics statistics)
        {
            foreach (var item in items)
            {
                if (this.IsQuestion(item))
                    statistics.QuestionsCount++;
                else if (this.IsRoster(item))
                    statistics.RostersCount++;
                else if (this.IsGroup(item))
                    statistics.GroupsCount++;
                else if (this.IsStaticText(item))
                    statistics.StaticTextsCount++;
                else if (this.IsVariable(item))
                    statistics.VariablesCount++;
            }
            return statistics;
        }

        public string GetQuestionTitle(IQuestion question) => question.QuestionText;

        public string GetGroupTitle(IGroup group) => group.Title;

        public string GetGroupTitle(Guid groupId, bool removeHtmlTags = false) => removeHtmlTags ? this.Find<Group>(groupId).Title.RemoveHtmlTags() : this.Find<Group>(groupId).Title;

        public string GetStaticText(IStaticText staticText) => staticText.Text;

        public bool QuestionHasInstructions(IQuestion question) => !string.IsNullOrWhiteSpace(question.Instructions);

        public bool QuestionHasEnablementCondition(IQuestion question) => !string.IsNullOrWhiteSpace(question.ConditionExpression);

        public bool QuestionHasOptionsFilter(IQuestion question) => !string.IsNullOrWhiteSpace(question.Properties.OptionsFilterExpression) || !string.IsNullOrWhiteSpace(question.LinkedFilterExpression);

        public bool GroupHasEnablementCondition(IGroup group) => !string.IsNullOrWhiteSpace(@group.ConditionExpression);

        public bool StaticTextHasEnablementCondition(IStaticText text) => !string.IsNullOrWhiteSpace(text.ConditionExpression);

        public string GetRosterSourceQuestionVariable(Guid rosterId)
        {
            var roster = this.Find<Group>(rosterId);
            return roster?.RosterSizeQuestionId != null
                ? this.Find<IQuestion>(roster.RosterSizeQuestionId.Value)?.StataExportCaption
                : string.Empty;
        }

        public string GetStringifiedTypeOfRosterSizeQuestion(Guid? rosterSizeQuestionId)
        {
            if (!rosterSizeQuestionId.HasValue)
                return string.Empty;

            var question = this.Find<IQuestion>(rosterSizeQuestionId.Value);
            if (question == null)
                return string.Empty;
            switch (question.QuestionType)
            {
                case QuestionType.MultyOption:
                    return PdfStrings.RosterType_Multi;
                case QuestionType.Numeric:
                    return PdfStrings.RosterType_Numeric;
                case QuestionType.TextList:
                    return PdfStrings.RosterType_List;
                default:
                    return string.Empty;
            }
        }

        public string GetFormattedFixedRosterValue(IGroup roster, decimal value) =>
            this.FormatAsIntegerWithLeadingZeros(value, roster.FixedRosterTitles.Select(x => (double)x.Value));

        public string GetFormattedOptionValue(List<Answer> options, string optionValueAsString)
        {
            if (string.IsNullOrEmpty(optionValueAsString))
                return string.Empty;

            var optionValue = decimal.Parse(optionValueAsString);

            var values = options.Select(x => string.IsNullOrEmpty(x.AnswerValue) ? 0 : double.Parse(x.AnswerValue));
            return FormatAsIntegerWithLeadingZeros(optionValue, values);
        }

        public string GetFormattedQuestionType(IQuestion question)
        {
            switch (question.QuestionType)
            {
                case QuestionType.SingleOption:
                {
                    var singleQuestion = (question as SingleQuestion);
                    var isLinked = (singleQuestion?.LinkedToQuestionId.HasValue ?? false) ||
                                    (singleQuestion?.LinkedToRosterId.HasValue ?? false);
                    var isCascading = singleQuestion?.CascadeFromQuestionId.HasValue ?? false;
                    var isCombobox = singleQuestion?.IsFilteredCombobox ?? false;
                    var questionOptions = (new[]
                    {
                        isLinked ? PdfStrings.QuestionType_Linked : "",
                        isCascading? PdfStrings.QuestionType_Cascading : "",
                        isCombobox? PdfStrings.QuestionType_Combobox : ""
                    }).Where(x => !string.IsNullOrWhiteSpace(x));

                    var stringifiedQuestionOptions = string.Join(", ", questionOptions);
                    return PdfStrings.QuestionType_SingleSelect + (string.IsNullOrWhiteSpace(stringifiedQuestionOptions) ? "" : ": " + stringifiedQuestionOptions);
                }
                case QuestionType.MultyOption:
                {
                    var multyOptionsQuestion = (question as MultyOptionsQuestion);
                    var areAnswersOrdered = multyOptionsQuestion?.AreAnswersOrdered ?? false;
                    var isYesNoView = multyOptionsQuestion?.YesNoView ?? false;
                    var isLinked = (multyOptionsQuestion?.LinkedToQuestionId.HasValue ?? false) ||
                                    (multyOptionsQuestion?.LinkedToRosterId.HasValue ?? false);

                    var questionOptions = (new[]
                    {
                        areAnswersOrdered ? PdfStrings.QuestionType_Ordered : "",
                        isYesNoView ? PdfStrings.QuestionType_YesNo : "",
                        isLinked ? PdfStrings.QuestionType_Linked : ""
                    }).Where(x => !string.IsNullOrWhiteSpace(x));
                    var stringifiedQuestionOptions = string.Join(", ", questionOptions);
                    return PdfStrings.QuestionType_MultiSelect + (string.IsNullOrWhiteSpace(stringifiedQuestionOptions) ? "" : ": " + stringifiedQuestionOptions);
                }
                case QuestionType.Numeric:
                    var isInteger = (question as NumericQuestion)?.IsInteger ?? false;
                    return PdfStrings.QuestionType_Numeric + " " + (isInteger ? PdfStrings.QuestionType_Integer : PdfStrings.QuestionType_Decimal);
                case QuestionType.DateTime:
                    var isTimestamp = (question as DateTimeQuestion)?.IsTimestamp ?? false;
                    return isTimestamp ? PdfStrings.QuestionType_CurrentTime : PdfStrings.QuestionType_Date;
                case QuestionType.GpsCoordinates:
                    return PdfStrings.QuestionType_GPS;
                case QuestionType.Text:
                    return PdfStrings.QuestionType_Text;
                case QuestionType.TextList:
                    return PdfStrings.QuestionType_List;
                case QuestionType.QRBarcode:
                    return PdfStrings.QuestionType_Barcode;
                case QuestionType.Multimedia:
                    return PdfStrings.QuestionType_Picture;
                case QuestionType.Area:
                    return PdfStrings.QuestionType_Area;
                case QuestionType.Audio:
                    return PdfStrings.QuestionType_Audio;
                default:
                    return string.Empty;
            }
        }

        public string GetFormattedVariableType(IVariable variable)
        {
            switch (variable.Type)
            {
                case VariableType.String:
                    return "string";
                case VariableType.Boolean:
                    return "boolean";
                case VariableType.DateTime:
                    return "datetime";
                case VariableType.Double:
                    return "double";
                case VariableType.LongInteger:
                    return "long";
                default:
                    return string.Empty;
            }
        }

        public string GetQuestionOptionsFilter(IQuestion question)
        {
            return string.IsNullOrWhiteSpace(question.Properties.OptionsFilterExpression) ? question.LinkedFilterExpression : question.Properties.OptionsFilterExpression;
        }

        private string FormatAsIntegerWithLeadingZeros(decimal value, IEnumerable<double> values)
        {
            var maxValue = values.Select(x => Math.Floor(Math.Log10(Math.Abs(x)) + 1)).Max();
            maxValue = Math.Max(Settings.MinAmountOfDigitsInCodes, maxValue);
            return Convert.ToInt64(value).ToString($"D{maxValue}");
        }

        public string GetQuestionInstructionExcerpt(IQuestion question) =>
            question.Instructions.Substring(0, Math.Min(this.Settings.InstructionsExcerptLength, question.Instructions.Length));

        public string GetQuestionOptionsFilterExcerpt(IQuestion question) =>
            GetQuestionOptionsFilter(question).Substring(0, Math.Min(this.Settings.VariableExpressionExcerptLength, GetQuestionOptionsFilter(question).Length));

        public string GetLinkedQuestionFilterExcerpt(IQuestion question) =>
            question.LinkedFilterExpression.Substring(0, Math.Min(this.Settings.VariableExpressionExcerptLength, question.LinkedFilterExpression.Length));

        public string GetVariableExpressionExcerpt(IVariable variable) =>
            variable.Expression?.Substring(0, Math.Min(this.Settings.VariableExpressionExcerptLength, variable.Expression.Length));

        public bool InstructionIsTooLong(IQuestion question) =>
            question.Instructions?.Length > this.Settings.InstructionsExcerptLength;

        public bool OptionsFilterIsTooLong(IQuestion question) =>
            question.Properties.OptionsFilterExpression?.Length > this.Settings.VariableExpressionExcerptLength || question.LinkedFilterExpression?.Length > this.Settings.VariableExpressionExcerptLength;

        public bool VariableExpressionIsTooLong(IVariable variable) =>
            variable.Expression?.Length > this.Settings.VariableExpressionExcerptLength;

        public int GetEntityIndexInAppendix(Guid entityId, string appendix)
        {
            switch (appendix)
            {
                case "E":
                    return this.ItemsWithLongConditions.Single(x => x.Id == entityId).Index;
                case "V":
                    return this.ItemsWithLongValidations.Single(x => x.Id == entityId).Index;
                case "I":
                {
                    var question = Find<IQuestion>(entityId);
                    return  QuestionsWithLongInstructions.IndexOf(question) + 1;
                }
                case "F":
                {
                    var question = Find<IQuestion>(entityId);
                    return QuestionsWithLongOptionsFilterExpression.IndexOf(question) + 1;
                }
                case "O":
                {
                    var question = Find<IQuestion>(entityId);
                    return QuestionsWithLongOptionsList.IndexOf(question) + 1;
                }
                case "VE":
                {
                    return this.VariableWithLongExpressions.FindIndex(x => x.PublicKey == entityId) + 1;
                }
                case "SV":
                {
                    return this.QuestionsWithLongSpecialValuesList.FindIndex(x => x.PublicKey == entityId) + 1;
                }
            }
            return -1;
        }

        public bool ExpressionIsTooLong(string expression) => expression?.Length > this.Settings.ExpressionExcerptLength;

        public string GetExpressionExcerpt(string expression) => expression?.Substring(0, Math.Min(this.Settings.ExpressionExcerptLength, expression.Length)) ?? string.Empty;

        public string GetInstructionsRef(Guid id) => $"instructions-{id.FormatGuid()}";

        public string GetOptionsFilterRef(Guid id) => $"options-filter-{id.FormatGuid()}";

        public string GetConditionRef(Guid id) => $"condition-{id.FormatGuid()}";

        public string GetValidationsRef(Guid id) => $"validations-{id.FormatGuid()}";

        public string GetOptionsRef(Guid id) => $"options-{id.FormatGuid()}";

        public string GetVariableRef(Guid id) => $"variables-{id.FormatGuid()}";

        public string GetItemRef(Guid id) => $"{id.FormatGuid()}";

        public bool IsYesNoMultiQuestion(IQuestion question) => (question as MultyOptionsQuestion)?.YesNoView ?? false;

        public int GetValidationsCount(IList<ValidationCondition> validationConditions) => validationConditions?.Count ?? 0;

        public Guid GetAttachmentId(IStaticText staticText) => this.questionnaire.Attachments.First(x => x.Name == staticText.AttachmentName).AttachmentId;

        public bool StaticTextHasAttachedImage(IStaticText staticText) => !string.IsNullOrWhiteSpace(staticText.AttachmentName) &&
                                                                          this.questionnaire.Attachments.Any(x => x.Name == staticText.AttachmentName);

        public string GetQuestionScope(IQuestion question)
        {
            switch (question.QuestionScope)
            {
                case QuestionScope.Interviewer:
                    if (question.Featured)
                    {
                        return PdfStrings.QuestionScope_Identifying;
                    }
                    return string.Empty;
                case QuestionScope.Supervisor:
                    return PdfStrings.QuestionScope_Supervisor;
                case QuestionScope.Headquarter:
                    return PdfStrings.QuestionScope_Identifying;
                case QuestionScope.Hidden:
                    return PdfStrings.QuestionScope_Hidden;
                default:
                    return string.Empty;
            }
        }

        public bool IsInterviewerQuestion(IQuestion question) => question.QuestionScope == QuestionScope.Interviewer && !question.Featured;

        public bool IsConditionsAppendixEmpty => ItemsWithLongConditions.Count == 0;
        public bool IsValidationsAppendixEmpty => ItemsWithLongValidations.Count == 0;
        public bool IsInstructionsAppendixEmpty => QuestionsWithLongInstructions.Count == 0;
        public bool IsOptionsFilterAppendixEmpty => QuestionsWithLongOptionsFilterExpression.Count == 0;
        public bool IsOptionsAppendixEmpty => QuestionsWithLongOptionsList.Count == 0;
        public bool IsVariablesAppendixEmpty => VariableWithLongExpressions.Count == 0;

        public bool IsSpecialValuesAppendixEmpty => QuestionsWithLongSpecialValuesList.Count == 0;

        public char ConditionsAppendixIndex => 'A';
        public char ValidationsAppendixIndex => IsConditionsAppendixEmpty ? ConditionsAppendixIndex : (char)(ConditionsAppendixIndex + 1);
        public char InstructionsAppendixIndex => IsValidationsAppendixEmpty ? ValidationsAppendixIndex : (char)(ValidationsAppendixIndex + 1);
        public char OptionsAppendixIndex => IsInstructionsAppendixEmpty ? InstructionsAppendixIndex : (char)(InstructionsAppendixIndex + 1);
        public char VariablesAppendixIndex => IsOptionsAppendixEmpty ? OptionsAppendixIndex : (char)(OptionsAppendixIndex + 1);
        public char OptionsFilterAppendixIndex => IsVariablesAppendixEmpty ? VariablesAppendixIndex : (char)(VariablesAppendixIndex + 1);

        public char SpecialValuesAppendixIndex => IsVariablesAppendixEmpty ? OptionsFilterAppendixIndex : (char)(OptionsFilterAppendixIndex + 1);
    }
}

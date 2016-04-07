using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    public class PdfSettings
    {
        public int InstructionsExcerptLength { get; } = 200;
        public int ExpressionExcerptLength { get; } = 200;
        public int OptionsExcerptCount { get; } = 16;
        public int MinAmountOfDigitsInCodes { get; } = 2;
    }

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
        }

        public class QuestionnaireStatistics : GroupStatistics
        {
            public int SectionsCount { get; set; } = 0;
            public int QuestionsWithConditionsCount { get; set; } = 0;
        }

        private readonly QuestionnaireDocument questionnaire;
        public PdfSettings Settings { get; }

        public ModificationStatisticsByUser Created { get; set; }
        public ModificationStatisticsByUser LastModified { get; set; }
        public ModificationStatisticsByUser Requested { get; set; }

        public QuestionnaireStatistics Statistics { get; set; } = new QuestionnaireStatistics();
        private readonly List<IComposite> allItems;

        public PdfQuestionnaireModel(QuestionnaireDocument questionnaire, PdfSettings settings)
        {
            this.questionnaire = questionnaire;
            this.Settings = settings;
            this.questionnaire.ConnectChildrenWithParent();
            this.allItems = this.questionnaire.Children.SelectMany<IComposite, IComposite>(x => x.TreeToEnumerable<IComposite>(g => g.Children)).ToList();

            QuestionsWithLongConditions = Find<IQuestion>(x => x.ConditionExpression?.Length > Settings.ExpressionExcerptLength).ToList();
            QuestionsWithLongValidations = Find<IQuestion>(x => x.ValidationConditions.Count > 0 && x.ValidationConditions.Any(condition => condition.Expression?.Length > Settings.ExpressionExcerptLength)).ToList();
            QuestionsWithLongInstructions = Find<IQuestion>(x => x.Instructions?.Length > Settings.InstructionsExcerptLength).ToList();
            QuestionsWithLongOptionsList = Find<IQuestion>(x => x.Answers?.Count > Settings.OptionsExcerptCount).ToList();

            FillStatistics(this.allItems, this.Statistics);
            this.Statistics.SectionsCount = questionnaire.Children.Count;
            this.Statistics.GroupsCount -= Statistics.SectionsCount;
            this.Statistics.QuestionsWithConditionsCount = Find<IQuestion>(x => !string.IsNullOrWhiteSpace(x.ConditionExpression) || x.ValidationConditions.Any()).Count();
        }

        public List<IQuestion> QuestionsWithLongOptionsList { get; private set; }

        public List<IQuestion> QuestionsWithLongInstructions { get; private set; }

        public List<IQuestion> QuestionsWithLongValidations { get; private set; }

        public List<IQuestion> QuestionsWithLongConditions { get; private set; }

        public string Title => this.questionnaire.Title;
        public IEnumerable<Guid> SectionIds => this.questionnaire.Children.Select(x => x.PublicKey).ToList();
        public IEnumerable<ModificationStatisticsByUser> SharedPersons { get; set; }

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
           => this.Find(condition).FirstOrDefault();

        public IEnumerable<T> Find<T>() where T : class
           => this.allItems.Where(x => x is T).Cast<T>();

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
            => this.Find<T>().Where(condition);

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            return this.allItems.FirstOrDefault(x => x is T && x.PublicKey == publicKey) as T;
        }

        public string GetGroupTitle(Guid groupId) => this.Find<Group>(groupId).Title;

        public IEnumerable<IComposite> GetChildren(Guid groupId)
        {
            return this.Find<Group>(groupId).Children;
        }

        public bool IsQuestion(IComposite item)
        {
            var question = item as IQuestion;
            return question != null;
        }

        public bool IsGroup(IComposite item)
        {
            var group = item as IGroup;
            return group != null;
        }

        public bool IsRoster(IComposite item)
        {
            return this.IsGroup(item) && (item as IGroup).IsRoster;
        }

        public bool IsStaticText(IComposite item)
        {
            var text = item as IStaticText;
            return text != null;
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

        private GroupStatistics FillStatistics(IEnumerable<IComposite> items, GroupStatistics statistics)
        {
            foreach (var item in items)
            {
                if (this.IsQuestion(item))
                    statistics.QuestionsCount++;
                else if (this.IsRoster(item))
                    statistics.RostersCount++;
                else if (this.IsGroup(item))
                    statistics.GroupsCount++;
                else
                    statistics.StaticTextsCount++;
            }
            return statistics;
        }

        public bool QuestionHasInstructions(IQuestion question) => !string.IsNullOrWhiteSpace(question.Instructions);

        public bool QuestionHasEnablementCondition(IQuestion question) => !string.IsNullOrWhiteSpace(question.ConditionExpression);

        public bool GroupHasEnablementCondition(IGroup group) => !string.IsNullOrWhiteSpace(@group.ConditionExpression);

        public string GetRosterSourceQuestionVariable(Guid rosterId)
        {
            var roster = this.Find<Group>(rosterId);
            return roster.RosterSizeQuestionId != null
                ? this.Find<IQuestion>(roster.RosterSizeQuestionId.Value)?.StataExportCaption
                : string.Empty;
        }

        public string GetStringifiedTypeOfRosterSizeQuestion(Guid? rosterSizeQuestionId)
        {
            if (!rosterSizeQuestionId.HasValue)
                return string.Empty;

            var question = this.Find<IQuestion>(rosterSizeQuestionId.Value);
            switch (question.QuestionType)
            {
                case QuestionType.MultyOption:
                    return "multi-select";
                case QuestionType.Numeric:
                    return "numeric";
                case QuestionType.TextList:
                    return "list";
                default:
                    return string.Empty;
            }
        }

        public string GetFormattedFixedRosterValue(IGroup roster, decimal value) =>
            this.FormatAsIntegerWithLeadingZeros(value, roster.FixedRosterTitles.Select(x => (double)x.Value));

        public string GetFormattedOptionValue(List<Answer> options, string optionValueAsString)
        {
            var values = options.Select(x => double.Parse(x.AnswerValue));
            var optionValue = decimal.Parse(optionValueAsString);
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
                        isLinked ? "linked" : "",
                        isCascading? "cascading" : "",
                        isCombobox? "Combo box" : ""
                    }).Where(x => !string.IsNullOrWhiteSpace(x));

                    var stringifiedQuestionOptions = string.Join(", ", questionOptions);
                    return "single-select" + (string.IsNullOrWhiteSpace(stringifiedQuestionOptions) ? "" : ": " + stringifiedQuestionOptions);
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
                        areAnswersOrdered ? "ordered" : "",
                        isYesNoView ? "yes/no" : "",
                        isLinked ? "linked" : ""
                    }).Where(x => !string.IsNullOrWhiteSpace(x));
                    var stringifiedQuestionOptions = string.Join(", ", questionOptions);
                    return "multi-select" + (string.IsNullOrWhiteSpace(stringifiedQuestionOptions) ? "" : ": " + stringifiedQuestionOptions);
                }
                case QuestionType.Numeric:
                    var isInteger = (question as NumericQuestion)?.IsInteger ?? false;
                    return "numeric: " + (isInteger ? "integer" : "decimal");
                case QuestionType.DateTime:
                    return "date: MM/DD/YYYY";
                case QuestionType.GpsCoordinates:
                    return "GPS";
                case QuestionType.Text:
                    return "text";
                case QuestionType.TextList:
                    return "list";
                case QuestionType.QRBarcode:
                    return "barcode";
                case QuestionType.Multimedia:
                    return "picture";
                default:
                    return string.Empty;
            }
        }

        private string FormatAsIntegerWithLeadingZeros(decimal value, IEnumerable<double> values)
        {
            var maxValue = values.Select(x => Math.Floor(Math.Log10(Math.Abs(x)) + 1)).Max();
            maxValue = Math.Max(Settings.MinAmountOfDigitsInCodes, maxValue);
            return Convert.ToInt64(value).ToString($"D{maxValue}");
        }

        public string GetQuestionInstructionExcerpt(IQuestion question) =>
            question.Instructions.Substring(0, Math.Min(this.Settings.InstructionsExcerptLength, question.Instructions.Length));

        public bool InstructionIsTooLong(IQuestion question) =>
            question.Instructions?.Length > this.Settings.InstructionsExcerptLength;

        public int GetQuestionIndexInAppendix(Guid questionId, string appendix)
        {
            var question = Find<IQuestion>(questionId);
            var index = -2;
            switch (appendix)
            {
                case "E":
                    index = QuestionsWithLongConditions.IndexOf(question);
                    break;
                case "V":
                    index = QuestionsWithLongValidations.IndexOf(question);
                    break;
                case "I":
                    index = QuestionsWithLongInstructions.IndexOf(question);
                    break;
                case "O":
                    index = QuestionsWithLongOptionsList.IndexOf(question);
                    break;
            }
            return index + 1;
        }

        public bool ExpressionIsTooLong(string expression) => expression?.Length > this.Settings.ExpressionExcerptLength;

        public string GetExpressionExcerpt(string expression) => expression.Substring(0, Math.Min(this.Settings.ExpressionExcerptLength, expression.Length));
        public string GetInstructionsId(Guid id) => $"instructions-{id.FormatGuid()}";

        public string GetConditionId(Guid id) => $"condition-{id.FormatGuid()}";

        public string GetQuestionId(Guid id) => $"question-{id.FormatGuid()}";

        public string GetGroupId(Guid id) => $"group-{id.FormatGuid()}";

        public string GetValidationsId(Guid id) => $"validations-{id.FormatGuid()}";

        public string GetOptionsId(Guid id) => $"options-{id.FormatGuid()}";

        public bool IsYesNoMultiQuestion(IQuestion question)
        {
            var multyOptionsQuestion = (question as MultyOptionsQuestion);
            return multyOptionsQuestion?.YesNoView ?? false;
        }
    }
}
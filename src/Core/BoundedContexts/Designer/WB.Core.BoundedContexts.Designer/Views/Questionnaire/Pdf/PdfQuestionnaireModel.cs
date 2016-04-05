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

        public ModificationStatisticsByUser Created { get; set; }
        public ModificationStatisticsByUser LastModified { get; set; }
        public ModificationStatisticsByUser Requested { get; set; }

        public QuestionnaireStatistics Statistics { get; set; } = new QuestionnaireStatistics();
        private List<IComposite> allItems;

        public PdfQuestionnaireModel(QuestionnaireDocument questionnaire)
        {
            this.questionnaire = questionnaire;
            this.questionnaire.ConnectChildrenWithParent();
            this.allItems = this.questionnaire.Children.SelectMany<IComposite, IComposite>(x => x.TreeToEnumerable<IComposite>(g => g.Children)).ToList();

            FillStatistics(this.allItems, this.Statistics);
            this.Statistics.SectionsCount = questionnaire.Children.Count;
            this.Statistics.GroupsCount -= Statistics.SectionsCount;
            this.Statistics.QuestionsWithConditionsCount = Find<IQuestion>(x => !string.IsNullOrWhiteSpace(x.ConditionExpression) || x.ValidationConditions.Any()).Count();
        }

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
            var childItems = this.Find<Group>(groupId).TreeToEnumerable<IComposite>(g => g.Children).Where(x => x.PublicKey!=groupId);

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

        public bool QuestionHasInstructions(IQuestion question)
        {
            return !string.IsNullOrWhiteSpace(question.Instructions);
        }

        public bool QuestionHasEnablementCondition(IQuestion question)
        {
            return !string.IsNullOrWhiteSpace(question.ConditionExpression);
        }

        public bool GroupHasEnablementCondition(IGroup group)
        {
            return !string.IsNullOrWhiteSpace(group.ConditionExpression);
        }

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

        public string GetFormattedFixedRosterValue(IGroup roster, decimal value)
        {
            var values = roster.FixedRosterTitles.Select(x => (double)x.Value);
            return FormatAsIntegerWithLeadingZeros(value, values);
        }

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
                    return "single-select";
                case QuestionType.MultyOption:
                    var areAnswersOrdered = (question as MultyOptionsQuestion)?.AreAnswersOrdered ?? false;
                    return "multi-select" + (areAnswersOrdered ? " / ordered": "");
                case QuestionType.Numeric:
                    var isInteger = (question as NumericQuestion)?.IsInteger ?? false;
                    return "numeric / " + (isInteger ? "integer" : "decimal");
                case QuestionType.DateTime:
                    return "date / MM/DD/YYYY";
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

        private static string FormatAsIntegerWithLeadingZeros(decimal value, IEnumerable<double> values)
        {
            var maxValue = values.Select(x => Math.Floor(Math.Log10(Math.Abs(x)) + 1)).Max();
            maxValue = Math.Max(2, maxValue);
            return Convert.ToInt64(value).ToString($"D{maxValue}");
        }
    }
}
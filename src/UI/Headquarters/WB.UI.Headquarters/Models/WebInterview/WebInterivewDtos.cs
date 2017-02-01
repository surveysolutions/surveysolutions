using System;
using System.Collections.Generic;
using System.Diagnostics;
using WB.Core.SharedKernels.DataCollection;

namespace WB.UI.Headquarters.Models.WebInterview
{
    public class InterviewTextQuestion : GenericQuestion
    {
        public string Mask { get; set; }
        public string Answer { get; set; }
    }

    public class InterviewIntegerQuestion : GenericQuestion
    {
        public int? Answer { get; set; }
        public bool IsRosterSize { get; set; }
        public int? AnswerMaxValue { get; set; }
        public bool UseFormatting { get; set; }
    }

    public class InterviewDoubleQuestion : GenericQuestion
    {
        public double? Answer { get; set; }
        public int? CountOfDecimalPlaces { get; set; }
        public bool UseFormatting { get; set; }
    }

    public class InterviewSingleOptionQuestion : CategoricalQuestion
    {
        public int? Answer { get; set; }
    }

    public class InterviewCascadingComboboxQuestion : CategoricalQuestion
    {
        public DropdownItem Answer { get; set; }
    }

    public class InterviewFilteredQuestion : GenericQuestion
    {
        public DropdownItem Answer { get; set; }
    }
    
    public class InterviewDateQuestion : GenericQuestion
    {
        public bool IsTimestamp { get; set; }
        public DateTime? Answer { get; set; }
    }

    public class InterviewMutliOptionQuestion : CategoricalQuestion
    {
        public int? MaxSelectedAnswersCount { get; set; }
        public bool Ordered { get; set; }
        public int[] Answer { get; set; }
        public bool IsRosterSize { get; set; }
    }

    public class InterviewYesNoQuestion : CategoricalQuestion
    {
        public int? MaxSelectedAnswersCount { get; set; }
        public bool Ordered { get; set; }
        public InterviewYesNoAnswer[] Answer { get; set; }
        public bool IsRosterSize { get; set; }
    }

    public class InterviewYesNoAnswer
    {
        public int Value { get; set; }
        public bool Yes { get; set; }
    }

    public class CategoricalQuestion: GenericQuestion
    {
        public List<CategoricalOption> Options { get; set; }
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTextListQuestion : GenericQuestion
    {
        public int? MaxAnswersCount { get; set; }
        public List<TextListAnswerRow> Rows { get; set; }
        public bool IsRosterSize { get; set; }

        public override string ToString() => string.Join(@", ", Rows);
    }

    [DebuggerDisplay("{ToString()}")]
    public class TextListAnswerRow
    {
        public decimal Value { get; set; }
        public string Text { get; set; }

        public override string ToString() => $@"{Value} -> {Text}";
    }

    public abstract class GenericQuestion : InterviewEntity
    {
        public string Instructions { get; set; }
        public bool HideInstructions { get; set; }
        public bool IsAnswered { get; set; }
        public Validity Validity { get; set; } = new Validity();
    }

    public class Validity
    {
        public bool IsValid { get; set; }
        public string[] Messages { get; set; }
    }

    public abstract class InterviewEntity
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public bool IsDisabled { get; set; }
        public bool HideIfDisabled { get; set; }
    }

    public class InterviewStaticText : InterviewEntity
    {
        public string AttachmentContent { get; set; }
        public Validity Validity { get; set; } = new Validity();
    }

    public class InterviewGroupOrRosterInstance : InterviewEntity
    {
        public string RosterTitle { get; set; }
        public string Status { get; set; }
        public string StatisticsByAnswersAndSubsections { get; set; }
        public string StatisticsByInvalidAnswers { get; set; }
        public Validity Validity { get; set; } = new Validity();
    }

    public class SidebarPanel
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
        public string Title { get; set;}
        public string State { get;set; }
        public bool Collapsed { get; set; }
        public bool HasChildrens { get; set; }
        public Validity Validity { get; set; } = new Validity();
    }

    public enum GroupStatus
    {
        NotStarted = 1,
        Started,
        Completed
    }

    public class DropdownItem
    {
        public DropdownItem(int value, string title)
        {
            this.Value = value;
            this.Title = title;
        }

        public int Value { get; set; }

        public string Title { get; set; }
    }

    /// <summary>
    /// Used during dev, should be deleted when all types of questions are implemented
    /// </summary>
    public class StubEntity : GenericQuestion
    {
    }
}
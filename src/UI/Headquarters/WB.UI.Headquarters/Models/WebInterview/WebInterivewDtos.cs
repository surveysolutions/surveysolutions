using System;
using System.Collections.Generic;
using System.Diagnostics;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Headquarters.Models.WebInterview
{
    public class ChangeLanguageRequest
    {
        public string Language { get; set; }
    }

    public class InterviewInfo
    {
        public string QuestionnaireTitle { get; set; }
        public string FirstSectionId { get; set; }
        public string InterviewKey { get; set; }
    }

    public class LanguageInfo
    {
        public string OriginalLanguageName { get; set; }
        public string CurrentLanguage { get; set; }
        public IEnumerable<string> Languages { get; set; }
    }

    public class LinkedOption
    {
        public string Value { get; set; }
        public RosterVector RosterVector { get; set; }
        public string Title { get; set; }
    }

    public class InterviewLinkedMultiQuestion : LinkedCategoricalQuestion
    {
        public RosterVector[] Answer { get; set; }
        public bool Ordered { get; set; }
        public int? MaxSelectedAnswersCount { get; set; }
    }

    public class InterviewLinkedSingleQuestion : LinkedCategoricalQuestion
    {
        public RosterVector Answer { get; set; }
    }

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
    
    public class InterviewFilteredQuestion : GenericQuestion
    {
        public DropdownItem Answer { get; set; }
    }

    public class InterviewMultimediaQuestion : GenericQuestion
    {
        public string Answer { get; set; }
    }

    public class InterviewAudioQuestion : GenericQuestion
    {
        public string Answer { get; set; }
    }
    
    public class InterviewDateQuestion : GenericQuestion
    {
        public bool IsTimestamp { get; set; }
        public DateTime? Answer { get; set; }
    }

    public class InterviewGpsQuestion : GenericQuestion
    {
        public GpsAnswer Answer { get; set; }
    }

    public class InterviewBarcodeQuestion : GenericQuestion
    {
        public string Answer { get; set; }
    }

    public class GpsAnswer
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double? Accuracy { get; set; }
        public double? Altitude { get; set; }
        public long? Timestamp { get; set; }
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

    public class LinkedCategoricalQuestion : GenericQuestion
    {
        public List<LinkedOption> Options { get; set; }
    }

    public class CategoricalQuestion: GenericQuestion
    {
        public List<CategoricalOption> Options { get; set; }
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTextListQuestion : GenericQuestion
    {
        public int? MaxAnswersCount { get; set; }
        public List<TextListAnswerRowDto> Rows { get; set; }
        public bool IsRosterSize { get; set; }

        public override string ToString() => string.Join(@", ", Rows);
    }

    [DebuggerDisplay("{ToString()}")]
    public class TextListAnswerRowDto
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
        public Comment[] Comments { get; set; }
    }

    public class Validity
    {
        public bool IsValid { get; set; }
        public string[] Messages { get; set; }
    }

    public class Comment
    {
        public string Text { get; set; }
        public bool IsOwnComment { get; set; }
        public UserRoles UserRole { get; set; }
        public DateTime CommentTimeUtc { get; set; }
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
        public bool IsRoster { set; get; }
        public string RosterTitle { get; set; }
        public string Status { get; set; }
        public string StatisticsByAnswersAndSubsections { get; set; }
        public string StatisticsByInvalidAnswers { get; set; }
        public Validity Validity { get; set; } = new Validity();
    }

    public class Sidebar
    {
        public Sidebar()
        {
            this.Groups = new List<SidebarPanel>();
        }

        public List<SidebarPanel> Groups { get; set; }
    }

    public class SidebarPanel
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
        public string Title { get; set;}
        public string RosterTitle { get; set; }
        public string State { get;set; }
        public bool Collapsed { get; set; }
        public bool HasChildren { get; set; }
        public Validity Validity { get; set; } = new Validity();
        public bool Current { get; set; }
        public bool IsRoster { get; set; }
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

    public class CompleteInfo
    {
        public int AnsweredCount { get; set; }

        public int UnansweredCount { get; set; }

        public int ErrorsCount { get; set; }

        public EntityWithError[] EntitiesWithError { get;set; }
    }

    public class EntityWithError : QuestionReference { }

    public class CoverInfo
    {
        public CoverInfo()
        {
            this.IdentifyingQuestions = new List<IdentifyingQuestion>();
            this.EntitiesWithComments = new EntityWithComment[0];
        }

        public List<IdentifyingQuestion> IdentifyingQuestions { get; set; }
        public EntityWithComment[] EntitiesWithComments { get; set; }
        public int CommentedQuestionsCount { get; set; }
        public string SupervisorRejectComment { get; set; }
    }

    public class EntityWithComment : QuestionReference { }

    public class QuestionReference
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
        public string Title { get; set; }
        public bool IsPrefilled { get; set; }
    }

    public class CompleteInterviewRequest
    {
        public string Comment { get; set; }
    }
}
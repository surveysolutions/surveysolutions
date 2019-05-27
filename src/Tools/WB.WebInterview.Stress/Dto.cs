using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.WebInterview.Stress
{
    public class SectionData
    {
        public InterviewEntityWithType[] Entities { get; set; }
        public InterviewEntity[] Details { get; set; }
    }

    public class ChangeLanguageRequest
    {
        public string Language { get; set; }
    }

    public class InterviewInfo
    {
        public string QuestionnaireTitle { get; set; }
        public string FirstSectionId { get; set; }
        public string InterviewKey { get; set; }
        public bool InterviewCannotBeChanged { get; set; }
        public bool ReceivedByInterviewer { get; set; }
        public bool IsCurrentUserObserving { get; set; }
        public bool DoesBrokenPackageExist { get; set; }
        public long QuestionnaireVersion { get; set; }
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

    public class CategoricalOption
    {
        public int Value { get; set; }
        public int? ParentValue { get; set; }
        public string Title { get; set; }
    }

    public class RosterVector
    {

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
        public List<CategoricalOption> Options { get; set; }
        public bool IsProtected { get; set; }
        public int? ProtectedAnswer { get; set; }
    }

    public class InterviewDoubleQuestion : GenericQuestion
    {
        public double? Answer { get; set; }
        public int? CountOfDecimalPlaces { get; set; }
        public bool UseFormatting { get; set; }
        public List<CategoricalOption> Options { get; set; }
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
        public DateTime? AnswerTimeUtc { get; set; }
    }

    public class InterviewAudioQuestion : GenericQuestion
    {
        public long? Answer { get; set; }
        public string Filename { get; set; }
    }

    public class InterviewDateQuestion : GenericQuestion
    {
        public bool IsTimestamp { get; set; }
        public DateTime? Answer { get; set; }
        public DateTime? DefaultDate { get; set; }
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
        public int[] ProtectedAnswer { get; set; }
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
        public bool IsProtected { get; set; }
    }

    public class LinkedCategoricalQuestion : GenericQuestion
    {
        public List<LinkedOption> Options { get; set; }
    }

    public class CategoricalQuestion : GenericQuestion
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
        public bool IsProtected { get; set; }

        public override string ToString() => $@"{Value} -> {Text}";
    }

    public  class GenericQuestion : InterviewEntity
    {
        public string Instructions { get; set; }
        public bool HideInstructions { get; set; }
        public bool IsAnswered { get; set; }
        public bool AcceptAnswer { get; set; }
        public bool IsForSupervisor { get; set; }
        public Validity Validity { get; set; } = new Validity();
        public int MaxLength => 5;
    }

    public class Validity
    {
        public bool IsValid { get; set; }
        public string[] Messages { get; set; }
        public string[] Warnings { get; set; }
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
        public GroupStatus Status { get; set; }

        public Validity Validity { get; set; } = new Validity();
    }

    public enum GroupStatus
    {
        Disabled = 1,
        NotStarted = 2,
        Started = 3,
        StartedInvalid = 4,
        Completed = 5,
        CompletedInvalid = 6
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
        public string Title { get; set; }
        public string RosterTitle { get; set; }
        public GroupStatus Status { get; set; }
        public bool Collapsed { get; set; }
        public bool HasChildren { get; set; }
        public Validity Validity { get; set; } = new Validity();
        public bool Current { get; set; }
        public bool IsRoster { get; set; }
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

        public EntityWithError[] EntitiesWithError { get; set; }
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

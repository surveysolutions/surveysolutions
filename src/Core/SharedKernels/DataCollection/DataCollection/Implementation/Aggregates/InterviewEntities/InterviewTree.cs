using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTree
    {
        public InterviewTree(Guid interviewId, IEnumerable<InterviewTreeSection> sections)
        {
            this.InterviewId = interviewId.FormatGuid();
            this.Sections = sections.ToList();

            foreach (var section in this.Sections)
            {
                ((IInternalInterviewTreeNode)section).SetTree(this);
            }
        }

        public string InterviewId { get; }
        public IReadOnlyCollection<InterviewTreeSection> Sections { get; }

        public InterviewTreeQuestion GetQuestion(Identity questionIdentity)
            => this
                .GetNodes<InterviewTreeQuestion>()
                .Single(node => node.Identity == questionIdentity);

        public IReadOnlyCollection<InterviewTreeQuestion> FindQuestions(Guid questionId)
            => this
                .GetNodes<InterviewTreeQuestion>()
                .Where(node => node.Identity.Id == questionId)
                .ToReadOnlyCollection();

        private IEnumerable<TNode> GetNodes<TNode>() => this.GetNodes().OfType<TNode>();

        private IEnumerable<IInterviewTreeNode> GetNodes()
            => this.Sections.Cast<IInterviewTreeNode>().TreeToEnumerable(node => node.Children);

        public void RemoveNode(Identity identity)
        {
            foreach (var node in this.GetNodes().Where(x => x.Identity.Equals(identity)))
                ((InterviewTreeGroup) node.Parent)?.RemoveChildren(node.Identity);
        }

        public IReadOnlyCollection<InterviewTreeNodeDiff> FindDiff(InterviewTree changedTree)
        {
            var sourceNodes = this.Sections.SelectMany(x => x.Children).TreeToEnumerable(x => x.Children).ToList();
            var changedNodes = changedTree.Sections.SelectMany(x => x.Children).TreeToEnumerable(x => x.Children).ToList();

            var leftOuterJoin = from source in sourceNodes
                                join changed in changedNodes
                                    on source.Identity equals changed.Identity
                                    into temp
                                from changed in temp.DefaultIfEmpty()
                                select new InterviewTreeNodeDiff() { SourceNode = source, ChangedNode = changed };

            var rightOuterJoin = from changed in changedNodes
                                 join source in sourceNodes
                                     on changed.Identity equals source.Identity
                                     into temp
                                 from source in temp.DefaultIfEmpty()
                                 select new InterviewTreeNodeDiff() { SourceNode = source, ChangedNode = changed };

            var fullOuterJoin = leftOuterJoin.Concat(rightOuterJoin);

            return fullOuterJoin
                .DistinctBy(x => new { sourceIdentity = x.SourceNode?.Identity, changedIdentity = x.ChangedNode?.Identity })
                .ToReadOnlyCollection();
        }

        public override string ToString()
            => $"Tree ({this.InterviewId})" + Environment.NewLine
            + string.Join(Environment.NewLine, this.Sections.Select(section => section.ToString().PrefixEachLine("  ")));
    }

    public interface IInterviewTreeNode
    {
        Identity Identity { get; }
        IInterviewTreeNode Parent { get; }
        IReadOnlyCollection<IInterviewTreeNode> Children { get; }

        bool IsDisabled();
    }

    public interface IInternalInterviewTreeNode
    {
        void SetTree(InterviewTree tree);
        void SetParent(IInterviewTreeNode parent);
    }

    public abstract class InterviewTreeLeafNode : IInterviewTreeNode, IInternalInterviewTreeNode
    {
        private readonly bool isDisabled;

        protected InterviewTreeLeafNode(Identity identity, bool isDisabled)
        {
            this.Identity = identity;
            this.isDisabled = isDisabled;
        }

        public Identity Identity { get; }
        public InterviewTree Tree { get; private set; }
        public IInterviewTreeNode Parent { get; private set; }
        IReadOnlyCollection<IInterviewTreeNode> IInterviewTreeNode.Children { get; } = Enumerable.Empty<IInterviewTreeNode>().ToReadOnlyCollection();

        void IInternalInterviewTreeNode.SetTree(InterviewTree tree) => this.Tree = tree;
        void IInternalInterviewTreeNode.SetParent(IInterviewTreeNode parent) => this.Parent = parent;

        public bool IsDisabled() => this.isDisabled || (this.Parent?.IsDisabled() ?? false);
    }

    public abstract class InterviewTreeGroup : IInterviewTreeNode, IInternalInterviewTreeNode
    {
        private readonly bool isDisabled;
        private readonly List<IInterviewTreeNode> children;

        protected InterviewTreeGroup(Identity identity, IEnumerable<IInterviewTreeNode> children, bool isDisabled)
        {
            this.Identity = identity;
            this.children = children.ToList();
            this.isDisabled = isDisabled;

            foreach (var child in this.Children)
            {
                ((IInternalInterviewTreeNode)child).SetParent(this);
            }
        }

        public Identity Identity { get; }
        public InterviewTree Tree { get; private set; }
        public IInterviewTreeNode Parent { get; private set; }
        public IReadOnlyCollection<IInterviewTreeNode> Children => this.children;

        void IInternalInterviewTreeNode.SetTree(InterviewTree tree)
        {
            this.Tree = tree;

            foreach (var child in this.Children)
            {
                ((IInternalInterviewTreeNode)child).SetTree(tree);
            }
        }

        void IInternalInterviewTreeNode.SetParent(IInterviewTreeNode parent) => this.Parent = parent;

        public void AddChildren(IInterviewTreeNode child)
        {
            var internalTreeNode = child as IInternalInterviewTreeNode;
            if (internalTreeNode == null) throw new ArgumentException(nameof(child));

            internalTreeNode.SetTree(this.Tree);
            internalTreeNode.SetParent(this);
            this.children.Add(child);
        } 

        public void RemoveChildren(Identity identity)
        {
            var nodesToRemove = this.children.Where(x => x.Identity.Equals(identity)).ToArray();
            nodesToRemove.ForEach(nodeToRemove => this.children.Remove(nodeToRemove));
        }

        public void RemoveChildren(Identity[] identities)
        {
            foreach (var child in this.children.Where(x => identities.Contains(x.Identity)))
                this.children.Remove(child);
        }

        public bool IsDisabled() => this.isDisabled || (this.Parent?.IsDisabled() ?? false);

        public InterviewTreeQuestion GetQuestionFromThisOrUpperLevel(Guid questionId)
        {
            InterviewTreeQuestion question = null;
            IInterviewTreeNode group = this;
            while (question == null)
            {
                question = group.Children.FirstOrDefault(x => x.Identity.Id == questionId) as InterviewTreeQuestion;
                if (group is InterviewTreeSection)
                    break;
                group = group.Parent;
            }

            return question;
        }

        public bool HasChild(Identity identity)
        {
            return this.Children.Any(x => x.Identity.Equals(identity));
        }
    }

    public class InterviewTreeQuestion : InterviewTreeLeafNode
    {
        public InterviewTreeQuestion(Identity identity, bool isDisabled, string title, string variableName,
            QuestionType questionType, object answer,
            IEnumerable<RosterVector> linkedOptions, Identity cascadingParentQuestionIdentity, bool isYesNo, bool isDecimal)
            : base(identity, isDisabled)
        {
            this.Title = title;
            this.VariableName = variableName;

            if (questionType == QuestionType.SingleOption)
            {
                if (linkedOptions == null)
                    this.AsSingleOption = new InterviewTreeSingleOptionQuestion(answer);
                else
                    this.AsSingleLinkedOption = new InterviewTreeSingleLinkedOptionQuestion(linkedOptions, answer);
            }

            if (questionType == QuestionType.MultyOption)
            {
                if (isYesNo)
                    this.AsYesNo = new InterviewTreeYesNoQuestion(answer);
                else if (linkedOptions != null)
                    this.AsMultiLinkedOption = new InterviewTreeMultiLinkedOptionQuestion(linkedOptions, answer);
                else
                    this.AsMultiOption = new InterviewTreeMultiOptionQuestion(answer);
            }

            if(questionType == QuestionType.DateTime)
                this.AsDateTime = new InterviewTreeDateTimeQuestion(answer);

            if (questionType == QuestionType.GpsCoordinates)
                this.AsGps = new InterviewTreeGpsQuestion(answer);

            if (questionType == QuestionType.Multimedia)
                this.AsMultimedia = new InterviewTreeMultimediaQuestion(answer);

            if (questionType == QuestionType.Numeric)
            {
                if (isDecimal)
                    this.AsDouble = new InterviewTreeDoubleQuestion(answer);
                else
                    this.AsInteger = new InterviewTreeIntegerQuestion(answer);
            }

            if (questionType == QuestionType.QRBarcode)
                this.AsQRBarcode = new InterviewTreeQRBarcodeQuestion(answer);

            if (questionType == QuestionType.Text)
                this.AsText = new InterviewTreeTextQuestion(answer);

            if (questionType == QuestionType.TextList)
                this.AsTextList = new InterviewTreeTextListQuestion(answer);

            if (cascadingParentQuestionIdentity != null)
                this.AsCascading = new InterviewTreeCascadingQuestion(this, cascadingParentQuestionIdentity);
        }

        public InterviewTreeDoubleQuestion AsDouble { get; }
        public InterviewTreeTextListQuestion AsTextList { get; }
        public InterviewTreeTextQuestion AsText { get; }
        public InterviewTreeQRBarcodeQuestion AsQRBarcode { get; }
        public InterviewTreeIntegerQuestion AsInteger { get;}
        public InterviewTreeMultimediaQuestion AsMultimedia { get; }
        public InterviewTreeGpsQuestion AsGps { get; }
        public InterviewTreeDateTimeQuestion AsDateTime { get;}
        public InterviewTreeMultiOptionQuestion AsMultiOption { get; }
        public InterviewTreeMultiLinkedOptionQuestion AsMultiLinkedOption { get; }
        public InterviewTreeYesNoQuestion AsYesNo { get; }
        public InterviewTreeSingleLinkedOptionQuestion AsSingleLinkedOption { get;  }
        public InterviewTreeSingleOptionQuestion AsSingleOption { get; }
        public InterviewTreeLinkedQuestion AsLinked { get; }
        public InterviewTreeCascadingQuestion AsCascading { get; }

        public string Title { get; }
        public string VariableName { get; }
        
        public bool IsDouble => this.AsDouble != null;
        public bool IsInteger => this.AsInteger != null;
        public bool IsSingleOption => this.AsSingleOption != null;
        public bool IsMultiOption => this.AsMultiOption != null;
        public bool IsMultiLinkedOption => this.AsMultiLinkedOption != null;
        public bool IsSingleLinkedOption => this.AsSingleLinkedOption != null;
        public bool IsQRBarcode => this.AsQRBarcode != null;
        public bool IsText => this.AsText != null;
        public bool IsTextList => this.AsTextList != null;
        public bool IsYesNo => this.AsYesNo != null;
        public bool IsDateTime => this.AsDateTime != null;
        public bool IsGps => this.AsGps != null;
        public bool IsMultimedia => this.AsMultimedia != null;

        public bool IsLinked => this.AsLinked != null;
        public bool IsCascading => this.AsCascading != null;

        public string FormatForException() => $"'{this.Title} [{this.VariableName}] ({this.Identity})'";

        public override string ToString() => $"Question ({this.Identity}) '{this.Title}'";
    }

    public class InterviewTreeDateTimeQuestion
    {
        private DateTime? answer;
        public InterviewTreeDateTimeQuestion(object answer)
        {
            this.answer = (DateTime?)answer;
        }

        public bool IsAnswered => this.answer != null;
        public DateTime GetAnswer() => this.answer.Value;
        public void SetAnswer(DateTime answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeDateTimeQuestion question)
        {
            if (question?.answer == null || this.answer == null)
                return false;
            return question.answer.Equals(this.answer);
        }
    }

    public class InterviewTreeGpsQuestion
    {
        private GeoPosition answer;

        public InterviewTreeGpsQuestion(object answer)
        {
            this.answer = (GeoPosition)answer;
        }

        public bool IsAnswered => this.answer != null;
        public GeoPosition GetAnswer() => this.answer;
        public void SetAnswer(GeoPosition answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeGpsQuestion question)
        {
            if (question?.answer == null || this.answer == null)
                return false;
            return question.answer.Equals(this.answer);
        }
    }

    public class InterviewTreeMultimediaQuestion
    {
        private string answer;

        public InterviewTreeMultimediaQuestion(object answer)
        {
            this.answer = (string)answer;
        }

        public bool IsAnswered => this.answer != null;
        public string GetAnswer() => this.answer;
        public void SetAnswer(string answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeMultimediaQuestion question)
        {
            if (question?.answer == null || this.answer == null)
                return false;
            return question.answer.Equals(this.answer);
        }
    }

    public class InterviewTreeIntegerQuestion
    {
        private int? answer;

        public InterviewTreeIntegerQuestion(object answer)
        {
            this.answer = answer == null? (int?)null : Convert.ToInt32(answer);
        }

        public bool IsAnswered => this.answer != null;
        public int GetAnswer() => this.answer.Value;
        public void SetAnswer(int answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeIntegerQuestion question)
        {
            if (question?.answer == null || this.answer == null)
                return false;
            return question.answer.Equals(this.answer);
        }
    }

    public class InterviewTreeDoubleQuestion
    {
        private double? answer;

        public InterviewTreeDoubleQuestion(object answer)
        {
            this.answer = answer == null ? (double?)null : Convert.ToDouble(answer);
        }

        public bool IsAnswered => this.answer != null;
        public double GetAnswer() => this.answer.Value;
        public void SetAnswer(double answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeDoubleQuestion question)
        {
            if (question?.answer == null || this.answer == null)
                return false;
            return question.answer.Equals(this.answer);
        }
    }

    public class InterviewTreeQRBarcodeQuestion
    {
        private string answer;

        public InterviewTreeQRBarcodeQuestion(object answer)
        {
            this.answer = (string)answer;
        }

        public bool IsAnswered => this.answer != null;
        public string GetAnswer() => this.answer;
        public void SetAnswer(string answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;
        public bool EqualByAnswer(InterviewTreeQRBarcodeQuestion question)
        {
            if (question?.answer== null || this.answer == null)
                return false;
            return question.answer.Equals(this.answer);
        }
    }

    public class InterviewTreeTextQuestion
    {
        private string answer;

        public InterviewTreeTextQuestion(object answer)
        {
            this.answer = answer as string;
        }

        public bool IsAnswered => this.answer != null;
        public string GetAnswer() => this.answer;
        public void SetAnswer(string answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeTextQuestion question)
        {
            if (question?.answer == null || this.answer == null)
                return false;
            return question.answer.Equals(this.answer);
        }
    }

    public class InterviewTreeYesNoQuestion
    {
        private AnsweredYesNoOption[] answer;

        public InterviewTreeYesNoQuestion(object answer)
        {
            this.answer = (AnsweredYesNoOption[])answer;
        }

        public bool IsAnswered => this.answer != null;
        public AnsweredYesNoOption[] GetAnswer() => this.answer;
        public void SetAnswer(AnsweredYesNoOption[] answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeYesNoQuestion question)
        {
            if (question?.answer == null || this.answer == null)
                return false;
            return question.answer.SequenceEqual(this.answer);
        }
    }

    public class InterviewTreeTextListQuestion
    {
        private Tuple<decimal, string>[] answer;

        public InterviewTreeTextListQuestion(object answer)
        {
            this.answer = (Tuple<decimal, string>[])answer;
        }

        public bool IsAnswered => this.answer != null;
        public Tuple<decimal, string>[] GetAnswer() => this.answer;
        public void SetAnswer(Tuple<decimal, string>[] answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeTextListQuestion question)
        {
            if (question?.answer == null || this.answer == null)
                return false;
            return question.answer.SequenceEqual(this.answer);
        }
    }

    public class InterviewTreeSingleOptionQuestion
    {
        private int? answer;

        public InterviewTreeSingleOptionQuestion(object answer)
        {
            this.answer = this.answer == null? (int?)null : Convert.ToInt32(answer);
        }

        public bool IsAnswered => this.answer != null;

        public int GetAnswer() => this.answer.Value;
        public void SetAnswer(int answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeSingleOptionQuestion question)
        {
            if (question?.answer == null || this.answer == null)
                return false;
            return question.answer.Equals(this.answer);
        }
    }

    public class InterviewTreeMultiOptionQuestion
    {
        private decimal[] answer;

        public InterviewTreeMultiOptionQuestion(object answer)
        {
            this.answer = (decimal[])answer;
        }

        public bool IsAnswered => this.answer != null;
        public decimal[] GetAnswer() => this.answer;
        public void SetAnswer(decimal[] answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeMultiOptionQuestion question)
        {
            if (question?.answer == null || this.answer == null)
                return false;
            return question.answer.SequenceEqual(this.answer);
        }
    }

    public class InterviewTreeSingleLinkedOptionQuestion : InterviewTreeLinkedQuestion
    {
        private RosterVector answer;
        public InterviewTreeSingleLinkedOptionQuestion(IEnumerable<RosterVector> linkedOptions, object answer) : base(linkedOptions)
        {
            this.answer = (RosterVector)answer;
        }

        public bool IsAnswered => this.answer != null;
        public RosterVector GetAnswer() => this.answer;
        public void SetAnswer(RosterVector answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeSingleLinkedOptionQuestion question)
        {
            if (question?.answer == null || this.answer == null)
                return false;
            return question.answer.Equals(this.answer);
        }
    }

    public class InterviewTreeMultiLinkedOptionQuestion : InterviewTreeLinkedQuestion
    {
        private decimal[] answer;
        public InterviewTreeMultiLinkedOptionQuestion(IEnumerable<RosterVector> linkedOptions, object answer) : base(linkedOptions)
        {
            this.answer = (decimal[])answer;
        }

        public bool IsAnswered => this.answer != null;
        public decimal[] GetAnswer() => this.answer;
        public void SetAnswer(decimal[] answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeMultiLinkedOptionQuestion question)
        {
            if (question?.answer == null || this.answer == null)
                return false;
            return question.answer.Equals(this.answer);
        }
    }

    public abstract class InterviewTreeLinkedQuestion
    {
        protected InterviewTreeLinkedQuestion(IEnumerable<RosterVector> linkedOptions)
        {
            if (linkedOptions == null) throw new ArgumentNullException(nameof(linkedOptions));

            this.Options = linkedOptions.ToReadOnlyCollection();
        }

        public IReadOnlyCollection<RosterVector> Options { get; }
    }

    public class InterviewTreeCascadingQuestion
    {
        private readonly InterviewTreeQuestion question;
        private readonly Identity cascadingParentQuestionIdentity;

        public InterviewTreeCascadingQuestion(InterviewTreeQuestion question, Identity cascadingParentQuestionIdentity)
        {
            if (cascadingParentQuestionIdentity == null) throw new ArgumentNullException(nameof(cascadingParentQuestionIdentity));

            this.question = question;
            this.cascadingParentQuestionIdentity = cascadingParentQuestionIdentity;
        }

        private InterviewTree Tree => this.question.Tree;

        public InterviewTreeSingleOptionQuestion GetCascadingParentQuestion()
            => this.Tree.GetQuestion(this.cascadingParentQuestionIdentity).AsSingleOption;
    }

    public class InterviewTreeStaticText : InterviewTreeLeafNode
    {
        public InterviewTreeStaticText(Identity identity)
            : base(identity, false) {}

        public override string ToString() => $"Text ({this.Identity})";
    }

    public class InterviewTreeSubSection : InterviewTreeGroup
    {
        public InterviewTreeSubSection(Identity identity, IEnumerable<IInterviewTreeNode> children, bool isDisabled)
            : base(identity, children, isDisabled) {}

        public override string ToString()
            => $"SubSection ({this.Identity})" + Environment.NewLine
            + string.Join(Environment.NewLine, this.Children.Select(child => child.ToString().PrefixEachLine("  ")));
    }

    public class InterviewTreeSection : InterviewTreeGroup
    {
        public InterviewTreeSection(Identity identity, IEnumerable<IInterviewTreeNode> children, bool isDisabled)
            : base(identity, children, isDisabled) {}

        public override string ToString()
            => $"Section ({this.Identity})" + Environment.NewLine
            + string.Join(Environment.NewLine, this.Children.Select(child => child.ToString().PrefixEachLine("  ")));
    }

    public class InterviewTreeRoster : InterviewTreeGroup
    {
        public string RosterTitle { get; set; }

        public InterviewTreeRoster(Identity identity, IEnumerable<IInterviewTreeNode> children, bool isDisabled)
            : base(identity, children, isDisabled) {}

        public override string ToString()
            => $"Roster ({this.Identity})" + Environment.NewLine
            + string.Join(Environment.NewLine, this.Children.Select(child => child.ToString().PrefixEachLine("  ")));
    }
}
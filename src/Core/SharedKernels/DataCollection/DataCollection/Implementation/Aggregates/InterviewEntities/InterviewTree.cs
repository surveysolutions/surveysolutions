using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

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


        internal InterviewTreeGroup GetGroup(Identity identity) 
            => this
            .GetNodes<InterviewTreeGroup>()
            .SingleOrDefault(node => node.Identity == identity);

        internal InterviewTreeStaticText GetStaticText(Identity identity) 
            => this
            .GetNodes<InterviewTreeStaticText>()
            .Single(node => node.Identity == identity);

        public IReadOnlyCollection<InterviewTreeQuestion> FindQuestions(Guid questionId)
            => this
                .GetNodes<InterviewTreeQuestion>()
                .Where(node => node.Identity.Id == questionId)
                .ToReadOnlyCollection();

        public IReadOnlyCollection<InterviewTreeQuestion> FindQuestions()
            => this
                .GetNodes<InterviewTreeQuestion>()
                .ToReadOnlyCollection();

        public IReadOnlyCollection<InterviewTreeRoster> FindRosters()
            => this
                .GetNodes<InterviewTreeRoster>()
                .ToReadOnlyCollection();

        public IEnumerable<IInterviewTreeNode> FindEntity(Guid nodeId)
        {
            return this.GetNodes().Where(x => x.Identity.Id == nodeId);
        }

        private IEnumerable<TNode> GetNodes<TNode>() => this.GetNodes().OfType<TNode>();

        private IEnumerable<IInterviewTreeNode> GetNodes()
            => this.Sections.Cast<IInterviewTreeNode>().TreeToEnumerable(node => node.Children);

        public void RemoveNode(Identity identity)
        {
            foreach (var node in this.GetNodes().Where(x => x.Identity.Equals(identity)))
                ((InterviewTreeGroup)node.Parent)?.RemoveChildren(node.Identity);
        }

        public IReadOnlyCollection<InterviewTreeNodeDiff> Compare(InterviewTree changedTree)
        {
            var sourceNodes = this.GetNodes().ToList();
            var changedNodes = changedTree.GetNodes().ToList();

            var leftOuterJoin = from source in sourceNodes
                                join changed in changedNodes
                                    on source.Identity equals changed.Identity
                                    into temp
                                from changed in temp.DefaultIfEmpty()
                                select CreateInterviewTreeNodeDiff(source, changed);

            var rightOuterJoin = from changed in changedNodes
                                 join source in sourceNodes
                                     on changed.Identity equals source.Identity
                                     into temp
                                 from source in temp.DefaultIfEmpty()
                                 select CreateInterviewTreeNodeDiff(source, changed);

            var fullOuterJoin = leftOuterJoin.Concat(rightOuterJoin);

            return fullOuterJoin
                .DistinctBy(x => new {sourceIdentity = x.SourceNode?.Identity, changedIdentity = x.ChangedNode?.Identity})
                .Where(diff =>
                    IsExistingChanged(diff) ||
                    IsDisabledChanged(diff) ||
                    IsRosterTitleChanged(diff as InterviewTreeRosterDiff) ||
                    IsAnswerByQuestionChanged(diff as InterviewTreeQuestionDiff))
                .ToReadOnlyCollection();
        }

        private static InterviewTreeNodeDiff CreateInterviewTreeNodeDiff(IInterviewTreeNode source, IInterviewTreeNode changed)
        {
            if (source is InterviewTreeRoster || changed is InterviewTreeRoster)
            {
                return new InterviewTreeRosterDiff(source, changed);
            }
            else if (source is InterviewTreeSection || changed is InterviewTreeSection)
            {
                return new InterviewTreeGroupDiff(source, changed);
            }
            else if (source is InterviewTreeGroup || changed is InterviewTreeGroup)
            {
                return new InterviewTreeGroupDiff(source, changed);
            }
            else if (source is InterviewTreeQuestion || changed is InterviewTreeQuestion)
            {
                return new InterviewTreeQuestionDiff(source, changed);
            }
            else if (source is InterviewTreeStaticText || changed is InterviewTreeStaticText)
            {
                return new InterviewTreeStaticTextDiff(source, changed);
            }
            else if (source is InterviewTreeVariable || changed is InterviewTreeVariable)
            {
                return new InterviewTreeVariableDiff(source, changed);
            }

            return new InterviewTreeNodeDiff(source, changed);
        }

        public static bool HasChangesByAnswer(InterviewTreeQuestion sourceQuestion,
            InterviewTreeQuestion changedQuestion)
        {
            if (sourceQuestion.IsText) return !sourceQuestion.AsText.EqualByAnswer(changedQuestion.AsText);
            if (sourceQuestion.IsInteger) return !sourceQuestion.AsInteger.EqualByAnswer(changedQuestion.AsInteger);
            if (sourceQuestion.IsDouble) return !sourceQuestion.AsDouble.EqualByAnswer(changedQuestion.AsDouble);
            if (sourceQuestion.IsDateTime) return !sourceQuestion.AsDateTime.EqualByAnswer(changedQuestion.AsDateTime);
            if (sourceQuestion.IsMultimedia) return !sourceQuestion.AsMultimedia.EqualByAnswer(changedQuestion.AsMultimedia);
            if (sourceQuestion.IsQRBarcode) return !sourceQuestion.AsQRBarcode.EqualByAnswer(changedQuestion.AsQRBarcode);
            if (sourceQuestion.IsGps) return !sourceQuestion.AsGps.EqualByAnswer(changedQuestion.AsGps);
            if (sourceQuestion.IsSingleOption) return !sourceQuestion.AsSingleOption.EqualByAnswer(changedQuestion.AsSingleOption);
            if (sourceQuestion.IsSingleLinkedOption) return !sourceQuestion.AsSingleLinkedOption.EqualByAnswer(changedQuestion.AsSingleLinkedOption);
            if (sourceQuestion.IsMultiOption) return !sourceQuestion.AsMultiOption.EqualByAnswer(changedQuestion.AsMultiOption);
            if (sourceQuestion.IsMultiLinkedOption) return !sourceQuestion.AsMultiLinkedOption.EqualByAnswer(changedQuestion.AsMultiLinkedOption);
            if (sourceQuestion.IsYesNo) return !sourceQuestion.AsYesNo.EqualByAnswer(changedQuestion.AsYesNo);
            if (sourceQuestion.IsTextList) return !sourceQuestion.AsTextList.EqualByAnswer(changedQuestion.AsTextList);

            return false;
        }

        private static bool IsExistingChanged(InterviewTreeNodeDiff diffByNode) => (diffByNode.SourceNode == null && diffByNode.ChangedNode != null) ||
                                                                            (diffByNode.SourceNode != null && diffByNode.ChangedNode == null);

        private static bool IsDisabledChanged(InterviewTreeNodeDiff diffByNode) => ((diffByNode.SourceNode.IsDisabled() && !diffByNode.ChangedNode.IsDisabled()) ||
                                                                             (!diffByNode.SourceNode.IsDisabled() && diffByNode.ChangedNode.IsDisabled()));

        private static bool IsAnswerByQuestionChanged(InterviewTreeQuestionDiff diffByQuestion)
        {
            if (diffByQuestion == null) return false;

            if ((diffByQuestion.SourceNode.IsAnswered() && !diffByQuestion.ChangedNode.IsAnswered()) ||
                (!diffByQuestion.SourceNode.IsAnswered() && diffByQuestion.ChangedNode.IsAnswered())) return true;
            
            return HasChangesByAnswer(diffByQuestion.SourceNode, diffByQuestion.ChangedNode);
        }

        private static bool IsRosterTitleChanged(InterviewTreeRosterDiff diffByRoster)
        {
            if (diffByRoster == null) return false;

            return diffByRoster.ChangedNode != null &&
                   diffByRoster.SourceNode?.RosterTitle != diffByRoster.ChangedNode.RosterTitle;
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

        void Disable();
        void Enable();
    }

    public interface IInternalInterviewTreeNode
    {
        void SetTree(InterviewTree tree);
        void SetParent(IInterviewTreeNode parent);
    }

    public abstract class InterviewTreeLeafNode : IInterviewTreeNode, IInternalInterviewTreeNode
    {
        private bool isDisabled;

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

        public void Disable() => this.isDisabled = true;
        public void Enable() => this.isDisabled = false;
    }

    public abstract class InterviewTreeGroup : IInterviewTreeNode, IInternalInterviewTreeNode
    {
        private bool isDisabled;
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
        public void Disable() => this.isDisabled = true;
        public void Enable() => this.isDisabled = false;

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
            IEnumerable<RosterVector> linkedOptions, Identity cascadingParentQuestionIdentity, bool isYesNo, bool isDecimal, Guid? linkedSourceId = null, Identity commonParentRosterIdForLinkedQuestion = null)
            : base(identity, isDisabled)
        {
            this.Title = title;
            this.VariableName = variableName;

            if (questionType == QuestionType.SingleOption)
            {
                if (linkedSourceId.HasValue)
                {
                    this.AsSingleLinkedOption = new InterviewTreeSingleLinkedOptionQuestion(linkedOptions, answer, linkedSourceId.Value, commonParentRosterIdForLinkedQuestion);
                }
                else
                    this.AsSingleOption = new InterviewTreeSingleOptionQuestion(answer);
            }

            if (questionType == QuestionType.MultyOption)
            {
                if (isYesNo)
                    this.AsYesNo = new InterviewTreeYesNoQuestion(answer);
                else if (linkedSourceId.HasValue)
                {
                    this.AsMultiLinkedOption = new InterviewTreeMultiLinkedOptionQuestion(linkedOptions, answer, linkedSourceId.Value, commonParentRosterIdForLinkedQuestion);
                }
                else
                    this.AsMultiOption = new InterviewTreeMultiOptionQuestion(answer);
            }

            if (questionType == QuestionType.DateTime)
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
        public InterviewTreeIntegerQuestion AsInteger { get; }
        public InterviewTreeMultimediaQuestion AsMultimedia { get; }
        public InterviewTreeGpsQuestion AsGps { get; }
        public InterviewTreeDateTimeQuestion AsDateTime { get; }
        public InterviewTreeMultiOptionQuestion AsMultiOption { get; }
        public InterviewTreeMultiLinkedOptionQuestion AsMultiLinkedOption { get; }
        public InterviewTreeYesNoQuestion AsYesNo { get; }
        public InterviewTreeSingleLinkedOptionQuestion AsSingleLinkedOption { get; }
        public InterviewTreeSingleOptionQuestion AsSingleOption { get; }

        public InterviewTreeLinkedQuestion AsLinked => this.IsSingleLinkedOption ? (InterviewTreeLinkedQuestion)this.AsSingleLinkedOption : this.AsMultiLinkedOption;

        public InterviewTreeCascadingQuestion AsCascading { get; }

        public string Title { get; }
        public string VariableName { get; }

        public bool IsValid => !this.FailedValidations?.Any() ?? true;
        public IReadOnlyList<FailedValidationCondition> FailedValidations { get; }
        
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

        public bool IsLinked => (this.IsMultiLinkedOption || this.IsSingleLinkedOption);
        public bool IsCascading => this.AsCascading != null;

        public bool IsAnswered()
        {
            if (this.IsText) return this.AsText.IsAnswered;
            if (this.IsInteger) return this.AsInteger.IsAnswered;
            if (this.IsDouble) return this.AsDouble.IsAnswered;
            if (this.IsDateTime) return this.AsDateTime.IsAnswered;
            if (this.IsMultimedia) return this.AsMultimedia.IsAnswered;
            if (this.IsQRBarcode) return this.AsQRBarcode.IsAnswered;
            if (this.IsGps) return this.AsGps.IsAnswered;
            if (this.IsSingleOption) return this.AsSingleOption.IsAnswered;
            if (this.IsSingleLinkedOption) return this.AsSingleLinkedOption.IsAnswered;
            if (this.IsMultiOption) return this.AsMultiOption.IsAnswered;
            if (this.IsMultiLinkedOption) return this.AsMultiLinkedOption.IsAnswered;
            if (this.IsYesNo) return this.AsYesNo.IsAnswered;
            if (this.IsTextList) return this.AsTextList.IsAnswered;

            return false;
        }

        public string FormatForException() => $"'{this.Title} [{this.VariableName}] ({this.Identity})'";

        public override string ToString() => $"Question ({this.Identity}) '{this.Title}'";

        public void UpdateLinkedOptions()
        {
            if (!IsLinked) return;

            InterviewTreeLinkedQuestion linkedQuestion = this.AsLinked;

            List<IInterviewTreeNode> sourceNodes = new List<IInterviewTreeNode>();
            if (linkedQuestion.CommonParentRosterIdForLinkedQuestion == null)
            {
                sourceNodes = this.Tree.FindEntity(linkedQuestion.LinkedSourceId).ToList();
            }
            else
            {
                var parentGroup = this.Tree.GetGroup(linkedQuestion.CommonParentRosterIdForLinkedQuestion);
                if (parentGroup !=null)
                    sourceNodes = parentGroup
                        .TreeToEnumerable<IInterviewTreeNode>(node => node.Children)
                        .Where(x => x.Identity.Id == linkedQuestion.LinkedSourceId)
                        .ToList();
            }

            var options = sourceNodes.Where(x => !x.IsDisabled()).Select(x => x.Identity.RosterVector);
            linkedQuestion.SetOptions(options);

        }
    }

    public class InterviewTreeDateTimeQuestion
    {
        private DateTime? answer;
        public InterviewTreeDateTimeQuestion(object answer)
        {
            this.answer = answer == null ? (DateTime?)null : Convert.ToDateTime(answer);
        }

        public bool IsAnswered => this.answer != null;
        public DateTime GetAnswer() => this.answer.Value;
        public void SetAnswer(DateTime answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeDateTimeQuestion question) => question?.answer == this.answer;
    }

    public class InterviewTreeGpsQuestion
    {
        private GeoPosition answer;

        public InterviewTreeGpsQuestion(object answer)
        {
            this.answer = answer as GeoPosition;
        }

        public bool IsAnswered => this.answer != null;
        public GeoPosition GetAnswer() => this.answer;
        public void SetAnswer(GeoPosition answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeGpsQuestion question) => question?.answer == this.answer;
    }

    public class InterviewTreeMultimediaQuestion
    {
        private string answer;

        public InterviewTreeMultimediaQuestion(object answer)
        {
            this.answer = answer as string;
        }

        public bool IsAnswered => this.answer != null;
        public string GetAnswer() => this.answer;
        public void SetAnswer(string answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeMultimediaQuestion question) => question?.answer == this.answer;
    }

    public class InterviewTreeIntegerQuestion
    {
        private int? answer;

        public InterviewTreeIntegerQuestion(object answer)
        {
            this.answer = answer == null ? (int?)null : Convert.ToInt32(answer);
        }

        public bool IsAnswered => this.answer != null;
        public int GetAnswer() => this.answer.Value;
        public void SetAnswer(int answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeIntegerQuestion question) => question?.answer == this.answer;
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

        public bool EqualByAnswer(InterviewTreeDoubleQuestion question) => question?.answer == this.answer;
    }

    public class InterviewTreeQRBarcodeQuestion
    {
        private string answer;

        public InterviewTreeQRBarcodeQuestion(object answer)
        {
            this.answer = answer as string;
        }

        public bool IsAnswered => this.answer != null;
        public string GetAnswer() => this.answer;
        public void SetAnswer(string answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;
        public bool EqualByAnswer(InterviewTreeQRBarcodeQuestion question) => question?.answer == this.answer;
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

        public bool EqualByAnswer(InterviewTreeTextQuestion question) => question?.answer == this.answer;
    }

    public class InterviewTreeYesNoQuestion
    {
        private AnsweredYesNoOption[] answer;

        public InterviewTreeYesNoQuestion(object answer)
        {
            this.answer = answer as AnsweredYesNoOption[];
        }

        public bool IsAnswered => this.answer != null;
        public AnsweredYesNoOption[] GetAnswer() => this.answer;
        public void SetAnswer(AnsweredYesNoOption[] answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeYesNoQuestion question)
        {
            if (question?.answer == null && this.answer == null)
                return true;

            if (question?.answer != null && this.answer != null)
                return question.answer.SequenceEqual(this.answer);

            return false;
        }

        public string GetOptionTitle(decimal optionCode)
        {
            return string.Empty;
        }
    }

    public class InterviewTreeTextListQuestion
    {
        private Tuple<decimal, string>[] answer;

        public InterviewTreeTextListQuestion(object answer)
        {
            this.answer = answer as Tuple<decimal, string>[];
        }

        public bool IsAnswered => this.answer != null;
        public Tuple<decimal, string>[] GetAnswer() => this.answer;
        public void SetAnswer(Tuple<decimal, string>[] answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeTextListQuestion question)
        {
            if (question?.answer == null && this.answer == null)
                return true;

            if (question?.answer != null && this.answer != null)
                return question.answer.SequenceEqual(this.answer);

            return false;
        }

        public string GetTitleByItemCode(decimal code)
        {
            if (!IsAnswered)
                return string.Empty;
            return this.answer.Single(x => x.Item1 == code).Item2;
        }
    }

    public class InterviewTreeSingleOptionQuestion
    {
        private int? answer;

        public InterviewTreeSingleOptionQuestion(object answer)
        {
            this.answer = answer == null ? (int?)null : Convert.ToInt32(answer);
        }

        public bool IsAnswered => this.answer != null;

        public int GetAnswer() => this.answer.Value;
        public void SetAnswer(int answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeSingleOptionQuestion question) => question?.answer == this.answer;
    }

    public class InterviewTreeMultiOptionQuestion
    {
        private decimal[] answer;

        public InterviewTreeMultiOptionQuestion(object answer)
        {
            this.answer = answer as decimal[];
        }

        public bool IsAnswered => this.answer != null;
        public decimal[] GetAnswer() => this.answer;
        public void SetAnswer(decimal[] answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeMultiOptionQuestion question)
        {
            if (question?.answer == null && this.answer == null)
                return true;

            if (question?.answer != null && this.answer != null)
                return question.answer.SequenceEqual(this.answer);

            return false;
        }

        internal void SetAnswer(int[] intValues)
        {
            SetAnswer((intValues ?? new int[0]).Select(Convert.ToDecimal).ToArray());
        }
    }

    public class InterviewTreeSingleLinkedOptionQuestion : InterviewTreeLinkedQuestion
    {
        private RosterVector answer;
        public InterviewTreeSingleLinkedOptionQuestion(IEnumerable<RosterVector> linkedOptions, object answer, Guid linkedSourceId, Identity commonParentRosterIdForLinkedQuestion)
            : base(linkedOptions, linkedSourceId, commonParentRosterIdForLinkedQuestion)
        {
            this.answer = answer as RosterVector;
        }

        public bool IsAnswered => this.answer != null;
        public RosterVector GetAnswer() => this.answer;
        public void SetAnswer(RosterVector answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeSingleLinkedOptionQuestion question) => question?.answer == this.answer;
    }

    public class InterviewTreeMultiLinkedOptionQuestion : InterviewTreeLinkedQuestion
    {
        private decimal[][] answer;
        public InterviewTreeMultiLinkedOptionQuestion(IEnumerable<RosterVector> linkedOptions, object answer, Guid linkedSourceId, Identity commonParentRosterIdForLinkedQuestion) 
            : base(linkedOptions, linkedSourceId, commonParentRosterIdForLinkedQuestion)
        {
            this.answer = answer as decimal[][];
        }

        public bool IsAnswered => this.answer != null;
        public decimal[][] GetAnswer() => this.answer;
        public void SetAnswer(decimal[][] answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeMultiLinkedOptionQuestion question)
        {
            if (question?.answer == null && this.answer == null)
                return true;

            if (question?.answer != null && this.answer != null)
                return question.answer.SelectMany(x => x).SequenceEqual(this.answer.SelectMany(x => x));

            return false;
        }
    }

    public abstract class InterviewTreeLinkedQuestion
    {
        public Guid LinkedSourceId { get; private set; }
        public Identity CommonParentRosterIdForLinkedQuestion { get; private set; }

        protected InterviewTreeLinkedQuestion(IEnumerable<RosterVector> linkedOptions, Guid linkedSourceId, Identity commonParentRosterIdForLinkedQuestion)
        {
            this.LinkedSourceId = linkedSourceId;
            this.CommonParentRosterIdForLinkedQuestion = commonParentRosterIdForLinkedQuestion;
            //Interview state returns null if linked question has no options
            // if (linkedOptions == null) throw new ArgumentNullException(nameof(linkedOptions));

            this.Options = linkedOptions?.ToList() ?? new List<RosterVector>();
        }

        public List<RosterVector> Options { get; private set; }

        public void SetOptions(IEnumerable<RosterVector> options)
        {
            this.Options = options.ToList();
        }
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

    public class InterviewTreeVariable : InterviewTreeLeafNode
    {
        public InterviewTreeVariable(Identity identity, bool isDisabled)
            : base(identity, isDisabled) { }

        public override string ToString() => $"Variable ({this.Identity})";
    }

    public class InterviewTreeStaticText : InterviewTreeLeafNode
    {
        public InterviewTreeStaticText(Identity identity)
            : base(identity, false) { }

        public bool IsValid => !this.FailedValidations?.Any() ?? true;
        public IReadOnlyList<FailedValidationCondition> FailedValidations { get; }

        public override string ToString() => $"Text ({this.Identity})";
    }

    public class InterviewTreeSubSection : InterviewTreeGroup
    {
        public InterviewTreeSubSection(Identity identity, IEnumerable<IInterviewTreeNode> children, bool isDisabled)
            : base(identity, children, isDisabled) { }

        public override string ToString()
            => $"SubSection ({this.Identity})" + Environment.NewLine
            + string.Join(Environment.NewLine, this.Children.Select(child => child.ToString().PrefixEachLine("  ")));
    }

    public class InterviewTreeSection : InterviewTreeGroup
    {
        public InterviewTreeSection(Identity identity, IEnumerable<IInterviewTreeNode> children, bool isDisabled)
            : base(identity, children, isDisabled) { }

        public override string ToString()
            => $"Section ({this.Identity})" + Environment.NewLine
            + string.Join(Environment.NewLine, this.Children.Select(child => child.ToString().PrefixEachLine("  ")));
    }

    public enum RosterType
    {
        Fixed = 1,
        Numeric = 2,
        YesNo = 3,
        Multi = 4,
        List = 5
    }

    public class InterviewTreeRoster : InterviewTreeGroup
    {
        public InterviewTreeRoster(Identity identity,
            IEnumerable<IInterviewTreeNode> children,
            bool isDisabled = false,
            string rosterTitle = null,
            int sortIndex = 0,
            RosterType rosterType = RosterType.Fixed,
            InterviewTreeQuestion rosterSizeQuestion = null,
            Identity rosterTitleQuestionIdentity = null)
            : base(identity, children, isDisabled)
        {
            RosterTitle = rosterTitle;
            SortIndex = sortIndex;
            this.RosterSizeQuestion = rosterSizeQuestion;

            switch (rosterType)
            {
                case RosterType.Fixed:
                    AsFixed = new InterviewTreeFixedRoster();
                    break;
                case RosterType.Numeric:
                    AsNumeric = new InterviewTreeNumericRoster(rosterSizeQuestion, rosterTitleQuestionIdentity);
                    break;
                case RosterType.YesNo:
                    AsYesNo = new InterviewTreeYesNoRoster(rosterSizeQuestion);
                    break;
                case RosterType.Multi:
                    AsMulti = new InterviewTreeMultiRoster(rosterSizeQuestion);
                    break;
                case RosterType.List:
                    AsList = new InterviewTreeListRoster(rosterSizeQuestion);
                    break;
            }
        }

        public string RosterTitle { get; set; }
        public int SortIndex { get; set; } = 0;
        public InterviewTreeQuestion RosterSizeQuestion { get; private set; }

        public InterviewTreeNumericRoster AsNumeric { get; }
        public InterviewTreeListRoster AsList { get; }
        public InterviewTreeYesNoRoster AsYesNo { get; }
        public InterviewTreeMultiRoster AsMulti { get; }
        public InterviewTreeFixedRoster AsFixed { get; }

        public bool IsNumeric => this.AsNumeric != null;
        public bool IsList => this.AsList != null;
        public bool IsYesNo => this.AsYesNo != null;
        public bool IsMulti => this.AsMulti != null;
        public bool IsFixed => this.AsFixed != null;

        public override string ToString()
            => $"Roster ({this.Identity}) [{RosterTitle}]" + Environment.NewLine
            + string.Join(Environment.NewLine, this.Children.Select(child => child.ToString().PrefixEachLine("  ")));

        public void UpadateTitle()
        {
            if (IsNumeric)
            {
                if (AsNumeric.HasTitleQuestion)
                {
                    var titleQuestion = Tree.GetQuestion(AsNumeric.RosterTitleQuestionIdentity);
                    // get answer and format is as string and set as RosterTitle
                }
            }
            else if (IsList)
            {
                var sizeQuestion = AsList.RosterSizeQuestion;
                this.RosterTitle = sizeQuestion.AsTextList.GetTitleByItemCode(Identity.RosterVector.Last());
            }
        }
    }

    public class InterviewTreeFixedRoster
    {
    }

    public class InterviewTreeMultiRoster
    {
        public InterviewTreeQuestion RosterSizeQuestion { get; set; }

        public InterviewTreeMultiRoster(InterviewTreeQuestion rosterSizeQuestion)
        {
            this.RosterSizeQuestion = rosterSizeQuestion;
        }
    }

    public class InterviewTreeYesNoRoster
    {
        public InterviewTreeQuestion RosterSizeQuestion { get; set; }

        public InterviewTreeYesNoRoster(InterviewTreeQuestion rosterSizeQuestion)
        {
            this.RosterSizeQuestion = rosterSizeQuestion;
        }
    }

    public class InterviewTreeNumericRoster
    {
        public InterviewTreeQuestion RosterSizeQuestion { get; set; }
        public Identity RosterTitleQuestionIdentity { get; set; }
        public bool HasTitleQuestion => RosterTitleQuestionIdentity != null;

        public InterviewTreeNumericRoster(InterviewTreeQuestion rosterSizeQuestion, Identity rosterTitleQuestionIdentity)
        {
            this.RosterSizeQuestion = rosterSizeQuestion;
            this.RosterTitleQuestionIdentity = rosterTitleQuestionIdentity;
        }
    }

    public class InterviewTreeListRoster
    {
        public InterviewTreeQuestion RosterSizeQuestion { get; set; }

        public InterviewTreeListRoster(InterviewTreeQuestion rosterSizeQuestion)
        {
            this.RosterSizeQuestion = rosterSizeQuestion;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeQuestion : InterviewTreeLeafNode
    {
        public InterviewTreeQuestion(Identity identity, bool isDisabled, string title, string variableName,
            QuestionType questionType, object answer,
            IEnumerable<RosterVector> linkedOptions, Identity cascadingParentQuestionIdentity, bool isYesNo, bool isDecimal, Guid? linkedSourceId = null, 
            Identity commonParentRosterIdForLinkedQuestion = null)
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

        public InterviewTreeDoubleQuestion AsDouble { get; private set; }
        public InterviewTreeTextListQuestion AsTextList { get; private set; }
        public InterviewTreeTextQuestion AsText { get; private set; }
        public InterviewTreeQRBarcodeQuestion AsQRBarcode { get; private set; }
        public InterviewTreeIntegerQuestion AsInteger { get; private set; }
        public InterviewTreeMultimediaQuestion AsMultimedia { get; private set; }
        public InterviewTreeGpsQuestion AsGps { get; private set; }
        public InterviewTreeDateTimeQuestion AsDateTime { get; private set; }
        public InterviewTreeMultiOptionQuestion AsMultiOption { get; private set; }
        public InterviewTreeMultiLinkedOptionQuestion AsMultiLinkedOption { get; private set; }
        public InterviewTreeYesNoQuestion AsYesNo { get; private set; }
        public InterviewTreeSingleLinkedOptionQuestion AsSingleLinkedOption { get; private set; }
        public InterviewTreeSingleOptionQuestion AsSingleOption { get; private set; }

        public InterviewTreeLinkedQuestion AsLinked => this.IsSingleLinkedOption ? (InterviewTreeLinkedQuestion)this.AsSingleLinkedOption : this.AsMultiLinkedOption;

        public InterviewTreeCascadingQuestion AsCascading { get; private set; }

        public string Title { get; }
        public string VariableName { get; }

        public bool IsValid => !this.FailedValidations?.Any() ?? true;
        public IReadOnlyList<FailedValidationCondition> FailedValidations { get; private set; }

        public void MarkAsInvalid(IEnumerable<FailedValidationCondition> failedValidations)
        {
            if(failedValidations == null) throw new ArgumentNullException(nameof(failedValidations));
            this.FailedValidations = failedValidations.ToReadOnlyCollection();
        }

        public void MarkAsValid()
            => this.FailedValidations = Enumerable.Empty<FailedValidationCondition>().ToList();

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

        public void CalculateLinkedOptions()
        {
            if (!this.IsLinked) return;

            InterviewTreeLinkedQuestion linkedQuestion = this.AsLinked;

            List<IInterviewTreeNode> sourceNodes = new List<IInterviewTreeNode>();
            var isQuestionOnTopLevelOrInDifferentRosterBranch = linkedQuestion.CommonParentRosterIdForLinkedQuestion == null;
            if (isQuestionOnTopLevelOrInDifferentRosterBranch)
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

            var options = sourceNodes
                .Where(x => !x.IsDisabled())
                .Where(x => ((x as InterviewTreeQuestion)?.IsAnswered() ?? true))
                .Select(x => x.Identity.RosterVector).ToArray();
            this.UpdateLinkedOptionsAndResetAnswerIfNeeded(options);
        }

        public void SetAnswer(object answer)
        {
            if (this.IsText) { this.AsText.SetAnswer(answer as string); return; }
            if (this.IsInteger) { this.AsInteger.SetAnswer(Convert.ToInt32(answer)); return; }
            if (this.IsDouble) { this.AsDouble.SetAnswer(Convert.ToDouble(answer)); return; }
            if (this.IsDateTime) { this.AsDateTime.SetAnswer((DateTime)answer); return; }
            if (this.IsMultimedia) { this.AsMultimedia.SetAnswer(answer as string); return; }
            if (this.IsQRBarcode) { this.AsQRBarcode.SetAnswer(answer as string); return; }
            if (this.IsGps) { this.AsGps.SetAnswer((GeoPosition)answer); return; }
            if (this.IsSingleOption) { this.AsSingleOption.SetAnswer(Convert.ToInt32(answer)); return; }
            if (this.IsMultiOption) { this.AsMultiOption.SetAnswer((decimal[])answer); return; }
            if (this.IsSingleLinkedOption) { this.AsSingleLinkedOption.SetAnswer((RosterVector)answer); return; }
            if (this.IsMultiLinkedOption) { this.AsMultiLinkedOption.SetAnswer((decimal[][])answer); return; }
            if (this.IsYesNo) { this.AsYesNo.SetAnswer((AnsweredYesNoOption[])answer); return; }
            if (this.IsTextList) { this.AsTextList.SetAnswer((Tuple<decimal, string>[])answer); return; }
        }

        public string GetAnswerAsString(Func<decimal, string> getCategoricalAnswerOptionText = null)
        {
            if (this.IsText) return this.AsText.GetAnswer();
            if (this.IsMultimedia) return this.AsMultimedia.GetAnswer();
            if (this.IsQRBarcode) return this.AsQRBarcode.GetAnswer();
            if (this.IsInteger) return AnswerUtils.AnswerToString(this.AsInteger.GetAnswer());
            if (this.IsDouble) return AnswerUtils.AnswerToString(this.AsDouble.GetAnswer());
            if (this.IsDateTime) return AnswerUtils.AnswerToString(this.AsDateTime.GetAnswer());
            if (this.IsGps) return AnswerUtils.AnswerToString(this.AsGps.GetAnswer());
            if (this.IsTextList) return AnswerUtils.AnswerToString(this.AsTextList.GetAnswer());

            if (this.IsSingleLinkedOption)
            {
                var linkedQuestion = new Identity(this.AsSingleLinkedOption.LinkedSourceId, this.AsSingleLinkedOption.GetAnswer());
                return this.Tree.GetQuestion(linkedQuestion).GetAnswerAsString(getCategoricalAnswerOptionText);
            }
            if (this.IsMultiLinkedOption)
            {
                var formattedAnswers = this.AsMultiLinkedOption.GetAnswer()
                    .Select(x => new Identity(this.AsMultiLinkedOption.LinkedSourceId, x))
                    .Select(x => this.Tree.GetQuestion(x).GetAnswerAsString(getCategoricalAnswerOptionText));
                return string.Join(", ", formattedAnswers);
            }

            if (this.IsSingleOption) return AnswerUtils.AnswerToString(Convert.ToDecimal(this.AsSingleOption.GetAnswer()), getCategoricalAnswerOptionText);
            if (this.IsMultiOption) return AnswerUtils.AnswerToString(this.AsMultiOption.GetAnswer(), getCategoricalAnswerOptionText);
            if (this.IsYesNo) return AnswerUtils.AnswerToString(this.AsYesNo.GetAnswer(), getCategoricalAnswerOptionText);
            return string.Empty;
        }

        public void UpdateLinkedOptionsAndResetAnswerIfNeeded(RosterVector[] options)
        {
            if (!this.IsLinked) return;
            var previousOptions = this.AsLinked.Options;
            this.AsLinked.SetOptions(options);

            var optionsAreIdentical = previousOptions.SequenceEqual(options);
            if (optionsAreIdentical) return;

            if (this.IsMultiLinkedOption)
                this.AsMultiLinkedOption.RemoveAnswer();
            else
                this.AsSingleLinkedOption.RemoveAnswer();
        }

        public void RemoveAnswer()
        {
            if (this.IsText) this.AsText.RemoveAnswer();
            if (this.IsInteger) this.AsInteger.RemoveAnswer();
            if (this.IsDouble) this.AsDouble.RemoveAnswer();
            if (this.IsDateTime) this.AsDateTime.RemoveAnswer();
            if (this.IsMultimedia) this.AsMultimedia.RemoveAnswer();
            if (this.IsQRBarcode) this.AsQRBarcode.RemoveAnswer();
            if (this.IsGps) this.AsGps.RemoveAnswer();
            if (this.IsSingleOption) this.AsSingleOption.RemoveAnswer();
            if (this.IsSingleLinkedOption) this.AsSingleLinkedOption.RemoveAnswer();
            if (this.IsMultiOption)  this.AsMultiOption.RemoveAnswer();
            if (this.IsMultiLinkedOption) this.AsMultiLinkedOption.RemoveAnswer();
            if (this.IsYesNo) this.AsYesNo.RemoveAnswer();
            if (this.IsTextList)  this.AsTextList.RemoveAnswer();
        }

        public bool IsOnTheSameOrDeeperLevel(Identity questionIdentity)
        {
            var rosterLevel = questionIdentity.RosterVector.Length;
            return this.Identity.RosterVector.Take(rosterLevel).SequenceEqual(questionIdentity.RosterVector);
        }

        public override IInterviewTreeNode Clone()
        {
            var clonedQuestion = (InterviewTreeQuestion) this.MemberwiseClone();
            if (this.IsDateTime) clonedQuestion.AsDateTime = new InterviewTreeDateTimeQuestion(this.AsDateTime.IsAnswered ? this.AsDateTime.GetAnswer() : (object) null);
            if (this.IsDouble) clonedQuestion.AsDouble = new InterviewTreeDoubleQuestion(this.AsDouble.IsAnswered ? this.AsDouble.GetAnswer() : (object)null);
            if (this.IsGps) clonedQuestion.AsGps = new InterviewTreeGpsQuestion(this.AsGps.IsAnswered ? this.AsGps.GetAnswer() : (object)null);
            if (this.IsMultiOption) clonedQuestion.AsMultiOption = new InterviewTreeMultiOptionQuestion(this.AsMultiOption.IsAnswered ? this.AsMultiOption.GetAnswer() : (object)null);
            if (this.IsMultimedia) clonedQuestion.AsMultimedia = new InterviewTreeMultimediaQuestion(this.AsMultimedia.IsAnswered ? this.AsMultimedia.GetAnswer() : (object)null);
            if (this.IsQRBarcode) clonedQuestion.AsQRBarcode = new InterviewTreeQRBarcodeQuestion(this.AsQRBarcode.IsAnswered ? this.AsQRBarcode.GetAnswer() : (object)null);
            if (this.IsSingleOption) clonedQuestion.AsSingleOption = new InterviewTreeSingleOptionQuestion(this.AsSingleOption.IsAnswered ? this.AsSingleOption.GetAnswer() : (object)null);
            if (this.IsText) clonedQuestion.AsText = new InterviewTreeTextQuestion(this.AsText.IsAnswered ? this.AsText.GetAnswer() : (object)null);
            if (this.IsTextList) clonedQuestion.AsTextList = new InterviewTreeTextListQuestion(this.AsTextList.IsAnswered ? this.AsTextList.GetAnswer() : (object)null);
            if (this.IsYesNo) clonedQuestion.AsYesNo = new InterviewTreeYesNoQuestion(this.AsYesNo.IsAnswered ? this.AsYesNo.GetAnswer() : (object)null);
            if (this.IsInteger) clonedQuestion.AsInteger = new InterviewTreeIntegerQuestion(this.AsInteger.IsAnswered ? this.AsInteger.GetAnswer() : (object)null);

            if (this.IsMultiLinkedOption) clonedQuestion.AsMultiLinkedOption = new InterviewTreeMultiLinkedOptionQuestion(
                this.AsMultiLinkedOption.Options,
                this.AsMultiLinkedOption.GetAnswer(),
                this.AsMultiLinkedOption.LinkedSourceId,
                this.AsMultiLinkedOption.CommonParentRosterIdForLinkedQuestion);
            if (this.IsSingleLinkedOption) clonedQuestion.AsSingleLinkedOption = new InterviewTreeSingleLinkedOptionQuestion(
                this.AsSingleLinkedOption.Options,
                this.AsSingleLinkedOption.GetAnswer(),
                this.AsSingleLinkedOption.LinkedSourceId,
                this.AsSingleLinkedOption.CommonParentRosterIdForLinkedQuestion);
            if (this.IsCascading) clonedQuestion.AsCascading = new InterviewTreeCascadingQuestion(
                clonedQuestion, 
                this.AsCascading.CascadingParentQuestionIdentity);

            clonedQuestion.FailedValidations = this.FailedValidations?
                .Select(v => new FailedValidationCondition(v.FailedConditionIndex))
                .ToReadOnlyCollection();

            return clonedQuestion;
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

        public void SetAnswer(YesNoAnswersOnly yesNoAnswersOnly)
        {
            var yesNoOptionsList = new List<AnsweredYesNoOption>();
            yesNoOptionsList.AddRange(yesNoAnswersOnly.Yes.Select(yesOption => new AnsweredYesNoOption(yesOption, true)));
            yesNoOptionsList.AddRange(yesNoAnswersOnly.No.Select(noOption => new AnsweredYesNoOption(noOption, false)));
            this.answer = yesNoOptionsList.ToArray();
        }

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
            if (answer != null && answer is RosterVector[])
                this.answer = (answer as RosterVector[]).Select(x => x.Coordinates.ToArray()).ToArray();
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

        public Identity CascadingParentQuestionIdentity => this.cascadingParentQuestionIdentity;
    }

}
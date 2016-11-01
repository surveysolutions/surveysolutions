using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeQuestion : InterviewTreeLeafNode
    {
        public InterviewTreeQuestion(Identity identity, 
            bool isDisabled, 
            string title, 
            string variableName,
            QuestionType questionType, 
            object answer,
            IEnumerable<RosterVector> linkedOptions, 
            Guid? cascadingParentQuestionId, 
            bool isYesNo, 
            bool isDecimal, 
            Guid? linkedSourceId = null, 
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

            if (cascadingParentQuestionId.HasValue)
                this.AsCascading = new InterviewTreeCascadingQuestion(this, cascadingParentQuestionId.Value);
        }

        public InterviewTreeQuestion(Identity questionIdentity) : base(questionIdentity, false)
        {
            
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

        public void SetAnswer(AbstractAnswer answer)
        {
            if (answer == null)
            {
                this.RemoveAnswer();
            }
            else
            {
                this.AsText?.SetAnswer(((TextAnswer)answer).Value);
                this.AsInteger?.SetAnswer(((NumericIntegerAnswer)answer).Value);
                this.AsDouble?.SetAnswer(((NumericRealAnswer)answer).Value);
                this.AsDateTime?.SetAnswer(((DateTimeAnswer)answer).Value);
                this.AsMultimedia?.SetAnswer(((MultimediaAnswer)answer).FileName);
                this.AsQRBarcode?.SetAnswer(((QRBarcodeAnswer)answer).DecodedText);
                this.AsGps?.SetAnswer(((GpsAnswer)answer).Value);
                this.AsSingleOption?.SetAnswer(((CategoricalFixedSingleOptionAnswer)answer).SelectedValue);
                this.AsSingleLinkedOption?.SetAnswer(((CategoricalLinkedSingleOptionAnswer)answer).SelectedValue);
                this.AsMultiOption?.SetAnswer(((CategoricalFixedMultiOptionAnswer)answer).CheckedValues);
                this.AsMultiLinkedOption?.SetAnswer(((CategoricalLinkedMultiOptionAnswer)answer).CheckedValues);
                this.AsYesNo?.SetAnswer(((YesNoAnswer)answer).ToAnsweredYesNoOptions());
                this.AsTextList?.SetAnswer(((TextListAnswer)answer).ToTupleArray());
            }
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
                .Where(x => (x as InterviewTreeQuestion)?.IsAnswered() ?? true)
                .Select(x => x.Identity.RosterVector).ToArray();
            this.UpdateLinkedOptionsAndResetAnswerIfNeeded(options);
        }

        [Obsolete("use SetAnswer instead")]
        public void SetObjectAnswer(object answer)
        {
            if (this.IsText) { this.AsText.SetAnswer(answer as string); return; }
            if (this.IsInteger) { this.AsInteger.SetAnswer(Convert.ToInt32(answer)); return; }
            if (this.IsDouble) { this.AsDouble.SetAnswer(Convert.ToDouble(answer)); return; }
            if (this.IsDateTime) { this.AsDateTime.SetAnswer((DateTime)answer); return; }
            if (this.IsMultimedia) { this.AsMultimedia.SetAnswer(answer as string); return; }
            if (this.IsQRBarcode) { this.AsQRBarcode.SetAnswer(answer as string); return; }
            if (this.IsGps) { this.AsGps.SetAnswer((GeoPosition)answer); return; }
            if (this.IsSingleOption) { this.AsSingleOption.SetAnswer(Convert.ToInt32(answer)); return; }
            if (this.IsMultiOption)
            {
                if (answer is RosterVector)
                    this.AsMultiOption.SetAnswer(answer as RosterVector);
                else if (answer is decimal[])
                    this.AsMultiOption.SetAnswer(answer as decimal[]);
                return;
            }
            if (this.IsSingleLinkedOption) { this.AsSingleLinkedOption.SetAnswer((RosterVector)answer); return; }
            if (this.IsMultiLinkedOption)
            {
                if (answer is RosterVector[])
                    this.AsMultiLinkedOption.SetAnswer((RosterVector[])answer);
                else if (answer is decimal[][])
                    this.AsMultiLinkedOption.SetAnswer((decimal[][])answer);
                return;
            }
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
            this.AsText?.RemoveAnswer();
            this.AsInteger?.RemoveAnswer();
            this.AsDouble?.RemoveAnswer();
            this.AsDateTime?.RemoveAnswer();
            this.AsMultimedia?.RemoveAnswer();
            this.AsQRBarcode?.RemoveAnswer();
            this.AsGps?.RemoveAnswer();
            this.AsSingleOption?.RemoveAnswer();
            this.AsSingleLinkedOption?.RemoveAnswer();
            this.AsMultiOption?.RemoveAnswer();
            this.AsMultiLinkedOption?.RemoveAnswer();
            this.AsYesNo?.RemoveAnswer();
            this.AsTextList?.RemoveAnswer();
        }

        public bool IsOnTheSameOrDeeperLevel(Identity questionIdentity)
        {
            var rosterLevel = questionIdentity.RosterVector.Length;
            return this.Identity.RosterVector.Take(rosterLevel).SequenceEqual(questionIdentity.RosterVector);
        }

        public override IInterviewTreeNode Clone()
        {
            var clonedQuestion = (InterviewTreeQuestion)this.MemberwiseClone();
            if (this.IsDateTime) clonedQuestion.AsDateTime = this.AsDateTime.Clone();
            if (this.IsDouble) clonedQuestion.AsDouble = this.AsDouble.Clone();
            if (this.IsGps) clonedQuestion.AsGps = this.AsGps.Clone();
            if (this.IsMultiOption) clonedQuestion.AsMultiOption = this.AsMultiOption.Clone();
            if (this.IsMultimedia) clonedQuestion.AsMultimedia = this.AsMultimedia.Clone();
            if (this.IsQRBarcode) clonedQuestion.AsQRBarcode = this.AsQRBarcode.Clone();
            if (this.IsSingleOption) clonedQuestion.AsSingleOption = this.AsSingleOption.Clone();
            if (this.IsText) clonedQuestion.AsText = this.AsText.Clone();
            if (this.IsTextList) clonedQuestion.AsTextList = this.AsTextList.Clone();
            if (this.IsYesNo) clonedQuestion.AsYesNo = this.AsYesNo.Clone();
            if (this.IsInteger) clonedQuestion.AsInteger = this.AsInteger.Clone();

            if (this.IsMultiLinkedOption) clonedQuestion.AsMultiLinkedOption = this.AsMultiLinkedOption.Clone();
            if (this.IsSingleLinkedOption) clonedQuestion.AsSingleLinkedOption = this.AsSingleLinkedOption.Clone();
            if (this.IsCascading) clonedQuestion.AsCascading = this.AsCascading.Clone(clonedQuestion); 

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

        public InterviewTreeDateTimeQuestion Clone()
        {
            return (InterviewTreeDateTimeQuestion)this.MemberwiseClone();
        }
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

        public InterviewTreeGpsQuestion Clone()
        {
            var clone = (InterviewTreeGpsQuestion)this.MemberwiseClone();
            clone.answer = this.answer?.Clone();
            return clone;
        }
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

        public InterviewTreeMultimediaQuestion Clone() => (InterviewTreeMultimediaQuestion)this.MemberwiseClone();
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

        public InterviewTreeIntegerQuestion Clone() => (InterviewTreeIntegerQuestion)this.MemberwiseClone();
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

        public InterviewTreeDoubleQuestion Clone() => (InterviewTreeDoubleQuestion)this.MemberwiseClone();
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

        public InterviewTreeQRBarcodeQuestion Clone() => (InterviewTreeQRBarcodeQuestion)this.MemberwiseClone();
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

        public InterviewTreeTextQuestion Clone() => (InterviewTreeTextQuestion)this.MemberwiseClone();
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
        public void SetAnswer(IEnumerable<AnsweredYesNoOption> answer) => this.answer = answer?.ToArray();

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

        public InterviewTreeYesNoQuestion Clone()
        {
            var clone = (InterviewTreeYesNoQuestion)this.MemberwiseClone();
            clone.SetAnswer(this.answer?.ToArray());
            return clone;
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

        public InterviewTreeTextListQuestion Clone()
        {
            var clone = (InterviewTreeTextListQuestion)this.MemberwiseClone();
            clone.answer = (Tuple<decimal, string>[])clone.answer?.Clone();
            return clone;
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

        public InterviewTreeSingleOptionQuestion Clone() => (InterviewTreeSingleOptionQuestion)this.MemberwiseClone();
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

        internal void SetAnswer(IEnumerable<int> intValues)
        {
            SetAnswer((intValues ?? new int[0]).Select(Convert.ToDecimal).ToArray());
        }

        public InterviewTreeMultiOptionQuestion Clone()
        {
            var clone = (InterviewTreeMultiOptionQuestion)this.MemberwiseClone();
            clone.answer = (decimal[])this.answer?.Clone();
            return clone;
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

        public InterviewTreeSingleLinkedOptionQuestion Clone()
        {
            var clone = (InterviewTreeSingleLinkedOptionQuestion) this.MemberwiseClone();
            if (this.IsAnswered) clone.SetAnswer(new RosterVector(this.answer));
            clone.SetOptions(this.Options);
            return clone;
        }
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
        public void SetAnswer(IEnumerable<RosterVector> answer)
        {
            this.answer = answer.Select(x => x.ToArray()).ToArray();
        }

        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeMultiLinkedOptionQuestion question)
        {
            if (question?.answer == null && this.answer == null)
                return true;

            if (question?.answer != null && this.answer != null)
                return question.answer.SelectMany(x => x).SequenceEqual(this.answer.SelectMany(x => x));

            return false;
        }

        public InterviewTreeMultiLinkedOptionQuestion Clone()
        {
            var clone = (InterviewTreeMultiLinkedOptionQuestion)this.MemberwiseClone();
            clone.SetAnswer(this.answer?.Select(a => (decimal[])a.Clone()).ToArray());
            clone.SetOptions(this.Options);
            return clone;
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

        public List<RosterVector> Options { get; protected set; }

        public void SetOptions(IEnumerable<RosterVector> options)
        {
            this.Options = options.ToList();
        }
    }

    public class InterviewTreeCascadingQuestion
    {
        private InterviewTreeQuestion question;
        private readonly Guid cascadingParentQuestionId;

        public InterviewTreeCascadingQuestion(InterviewTreeQuestion question, Guid cascadingParentQuestionId)
        {
            this.question = question;
            this.cascadingParentQuestionId = cascadingParentQuestionId;
        }

        public InterviewTreeSingleOptionQuestion GetCascadingParentQuestion()
        {
            return (this.question.Parent as InterviewTreeGroup)
                ?.GetQuestionFromThisOrUpperLevel(this.cascadingParentQuestionId)
                .AsSingleOption;
        }

        public Guid CascadingParentQuestionId => this.cascadingParentQuestionId;

        public InterviewTreeCascadingQuestion Clone(InterviewTreeQuestion question)
        {
            var clone = (InterviewTreeCascadingQuestion)this.MemberwiseClone();
            clone.question = question;
            return clone;
        }
    }

}
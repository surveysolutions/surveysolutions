using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeQuestion : InterviewTreeLeafNode, ISubstitutable
    {
        public InterviewTreeQuestion(Identity identity, 
            SubstitionText title, 
            string variableName,
            QuestionType questionType, 
            object answer, 
            IEnumerable<RosterVector> linkedOptions, 
            Guid? cascadingParentQuestionId, 
            bool isYesNo,
            bool isDecimal, 
            bool isLinkedToListQuestion,
            bool isTimestampQuestion = false, 
            Guid? linkedSourceId = null, 
            Identity commonParentRosterIdForLinkedQuestion = null, 
            SubstitionText[] validationMessages = null,
            bool isInterviewerQuestion = true,
            bool isPrefilled = false)
            : base(identity)
        {
            this.ValidationMessages = validationMessages ?? new SubstitionText[0];
            this.Title = title;
            this.VariableName = variableName;
            this.IsInterviewer = isInterviewerQuestion;
            this.IsPrefilled = isPrefilled;

            if (questionType == QuestionType.SingleOption)
            {
                if (linkedSourceId.HasValue)
                {
                    if (isLinkedToListQuestion)
                    {
                        this.AsSingleLinkedToList = new InterviewTreeSingleOptionLinkedToListQuestion(answer, linkedSourceId.Value);
                    }
                    else
                        this.AsSingleLinkedOption = new InterviewTreeSingleLinkedToRosterQuestion(linkedOptions, answer,
                            linkedSourceId.Value, commonParentRosterIdForLinkedQuestion);
                }
                else
                    this.AsSingleFixedOption = new InterviewTreeSingleOptionQuestion(answer);
            }

            if (questionType == QuestionType.MultyOption)
            {
                if (isYesNo)
                    this.AsYesNo = new InterviewTreeYesNoQuestion(answer);
                else if (linkedSourceId.HasValue)
                {
                    if (isLinkedToListQuestion)
                    {
                        this.AsMultiLinkedToList = new InterviewTreeMultiOptionLinkedToListQuestion(answer, linkedSourceId.Value);
                    }
                    else
                        this.AsMultiLinkedOption = new InterviewTreeMultiLinkedToRosterQuestion(linkedOptions, answer, linkedSourceId.Value, commonParentRosterIdForLinkedQuestion);
                }
                else
                    this.AsMultiFixedOption = new InterviewTreeMultiOptionQuestion(answer);
            }

            if (questionType == QuestionType.DateTime)
                this.AsDateTime = new InterviewTreeDateTimeQuestion(answer, isTimestampQuestion);

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

        public InterviewTreeDoubleQuestion AsDouble { get; private set; }
        public InterviewTreeTextListQuestion AsTextList { get; private set; }
        public InterviewTreeTextQuestion AsText { get; private set; }
        public InterviewTreeQRBarcodeQuestion AsQRBarcode { get; private set; }
        public InterviewTreeIntegerQuestion AsInteger { get; private set; }
        public InterviewTreeMultimediaQuestion AsMultimedia { get; private set; }
        public InterviewTreeGpsQuestion AsGps { get; private set; }
        public InterviewTreeDateTimeQuestion AsDateTime { get; private set; }
        public InterviewTreeMultiOptionQuestion AsMultiFixedOption { get; private set; }
        public InterviewTreeMultiLinkedToRosterQuestion AsMultiLinkedOption { get; private set; }
        public InterviewTreeYesNoQuestion AsYesNo { get; private set; }
        public InterviewTreeSingleLinkedToRosterQuestion AsSingleLinkedOption { get; private set; }
        public InterviewTreeSingleOptionQuestion AsSingleFixedOption { get; private set; }

        public InterviewTreeMultiOptionLinkedToListQuestion AsMultiLinkedToList { get; private set; }
        public InterviewTreeSingleOptionLinkedToListQuestion AsSingleLinkedToList { get; private set; }

        public InterviewTreeLinkedToListQuestion AsLinkedToList => this.IsSingleLinkedToList ? (InterviewTreeLinkedToListQuestion)this.AsSingleLinkedToList : this.AsMultiLinkedToList;
        public InterviewTreeLinkedToRosterQuestion AsLinked => this.IsSingleLinkedOption ? (InterviewTreeLinkedToRosterQuestion)this.AsSingleLinkedOption : this.AsMultiLinkedOption;

        public InterviewTreeCascadingQuestion AsCascading { get; private set; }

        public List<AnswerComment> AnswerComments { get; set; } = new List<AnswerComment>();

        public SubstitionText Title { get; private set; }

        public SubstitionText[] ValidationMessages { get; private set; }
        
        public string VariableName { get; }
        public bool IsInterviewer { get; private set; }
        public bool IsPrefilled { get; private set; }

        public bool IsValid => !this.FailedValidations?.Any() ?? this.isValidWithoutFailedValidations;

        public IReadOnlyList<FailedValidationCondition> FailedValidations { get; private set; }

        public void SetTitle(SubstitionText title) => this.Title = title;

        public void SetValidationMessages(SubstitionText[] validationMessages) => this.ValidationMessages = validationMessages;

        public void MarkInvalid(IEnumerable<FailedValidationCondition> failedValidations)
        {
            if(failedValidations == null) throw new ArgumentNullException(nameof(failedValidations));
            this.FailedValidations = failedValidations.ToReadOnlyCollection();
        }

        [Obsolete("Since v6.0")]
        private bool isValidWithoutFailedValidations = true;
        [Obsolete("Since v6.0")]
        public void MarkInvalid() => this.isValidWithoutFailedValidations = false;

        public void MarkValid()
        {
            this.isValidWithoutFailedValidations = true;
            this.FailedValidations = Enumerable.Empty<FailedValidationCondition>().ToList();
        }

        public bool IsDouble => this.AsDouble != null;
        public bool IsInteger => this.AsInteger != null;
        public bool IsSingleFixedOption => this.AsSingleFixedOption != null;
        public bool IsMultiFixedOption => this.AsMultiFixedOption != null;
        public bool IsMultiLinkedOption => this.AsMultiLinkedOption != null;
        public bool IsSingleLinkedOption => this.AsSingleLinkedOption != null;
        public bool IsQRBarcode => this.AsQRBarcode != null;
        public bool IsText => this.AsText != null;
        public bool IsTextList => this.AsTextList != null;
        public bool IsYesNo => this.AsYesNo != null;
        public bool IsDateTime => this.AsDateTime != null;
        public bool IsGps => this.AsGps != null;
        public bool IsMultimedia => this.AsMultimedia != null;

        public bool IsMultiLinkedToList => this.AsMultiLinkedToList != null;
        public bool IsSingleLinkedToList => this.AsSingleLinkedToList != null;

        public bool IsLinkedToListQuestion => (this.IsMultiLinkedToList || this.IsSingleLinkedToList);
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
            if (this.IsSingleFixedOption) return this.AsSingleFixedOption.IsAnswered;
            if (this.IsSingleLinkedOption) return this.AsSingleLinkedOption.IsAnswered;
            if (this.IsMultiFixedOption) return this.AsMultiFixedOption.IsAnswered;
            if (this.IsMultiLinkedOption) return this.AsMultiLinkedOption.IsAnswered;
            if (this.IsYesNo) return this.AsYesNo.IsAnswered;
            if (this.IsTextList) return this.AsTextList.IsAnswered;
            if (this.IsSingleLinkedToList) return this.AsSingleLinkedToList.IsAnswered;
            if (this.IsMultiLinkedToList) return this.AsMultiLinkedToList.IsAnswered;

            return false;
        }

        private string GetTypeAsText()
        {
            if (this.IsText) return "Text";
            if (this.IsInteger) return "Integer";
            if (this.IsDouble) return "Double";
            if (this.IsDateTime) return "DateTime";
            if (this.IsMultimedia) return "Multimedia";
            if (this.IsQRBarcode) return "QRBarcode";
            if (this.IsGps) return "Gps";
            if (this.IsSingleFixedOption) return "SingleFixedOption";
            if (this.IsSingleLinkedOption) return "SingleLinkedOption";
            if (this.IsMultiFixedOption) return "MultiFixedOption";
            if (this.IsMultiLinkedOption) return "MultiLinkedOption";
            if (this.IsYesNo) return "YesNo";
            if (this.IsTextList) return "TextList";
            if (this.IsSingleLinkedToList) return "SingleLinkedToList";
            if (this.IsMultiLinkedToList) return "MultiLinkedToList";

            return "no type";
        }

        public void SetAnswer(AbstractAnswer answer)
        {
            if (answer == null)
            {
                this.RemoveAnswer();
            }
            else
            {
                this.AsText?.SetAnswer((TextAnswer) answer);
                this.AsInteger?.SetAnswer((NumericIntegerAnswer) answer);
                this.AsDouble?.SetAnswer((NumericRealAnswer) answer);
                this.AsDateTime?.SetAnswer((DateTimeAnswer) answer);
                this.AsMultimedia?.SetAnswer((MultimediaAnswer) answer);
                this.AsQRBarcode?.SetAnswer((QRBarcodeAnswer) answer);
                this.AsGps?.SetAnswer((GpsAnswer) answer);
                this.AsSingleFixedOption?.SetAnswer((CategoricalFixedSingleOptionAnswer) answer);
                this.AsSingleLinkedOption?.SetAnswer((CategoricalLinkedSingleOptionAnswer) answer);
                this.AsMultiFixedOption?.SetAnswer((CategoricalFixedMultiOptionAnswer) answer);
                this.AsMultiLinkedOption?.SetAnswer((CategoricalLinkedMultiOptionAnswer) answer);
                this.AsYesNo?.SetAnswer((YesNoAnswer) answer);
                this.AsTextList?.SetAnswer((TextListAnswer) answer);

                this.AsMultiLinkedToList?.SetAnswer((CategoricalFixedMultiOptionAnswer)answer);
                this.AsSingleLinkedToList?.SetAnswer((CategoricalFixedSingleOptionAnswer)answer);
            }
        }

        public string FormatForException() => $"'{this.Title} [{this.VariableName}] ({this.Identity})'";

        public override string ToString()
            => $"{GetTypeAsText()} Question {this.Identity} '{this.Title}'. " +
               $"{(this.IsAnswered() ? $"Answer = '{this.GetAnswerAsString()}'" : "No answer")}. " +
               $"{(this.IsDisabled() ? "Disabled" : "Enabled")}. " +
               $"{(this.IsValid ? "Valid" : "Invalid")}";

        public void CalculateLinkedOptions()
        {
            if (!this.IsLinked) return;

            InterviewTreeLinkedToRosterQuestion linkedQuestion = this.AsLinked;

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

        public void CalculateLinkedToListOptions(bool resetAnswerOnOptionChange = true)
        {
            if (!this.IsLinkedToListQuestion) return;
            InterviewTreeLinkedToListQuestion linkedToListQuestion = this.AsLinkedToList;

            var refListQuestion = this.Tree.FindEntityInQuestionBranch(linkedToListQuestion.LinkedSourceId, Identity) as InterviewTreeQuestion;
            var options = refListQuestion?.AsTextList?.GetAnswer()?.Rows.Select(x => x.Value).ToArray() ?? new decimal[0];

            var previousOptions = this.AsLinkedToList.Options;
            this.AsLinkedToList.SetOptions(options);
            if(resetAnswerOnOptionChange)
            {
                var optionsAreIdentical = previousOptions.SequenceEqual(options);
                if (optionsAreIdentical) return;

                if (this.IsMultiLinkedToList)
                    this.AsMultiLinkedToList.RemoveAnswer();
                else
                    this.AsSingleLinkedToList.RemoveAnswer();
            }
        }


        [Obsolete("use SetAnswer instead")]
        public void SetObjectAnswer(object answer)
        {
            if (this.IsText) { this.AsText.SetAnswer(TextAnswer.FromString(answer as string)); return; }
            if (this.IsInteger) { this.AsInteger.SetAnswer(NumericIntegerAnswer.FromInt(Convert.ToInt32(answer))); return; }
            if (this.IsDouble) { this.AsDouble.SetAnswer(NumericRealAnswer.FromDouble(Convert.ToDouble(answer))); return; }
            if (this.IsDateTime) { this.AsDateTime.SetAnswer(DateTimeAnswer.FromDateTime((DateTime)answer)); return; }
            if (this.IsMultimedia) { this.AsMultimedia.SetAnswer(MultimediaAnswer.FromString(answer as string)); return; }
            if (this.IsQRBarcode) { this.AsQRBarcode.SetAnswer(QRBarcodeAnswer.FromString(answer as string)); return; }
            if (this.IsGps) { this.AsGps.SetAnswer(GpsAnswer.FromGeoPosition((GeoPosition)answer)); return; }
            if (this.IsSingleFixedOption) { this.AsSingleFixedOption.SetAnswer(CategoricalFixedSingleOptionAnswer.FromInt(Convert.ToInt32(answer))); return; }
            if (this.IsMultiFixedOption)
            {
                var answerAsRosterVector = answer as RosterVector;
                CategoricalFixedMultiOptionAnswer categoricalFixedMultiOptionAnswer = 
                    (answerAsRosterVector != null) 
                        ? CategoricalFixedMultiOptionAnswer.FromDecimalArray(answerAsRosterVector) 
                        : CategoricalFixedMultiOptionAnswer.FromDecimalArray((decimal[]) answer);

                this.AsMultiFixedOption.SetAnswer(categoricalFixedMultiOptionAnswer);
                return;
            }
            if (this.IsSingleLinkedOption)
            {
                var answerAsRosterVector = answer as RosterVector;
                CategoricalLinkedSingleOptionAnswer categoricalLinkedSingleOptionAnswer = 
                    (answerAsRosterVector != null)
                        ? CategoricalLinkedSingleOptionAnswer.FromRosterVector(answerAsRosterVector)
                        : CategoricalLinkedSingleOptionAnswer.FromRosterVector(new RosterVector((decimal[]) answer));

                this.AsSingleLinkedOption.SetAnswer(categoricalLinkedSingleOptionAnswer);
                return;
            }
            if (this.IsMultiLinkedOption)
            {
                var answerAsRosterVector = answer as RosterVector[];
                CategoricalLinkedMultiOptionAnswer categoricalLinkedMultiOptionAnswer =
                    (answerAsRosterVector != null)
                        ? CategoricalLinkedMultiOptionAnswer.FromRosterVectors(answerAsRosterVector)
                        : CategoricalLinkedMultiOptionAnswer.FromDecimalArrayArray((decimal[][]) answer);
                    
                this.AsMultiLinkedOption.SetAnswer(categoricalLinkedMultiOptionAnswer);
                return;
            }
            if (this.IsYesNo) { this.AsYesNo.SetAnswer(YesNoAnswer.FromAnsweredYesNoOptions((AnsweredYesNoOption[])answer)); return; }
            if (this.IsTextList) { this.AsTextList.SetAnswer(TextListAnswer.FromTupleArray((Tuple<decimal, string>[])answer)); return; }
            if (this.IsSingleLinkedToList) { this.AsSingleLinkedToList.SetAnswer(CategoricalFixedSingleOptionAnswer.FromInt(Convert.ToInt32(answer))); return; }
            if (this.IsMultiLinkedToList) { this.AsMultiLinkedToList.SetAnswer(CategoricalFixedMultiOptionAnswer.FromDecimalArray(answer as decimal[])); return; }
        }

        public string GetAnswerAsString(CultureInfo cultureInfo = null)
        {
            if (!this.IsAnswered()) return String.Empty;

            Func<decimal, string> getCategoricalAnswerOptionText = answerOptionValue
                => Tree.GetOptionForQuestionByOptionValue(this.Identity.Id, answerOptionValue);

            if (this.IsText) return this.AsText.GetAnswer()?.Value;
            if (this.IsMultimedia) return this.AsMultimedia.GetAnswer()?.FileName;
            if (this.IsQRBarcode) return this.AsQRBarcode.GetAnswer()?.DecodedText;
            if (this.IsInteger) return AnswerUtils.AnswerToString(this.AsInteger.GetAnswer()?.Value);
            if (this.IsDouble) return AnswerUtils.AnswerToString(this.AsDouble.GetAnswer()?.Value);
            if (this.IsDateTime)
                return AnswerUtils.AnswerToString(this.AsDateTime.GetAnswer()?.Value, cultureInfo: cultureInfo, isTimestamp: this.AsDateTime.IsTimestamp);
            if (this.IsGps) return AnswerUtils.AnswerToString(this.AsGps.GetAnswer()?.Value);
            if (this.IsTextList) return AnswerUtils.AnswerToString(this.AsTextList.GetAnswer()?.ToTupleArray());

            if (this.IsSingleLinkedOption)
            {
                var interviewTreeSingleLinkedToRosterQuestion = this.AsSingleLinkedOption;
                var linkedToEntityId = new Identity(interviewTreeSingleLinkedToRosterQuestion.LinkedSourceId, interviewTreeSingleLinkedToRosterQuestion.GetAnswer()?.SelectedValue);
                
                return this.Tree.GetQuestion(linkedToEntityId)?.GetAnswerAsString() ?? 
                       this.Tree.GetRoster(linkedToEntityId).RosterTitle;
            }
            if (this.IsMultiLinkedOption)
            {
                var formattedAnswers = this.AsMultiLinkedOption.GetAnswer()?.CheckedValues
                    .Select(x => new Identity(this.AsMultiLinkedOption.LinkedSourceId, x))
                    .Select(x => this.Tree.GetQuestion(x)?.GetAnswerAsString() ?? this.Tree.GetRoster(x).RosterTitle);
                return string.Join(", ", formattedAnswers);
            }

            if (this.IsSingleFixedOption) return AnswerUtils.AnswerToString(Convert.ToDecimal(this.AsSingleFixedOption.GetAnswer()?.SelectedValue), getCategoricalAnswerOptionText);
            if (this.IsMultiFixedOption) return AnswerUtils.AnswerToString(this.AsMultiFixedOption.GetAnswer()?.ToDecimals()?.ToArray(), getCategoricalAnswerOptionText);
            if (this.IsYesNo) return AnswerUtils.AnswerToString(this.AsYesNo.GetAnswer()?.ToAnsweredYesNoOptions()?.ToArray(), getCategoricalAnswerOptionText);

            if (this.IsSingleLinkedToList) return AnswerUtils.AnswerToString(Convert.ToDecimal(this.AsSingleLinkedToList.GetAnswer()?.SelectedValue), getCategoricalAnswerOptionText);
            if (this.IsMultiLinkedToList) return AnswerUtils.AnswerToString(this.AsMultiLinkedToList.GetAnswer()?.ToDecimals()?.ToArray(), getCategoricalAnswerOptionText);

            return string.Empty;
        }

        public void UpdateLinkedOptionsAndResetAnswerIfNeeded(RosterVector[] options, bool removeAnswer = true)
        {
            if (!this.IsLinked) return;
            var previousOptions = this.AsLinked.Options;
            this.AsLinked.SetOptions(options);

            if (!removeAnswer) return;

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
            this.AsSingleFixedOption?.RemoveAnswer();
            this.AsSingleLinkedOption?.RemoveAnswer();
            this.AsMultiFixedOption?.RemoveAnswer();
            this.AsMultiLinkedOption?.RemoveAnswer();
            this.AsYesNo?.RemoveAnswer();
            this.AsTextList?.RemoveAnswer();
            this.AsMultiLinkedToList?.RemoveAnswer();
            this.AsSingleLinkedToList?.RemoveAnswer();
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
            if (this.IsMultiFixedOption) clonedQuestion.AsMultiFixedOption = this.AsMultiFixedOption.Clone();
            if (this.IsMultimedia) clonedQuestion.AsMultimedia = this.AsMultimedia.Clone();
            if (this.IsQRBarcode) clonedQuestion.AsQRBarcode = this.AsQRBarcode.Clone();
            if (this.IsSingleFixedOption) clonedQuestion.AsSingleFixedOption = this.AsSingleFixedOption.Clone();
            if (this.IsText) clonedQuestion.AsText = this.AsText.Clone();
            if (this.IsTextList) clonedQuestion.AsTextList = this.AsTextList.Clone();
            if (this.IsYesNo) clonedQuestion.AsYesNo = this.AsYesNo.Clone();
            if (this.IsInteger) clonedQuestion.AsInteger = this.AsInteger.Clone();

            if (this.IsMultiLinkedOption) clonedQuestion.AsMultiLinkedOption = this.AsMultiLinkedOption.Clone();
            if (this.IsSingleLinkedOption) clonedQuestion.AsSingleLinkedOption = this.AsSingleLinkedOption.Clone();
            if (this.IsCascading) clonedQuestion.AsCascading = this.AsCascading.Clone(clonedQuestion);

            if (this.IsSingleLinkedToList) clonedQuestion.AsSingleLinkedToList = this.AsSingleLinkedToList.Clone();
            if (this.IsMultiLinkedToList) clonedQuestion.AsMultiLinkedToList = this.AsMultiLinkedToList.Clone();

            clonedQuestion.Title = this.Title?.Clone();
            clonedQuestion.ValidationMessages = this.ValidationMessages.Select(x => x.Clone()).ToArray();
            clonedQuestion.FailedValidations = this.FailedValidations?
                .Select(v => new FailedValidationCondition(v.FailedConditionIndex))
                .ToReadOnlyCollection();
            clonedQuestion.AnswerComments = this.AnswerComments?
                .Select(a => new AnswerComment(a.UserId, a.UserRole, a.CommentTime, a.Comment, a.QuestionIdentity))
                .ToList();


            return clonedQuestion;
        }

        public void ReplaceSubstitutions()
        {
            this.Title.ReplaceSubstitutions();
            foreach (var messagesWithSubstition in this.ValidationMessages)
            {
                messagesWithSubstition.ReplaceSubstitutions();
            }
        }

        public override void SetTree(InterviewTree tree)
        {
            base.SetTree(tree);
            this.Title?.SetTree(tree);
            foreach (var messagesWithSubstition in this.ValidationMessages)
            {
                messagesWithSubstition.SetTree(tree);
            }
        }
    }


    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeDateTimeQuestion
    {
        private DateTimeAnswer answer;
        public InterviewTreeDateTimeQuestion(object answer, bool isTimestamp)
        {
            this.IsTimestamp = isTimestamp;
            this.answer = answer == null ? null : DateTimeAnswer.FromDateTime(Convert.ToDateTime(answer));
        }

        public bool IsAnswered => this.answer != null;

        public bool IsTimestamp { get; private set; }
        public DateTimeAnswer GetAnswer() => this.answer;
        public void SetAnswer(DateTimeAnswer answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeDateTimeQuestion question) => question?.answer == this.answer;

        public InterviewTreeDateTimeQuestion Clone() => (InterviewTreeDateTimeQuestion) this.MemberwiseClone();

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeGpsQuestion
    {
        private GpsAnswer answer;

        public InterviewTreeGpsQuestion(object answer)
        {
            this.answer = GpsAnswer.FromGeoPosition(answer as GeoPosition);
        }

        public bool IsAnswered => this.answer != null;
        public GpsAnswer GetAnswer() => this.answer;
        public void SetAnswer(GpsAnswer answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeGpsQuestion question) => question?.answer == this.answer;

        public InterviewTreeGpsQuestion Clone() => (InterviewTreeGpsQuestion) this.MemberwiseClone();

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeMultimediaQuestion
    {
        private MultimediaAnswer answer;

        public InterviewTreeMultimediaQuestion(object answer)
        {
            this.answer = MultimediaAnswer.FromString(answer as string);
        }

        public bool IsAnswered => this.answer != null;
        public MultimediaAnswer GetAnswer() => this.answer;
        public void SetAnswer(MultimediaAnswer answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeMultimediaQuestion question) => question?.answer == this.answer;

        public InterviewTreeMultimediaQuestion Clone() => (InterviewTreeMultimediaQuestion) this.MemberwiseClone();

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeIntegerQuestion
    {
        private NumericIntegerAnswer answer;

        public InterviewTreeIntegerQuestion() { }

        public InterviewTreeIntegerQuestion(object answer)
        {
            this.answer = answer == null ? null : NumericIntegerAnswer.FromInt(Convert.ToInt32(answer));
        }

        public virtual bool IsAnswered => this.answer != null;
        public virtual NumericIntegerAnswer GetAnswer() => this.answer;
        public void SetAnswer(NumericIntegerAnswer answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeIntegerQuestion question) => question?.answer == this.answer;

        public InterviewTreeIntegerQuestion Clone() => (InterviewTreeIntegerQuestion)this.MemberwiseClone();

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeDoubleQuestion
    {
        private NumericRealAnswer answer;

        public InterviewTreeDoubleQuestion(object answer)
        {
            this.answer = answer == null ? null : NumericRealAnswer.FromDouble(Convert.ToDouble(answer));
        }

        public bool IsAnswered => this.answer != null;
        public NumericRealAnswer GetAnswer() => this.answer;
        public void SetAnswer(NumericRealAnswer answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeDoubleQuestion question) => question?.answer == this.answer;

        public InterviewTreeDoubleQuestion Clone() => (InterviewTreeDoubleQuestion)this.MemberwiseClone();

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeQRBarcodeQuestion
    {
        private QRBarcodeAnswer answer;

        public InterviewTreeQRBarcodeQuestion(object answer)
        {
            this.answer = QRBarcodeAnswer.FromString(answer as string);
        }

        public bool IsAnswered => this.answer != null;
        public QRBarcodeAnswer GetAnswer() => this.answer;
        public void SetAnswer(QRBarcodeAnswer answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;
        public bool EqualByAnswer(InterviewTreeQRBarcodeQuestion question) => question?.answer == this.answer;

        public InterviewTreeQRBarcodeQuestion Clone() => (InterviewTreeQRBarcodeQuestion)this.MemberwiseClone();

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeTextQuestion
    {
        private TextAnswer answer;

        public InterviewTreeTextQuestion(object answer)
        {
            this.answer = TextAnswer.FromString(answer as string);
        }

        public bool IsAnswered => this.answer != null;
        public TextAnswer GetAnswer() => this.answer;
        public void SetAnswer(TextAnswer answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeTextQuestion question) => question?.answer == this.answer;

        public InterviewTreeTextQuestion Clone() => (InterviewTreeTextQuestion)this.MemberwiseClone();

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeYesNoQuestion
    {
        private YesNoAnswer answer;

        public InterviewTreeYesNoQuestion(object answer)
        {
            this.answer = YesNoAnswer.FromAnsweredYesNoOptions(answer as AnsweredYesNoOption[]);
        }

        public bool IsAnswered => this.answer != null && this.answer.CheckedOptions.Count > 0;
        public YesNoAnswer GetAnswer() => this.answer;
        public void SetAnswer(YesNoAnswer answer) => this.answer = answer;

        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeYesNoQuestion question)
        {
            if (question?.answer == null && this.answer == null)
                return true;

            if (question?.answer != null && this.answer != null)
                return question.answer.ToAnsweredYesNoOptions().SequenceEqual(this.answer.ToAnsweredYesNoOptions());

            return false;
        }

        public InterviewTreeYesNoQuestion Clone() => (InterviewTreeYesNoQuestion) this.MemberwiseClone();

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeTextListQuestion
    {
        public InterviewTreeTextListQuestion() { }

        private TextListAnswer answer;

        public InterviewTreeTextListQuestion(object answer)
        {
            this.answer = TextListAnswer.FromTupleArray(answer as Tuple<decimal, string>[]);
        }

        public virtual bool IsAnswered => this.answer != null && this.answer.Rows.Count > 0;
        public virtual TextListAnswer GetAnswer() => this.answer;
        public void SetAnswer(TextListAnswer answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeTextListQuestion question)
        {
            if (question?.answer == null && this.answer == null)
                return true;

            if (question?.answer != null && this.answer != null)
                return question.answer.ToTupleArray().SequenceEqual(this.answer.ToTupleArray());

            return false;
        }

        public string GetTitleByItemCode(decimal code)
        {
            if (!IsAnswered)
                return string.Empty;
            return this.answer.Rows.FirstOrDefault(row => row.Value == code)?.Text ?? String.Empty;
        }

        public InterviewTreeTextListQuestion Clone() => (InterviewTreeTextListQuestion) this.MemberwiseClone();

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeSingleOptionQuestion
    {
        public InterviewTreeSingleOptionQuestion(){ }

        private CategoricalFixedSingleOptionAnswer answer;

        public InterviewTreeSingleOptionQuestion(object answer)
        {
            this.answer = answer == null ? null : CategoricalFixedSingleOptionAnswer.FromInt(Convert.ToInt32(answer));
        }

        public virtual bool IsAnswered => this.answer != null;

        public virtual CategoricalFixedSingleOptionAnswer GetAnswer() => this.answer;

        public void SetAnswer(CategoricalFixedSingleOptionAnswer answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeSingleOptionQuestion question) => question?.answer == this.answer;

        public InterviewTreeSingleOptionQuestion Clone() => (InterviewTreeSingleOptionQuestion) this.MemberwiseClone();

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeMultiOptionQuestion
    {
        private CategoricalFixedMultiOptionAnswer answer;

        public InterviewTreeMultiOptionQuestion(object answer)
        {
            this.answer = CategoricalFixedMultiOptionAnswer.FromDecimalArray(answer as decimal[]);
        }

        public bool IsAnswered => this.answer != null && this.answer.CheckedValues.Count > 0;
        public CategoricalFixedMultiOptionAnswer GetAnswer() => this.answer;
        public void SetAnswer(CategoricalFixedMultiOptionAnswer answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeMultiOptionQuestion question)
        {
            if (question?.answer == null && this.answer == null)
                return true;

            if (question?.answer != null && this.answer != null)
                return question.answer.CheckedValues.SequenceEqual(this.answer.CheckedValues);

            return false;
        }

        public InterviewTreeMultiOptionQuestion Clone() => (InterviewTreeMultiOptionQuestion) this.MemberwiseClone();

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeSingleLinkedToRosterQuestion : InterviewTreeLinkedToRosterQuestion
    {
        protected InterviewTreeSingleLinkedToRosterQuestion(){}

        private CategoricalLinkedSingleOptionAnswer answer;

        public InterviewTreeSingleLinkedToRosterQuestion(IEnumerable<RosterVector> linkedOptions, object answer, Guid linkedSourceId, Identity commonParentRosterIdForLinkedQuestion)
            : base(linkedOptions, linkedSourceId, commonParentRosterIdForLinkedQuestion)
        {
            this.answer = CategoricalLinkedSingleOptionAnswer.FromRosterVector(answer as RosterVector);
        }

        public virtual bool IsAnswered => this.answer != null && this.answer.SelectedValue.Length > 0;
        public virtual CategoricalLinkedSingleOptionAnswer GetAnswer() => this.answer;
        public void SetAnswer(CategoricalLinkedSingleOptionAnswer answer) => this.answer = answer;
        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeSingleLinkedToRosterQuestion question) => question?.answer == this.answer;

        public InterviewTreeSingleLinkedToRosterQuestion Clone()
        {
            var clone = (InterviewTreeSingleLinkedToRosterQuestion) this.MemberwiseClone();
            clone.SetOptions(this.Options);
            return clone;
        }

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeMultiLinkedToRosterQuestion : InterviewTreeLinkedToRosterQuestion
    {
        protected InterviewTreeMultiLinkedToRosterQuestion()
        {
        }

        private CategoricalLinkedMultiOptionAnswer answer;
        public InterviewTreeMultiLinkedToRosterQuestion(IEnumerable<RosterVector> linkedOptions, object answer, Guid linkedSourceId, Identity commonParentRosterIdForLinkedQuestion)
            : base(linkedOptions, linkedSourceId, commonParentRosterIdForLinkedQuestion)
        {
            this.answer = CategoricalLinkedMultiOptionAnswer.FromRosterVectors(answer as RosterVector[]);
        }

        public bool IsAnswered => this.answer != null && this.answer.CheckedValues.Count > 0;
        public CategoricalLinkedMultiOptionAnswer GetAnswer() => this.answer;
        public void SetAnswer(CategoricalLinkedMultiOptionAnswer answer) => this.answer = answer;

        public void RemoveAnswer() => this.answer = null;

        public bool EqualByAnswer(InterviewTreeMultiLinkedToRosterQuestion question)
        {
            if (question?.answer == null && this.answer == null)
                return true;

            if (question?.answer != null && this.answer != null)
                return question.answer.CheckedValues.SelectMany(x => x.Coordinates).SequenceEqual(this.answer.CheckedValues.SelectMany(x => x.Coordinates));

            return false;
        }

        public InterviewTreeMultiLinkedToRosterQuestion Clone()
        {
            var clone = (InterviewTreeMultiLinkedToRosterQuestion) this.MemberwiseClone();
            clone.SetOptions(this.Options);
            return clone;
        }

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";
    }

    [DebuggerDisplay("{ToString()}")]
    public abstract class InterviewTreeLinkedToRosterQuestion
    {

        protected InterviewTreeLinkedToRosterQuestion()
        {
        }

        public Guid LinkedSourceId { get; private set; }
        public Identity CommonParentRosterIdForLinkedQuestion { get; private set; }

        protected InterviewTreeLinkedToRosterQuestion(IEnumerable<RosterVector> linkedOptions, Guid linkedSourceId, Identity commonParentRosterIdForLinkedQuestion)
        {
            this.LinkedSourceId = linkedSourceId;
            this.CommonParentRosterIdForLinkedQuestion = commonParentRosterIdForLinkedQuestion;

            this.Options = linkedOptions?.ToList() ?? new List<RosterVector>();
        }

        public List<RosterVector> Options { get; protected set; }

        public void SetOptions(IEnumerable<RosterVector> options)
        {
            this.Options = options.ToList();
        }

        public override string ToString() => $"{this.LinkedSourceId.FormatGuid()} -> {string.Join(", ", this.Options)}";
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeLinkedToListQuestion
    {
        public Guid LinkedSourceId { get; protected set; }

        public IReadOnlyCollection<decimal> Options { get; protected set; }

        public void SetOptions(IEnumerable<decimal> options)
        {
            this.Options = options?.ToReadOnlyCollection() ?? new List<decimal>().ToReadOnlyCollection();
        }
    }
    
    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeMultiOptionLinkedToListQuestion : InterviewTreeLinkedToListQuestion
    {
        public InterviewTreeMultiOptionLinkedToListQuestion(object answer, Guid linkedToQuestionId)
        {
            this.answer = CategoricalFixedMultiOptionAnswer.FromDecimalArray(answer as decimal[]);
            this.Options = new List<decimal>();
            LinkedSourceId = linkedToQuestionId;
        }

        private CategoricalFixedMultiOptionAnswer answer;
        public bool IsAnswered => this.answer != null && this.answer.CheckedValues.Count > 0;
        public CategoricalFixedMultiOptionAnswer GetAnswer() => this.answer;
        public void SetAnswer(CategoricalFixedMultiOptionAnswer answer) => this.answer = answer;

        public bool EqualByAnswer(InterviewTreeMultiOptionLinkedToListQuestion question)
        {
            if (question?.answer == null && this.answer == null)
                return true;

            if (question?.answer != null && this.answer != null)
                return question.answer.CheckedValues.SequenceEqual(this.answer.CheckedValues);

            return false;
        }

        public void RemoveAnswer() => this.answer = null;

        public InterviewTreeMultiOptionLinkedToListQuestion Clone()
        {
            var clone = (InterviewTreeMultiOptionLinkedToListQuestion)this.MemberwiseClone();
            clone.SetOptions(this.Options);
            return clone;
        }

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeSingleOptionLinkedToListQuestion : InterviewTreeLinkedToListQuestion
    {
        public InterviewTreeSingleOptionLinkedToListQuestion(object answer, Guid linkedToQuestionId)
        {
            this.answer = answer == null ? null : CategoricalFixedSingleOptionAnswer.FromInt(Convert.ToInt32(answer));
            this.Options = new List<decimal>();
            LinkedSourceId = linkedToQuestionId;
        }

        private CategoricalFixedSingleOptionAnswer answer;
        public bool IsAnswered => this.answer != null;
        public CategoricalFixedSingleOptionAnswer GetAnswer() => this.answer;
        public void SetAnswer(CategoricalFixedSingleOptionAnswer answer) => this.answer = answer;
        public bool EqualByAnswer(InterviewTreeSingleOptionLinkedToListQuestion question) => question?.answer == this.answer;
        public void RemoveAnswer() => this.answer = null;

        public InterviewTreeSingleOptionLinkedToListQuestion Clone()
        {
            var clone = (InterviewTreeSingleOptionLinkedToListQuestion)this.MemberwiseClone();
            clone.SetOptions(this.Options);
            return clone;
        }

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";
    }

    [DebuggerDisplay("{ToString()}")]
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
                .AsSingleFixedOption;
        }

        public Guid CascadingParentQuestionId => this.cascadingParentQuestionId;

        public InterviewTreeCascadingQuestion Clone(InterviewTreeQuestion question)
        {
            var clone = (InterviewTreeCascadingQuestion)this.MemberwiseClone();
            clone.question = question;
            return clone;
        }

        public override string ToString() => string.Join(", ", this.GetCascadingParentQuestion()?.GetAnswer());
    }

}
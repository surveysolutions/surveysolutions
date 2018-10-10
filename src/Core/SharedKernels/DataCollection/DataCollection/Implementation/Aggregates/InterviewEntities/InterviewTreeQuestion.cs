using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public enum InterviewQuestionType
    {
        Text = 1,
        Integer =2,
        Double = 3,
        DateTime = 4,
        Multimedia = 5,
        QRBarcode  = 6,
        Gps = 7,
        SingleFixedOption = 8,
        SingleLinkedOption = 9,
        MultiFixedOption = 10,
        MultiLinkedOption = 11,
        YesNo = 12,
        TextList = 13,
        SingleLinkedToList = 14,
        MultiLinkedToList = 15,
        Area = 16,
        Audio = 17,
        Cascading = 18
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeQuestion : InterviewTreeLeafNode, ISubstitutable, IInterviewTreeValidateable
    {
        public InterviewTreeQuestion(Identity identity, 
            SubstitutionText title,
            SubstitutionText instructions,
            string variableName,
            QuestionType questionType, 
            object answer, // always null here
            IEnumerable<RosterVector> linkedOptions, 
            Guid? cascadingParentQuestionId, 
            bool isYesNo,
            bool isDecimal, 
            bool isLinkedToListQuestion,
            bool isTimestampQuestion = false, 
            Guid? linkedSourceId = null, 
            Identity commonParentRosterIdForLinkedQuestion = null, 
            SubstitutionText[] validationMessages = null,
            bool isInterviewerQuestion = true,
            bool isPrefilled = false,
            bool isSupervisors = false,
            bool isHidden = false)
            : base(identity)
        {
            this.ValidationMessages = validationMessages ?? new SubstitutionText[0];
            this.Title = title;
            this.VariableName = variableName;
            this.IsInterviewer = isInterviewerQuestion;
            this.IsPrefilled = isPrefilled;
            this.IsSupervisors = isSupervisors;
            this.IsHidden = isHidden;
            this.Instructions = instructions;

            switch (questionType)
            {
                case QuestionType.SingleOption:
                    {
                        if (linkedSourceId.HasValue)
                        {
                            if (isLinkedToListQuestion)
                            {
                                this.InterviewQuestion =
                                    new InterviewTreeSingleOptionLinkedToListQuestion(answer, linkedSourceId.Value);
                            }
                            else
                            {
                                this.InterviewQuestion = new InterviewTreeSingleLinkedToRosterQuestion(linkedOptions,
                                    answer,
                                    linkedSourceId.Value, commonParentRosterIdForLinkedQuestion);
                            }
                        }
                        else if (cascadingParentQuestionId.HasValue)
                        {
                            this.InterviewQuestion =
                                new InterviewTreeCascadingQuestion(this, cascadingParentQuestionId.Value, answer);
                        }
                        else
                        {
                            this.InterviewQuestion = new InterviewTreeSingleOptionQuestion(answer);
                        }
                    }
                    break;

                case QuestionType.MultyOption:
                    {
                        if (isYesNo)
                        {
                            this.InterviewQuestion = new InterviewTreeYesNoQuestion(answer);
                        }

                        else if (linkedSourceId.HasValue)
                        {
                            if (isLinkedToListQuestion)
                            {
                                this.InterviewQuestion =
                                    new InterviewTreeMultiOptionLinkedToListQuestion(answer, linkedSourceId.Value);
                            }
                            else
                            {
                                this.InterviewQuestion = new InterviewTreeMultiLinkedToRosterQuestion(linkedOptions,
                                    answer,
                                    linkedSourceId.Value, commonParentRosterIdForLinkedQuestion);
                            }
                        }
                        else
                        {
                            this.InterviewQuestion = new InterviewTreeMultiOptionQuestion(answer);
                        }
                        
                    }
                    break;
                case QuestionType.DateTime:
                    this.InterviewQuestion = new InterviewTreeDateTimeQuestion(answer, isTimestampQuestion);
                    break;

                case QuestionType.GpsCoordinates:
                    this.InterviewQuestion = new InterviewTreeGpsQuestion(answer);
                    break;

                case QuestionType.Multimedia:
                    this.InterviewQuestion = new InterviewTreeMultimediaQuestion(answer, null);
                    break;
                case QuestionType.Numeric:
                    {
                        if (isDecimal)
                        {
                            this.InterviewQuestion = new InterviewTreeDoubleQuestion(answer);
                        }
                        else
                        {
                            this.InterviewQuestion = new InterviewTreeIntegerQuestion(answer);
                        }
                    }
                    break;
                case QuestionType.QRBarcode:
                        this.InterviewQuestion = new InterviewTreeQRBarcodeQuestion(answer);
                        break;

                case QuestionType.Area:
                    this.InterviewQuestion = new InterviewTreeAreaQuestion(answer);
                    break;

                case QuestionType.Text:
                    this.InterviewQuestion = new InterviewTreeTextQuestion(answer);
                    break;
                case QuestionType.TextList:
                    this.InterviewQuestion = new InterviewTreeTextListQuestion(answer);
                    break;
                case QuestionType.Audio:
                    this.InterviewQuestion = new InterviewTreeAudioQuestion(answer, null);
                    break;
            }
        }

        public BaseInterviewQuestion InterviewQuestion { get; set; }
        
        public InterviewTreeLinkedToListQuestion AsLinkedToList => 
            this.IsSingleLinkedToList ? 
            this.InterviewQuestion as InterviewTreeLinkedToListQuestion : 
            this.InterviewQuestion as InterviewTreeMultiOptionLinkedToListQuestion;

        public InterviewTreeLinkedToRosterQuestion AsLinked => 
            this.IsSingleLinkedOption ? 
            this.InterviewQuestion as InterviewTreeLinkedToRosterQuestion : 
            this.InterviewQuestion as InterviewTreeMultiLinkedToRosterQuestion;

        public List<AnswerComment> AnswerComments { get; set; } = new List<AnswerComment>();

        public sealed override SubstitutionText Title { get; protected set; }

        public SubstitutionText[] ValidationMessages { get; private set; }

        public SubstitutionText Instructions { get; private set; }

        public string VariableName { get; }
        public bool IsInterviewer { get; private set; }
        public bool IsPrefilled { get; private set; }
        public bool IsSupervisors { get; private set; }
        public bool IsHidden { get; private set; }
        public bool IsReadonly { get; private set; }
        public bool IsValid
        {
            get
            {
                if (this.FailedErrors == null) return this.isValidWithoutFailedValidations;
                
                return this.FailedErrors.Count == 0; 
            }
        }

        public bool IsPlausible => this.FailedWarnings.Count == 0;

        public void RunImportInvariantsOrThrow(InterviewQuestionInvariants questionInvariants)
        {
            this.InterviewQuestion.RunImportInvariants(questionInvariants);
        }

        public IReadOnlyList<FailedValidationCondition> FailedErrors { get; private set; }

        public void SetTitle(SubstitutionText title) 
        {
            this.Title = title;
            this.Title.SetTree(this.Tree);
        }
        public void SetInstructions(SubstitutionText instructions)
        {
            this.Instructions = instructions;
            this.Instructions.SetTree(this.Tree);
        }

        public void SetValidationMessages(SubstitutionText[] validationMessages)
        {
            this.ValidationMessages = validationMessages ?? throw new ArgumentNullException(nameof(validationMessages));

            foreach (var validationMessage in validationMessages)
            {
                validationMessage.SetTree(this.Tree);
            }
        }

        public void MarkInvalid(IEnumerable<FailedValidationCondition> failedValidations)
        {
            this.FailedErrors = failedValidations?.ToReadOnlyCollection() ?? throw new ArgumentNullException(nameof(failedValidations));
        }

        [Obsolete("Since v6.0")]
        private bool isValidWithoutFailedValidations = true;
        [Obsolete("Since v6.0")]
        public void MarkInvalid() => this.isValidWithoutFailedValidations = false;

        public void MarkValid()
        {
            this.isValidWithoutFailedValidations = true;
            this.FailedErrors = Enumerable.Empty<FailedValidationCondition>().ToList();
        }

        public IReadOnlyList<FailedValidationCondition> FailedWarnings { get; private set; } = Array.Empty<FailedValidationCondition>();

        public void MarkPlausible()
            => this.FailedWarnings = new List<FailedValidationCondition>();

        public void MarkImplausible(IEnumerable<FailedValidationCondition> failedValidations)
        {
            if (failedValidations == null) throw new ArgumentNullException(nameof(failedValidations));
            this.FailedWarnings = failedValidations.ToReadOnlyCollection();
        }

        public InterviewTreeDoubleQuestion GetAsInterviewTreeDoubleQuestion() => this.InterviewQuestion as InterviewTreeDoubleQuestion;
        public InterviewTreeIntegerQuestion GetAsInterviewTreeIntegerQuestion() => this.InterviewQuestion as InterviewTreeIntegerQuestion;
        public InterviewTreeSingleOptionQuestion GetAsInterviewTreeSingleOptionQuestion() => this.InterviewQuestion as InterviewTreeSingleOptionQuestion;
        public InterviewTreeMultiOptionQuestion GetAsInterviewTreeMultiOptionQuestion() => this.InterviewQuestion as InterviewTreeMultiOptionQuestion;
        public InterviewTreeMultiLinkedToRosterQuestion GetAsInterviewTreeMultiLinkedToRosterQuestion() => this.InterviewQuestion as InterviewTreeMultiLinkedToRosterQuestion;
        public InterviewTreeSingleLinkedToRosterQuestion GetAsInterviewTreeSingleLinkedToRosterQuestion() => this.InterviewQuestion as InterviewTreeSingleLinkedToRosterQuestion;
        public InterviewTreeQRBarcodeQuestion GetAsInterviewTreeQRBarcodeQuestion() => this.InterviewQuestion as InterviewTreeQRBarcodeQuestion;
        public InterviewTreeTextQuestion GetAsInterviewTreeTextQuestion() => this.InterviewQuestion as InterviewTreeTextQuestion;
        public InterviewTreeTextListQuestion GetAsInterviewTreeTextListQuestion() => this.InterviewQuestion as InterviewTreeTextListQuestion;
        public InterviewTreeYesNoQuestion GetAsInterviewTreeYesNoQuestion() => this.InterviewQuestion as InterviewTreeYesNoQuestion;
        public InterviewTreeDateTimeQuestion GetAsInterviewTreeDateTimeQuestion() => this.InterviewQuestion as InterviewTreeDateTimeQuestion;
        public InterviewTreeGpsQuestion GetAsInterviewTreeGpsQuestion() => this.InterviewQuestion as InterviewTreeGpsQuestion;
        public InterviewTreeMultimediaQuestion GetAsInterviewTreeMultimediaQuestion() => this.InterviewQuestion as InterviewTreeMultimediaQuestion;
        public InterviewTreeAreaQuestion GetAsInterviewTreeAreaQuestion() => this.InterviewQuestion as InterviewTreeAreaQuestion;
        public InterviewTreeMultiOptionLinkedToListQuestion GetAsInterviewTreeMultiOptionLinkedToListQuestion() => this.InterviewQuestion as InterviewTreeMultiOptionLinkedToListQuestion;
        public InterviewTreeSingleOptionLinkedToListQuestion GetAsInterviewTreeSingleOptionLinkedToListQuestion() => this.InterviewQuestion as InterviewTreeSingleOptionLinkedToListQuestion;
        public InterviewTreeAudioQuestion GetAsInterviewTreeAudioQuestion() => this.InterviewQuestion as InterviewTreeAudioQuestion;

        public InterviewTreeCascadingQuestion GetAsInterviewTreeCascadingQuestion() => this.InterviewQuestion as InterviewTreeCascadingQuestion;

        public InterviewQuestionType InterviewQuestionType => this.InterviewQuestion.InterviewQuestionType;
        public bool IsDouble => this.InterviewQuestion.InterviewQuestionType == InterviewQuestionType.Double;
        public bool IsInteger => this.InterviewQuestion.InterviewQuestionType == InterviewQuestionType.Integer;
        public bool IsSingleFixedOption => this.InterviewQuestion.InterviewQuestionType == InterviewQuestionType.SingleFixedOption;
        public bool IsMultiFixedOption => this.InterviewQuestion.InterviewQuestionType == InterviewQuestionType.MultiFixedOption;
        public bool IsMultiLinkedOption => this.InterviewQuestion.InterviewQuestionType == InterviewQuestionType.MultiLinkedOption;
        public bool IsSingleLinkedOption => this.InterviewQuestion.InterviewQuestionType == InterviewQuestionType.SingleLinkedOption;
        public bool IsQRBarcode => this.InterviewQuestion.InterviewQuestionType == InterviewQuestionType.QRBarcode;
        public bool IsText => this.InterviewQuestion.InterviewQuestionType == InterviewQuestionType.Text;
        public bool IsTextList => this.InterviewQuestion.InterviewQuestionType == InterviewQuestionType.TextList;

        public bool IsNumericInteger => this.InterviewQuestion.InterviewQuestionType == InterviewQuestionType.Integer;
        public bool IsYesNo => this.InterviewQuestion.InterviewQuestionType == InterviewQuestionType.YesNo;
        public bool IsDateTime => this.InterviewQuestion.InterviewQuestionType == InterviewQuestionType.DateTime;
        public bool IsGps => this.InterviewQuestion.InterviewQuestionType == InterviewQuestionType.Gps;
        public bool IsMultimedia => this.InterviewQuestion.InterviewQuestionType == InterviewQuestionType.Multimedia;
        public bool IsArea => this.InterviewQuestion.InterviewQuestionType == InterviewQuestionType.Area;

        public bool IsMultiLinkedToList => this.InterviewQuestion.InterviewQuestionType == InterviewQuestionType.MultiLinkedToList;
        public bool IsSingleLinkedToList => this.InterviewQuestion.InterviewQuestionType == InterviewQuestionType.SingleLinkedToList;

        public bool IsLinkedToListQuestion => (this.IsMultiLinkedToList || this.IsSingleLinkedToList);
        public bool IsLinked => (this.IsMultiLinkedOption || this.IsSingleLinkedOption);
        public bool IsCascading => this.InterviewQuestion.InterviewQuestionType == InterviewQuestionType.Cascading;
        public bool IsAudio => this.InterviewQuestion.InterviewQuestionType == InterviewQuestionType.Audio;

        public bool IsAnswered()
        {
            return this.InterviewQuestion.IsAnswered();
        }

        private string GetTypeAsText()
        {
            return this.InterviewQuestion.InterviewQuestionType.ToString("D");
        }

        public void SetAnswer(AbstractAnswer answer)
        {
            if (answer == null)
            {
                this.RemoveAnswer();
            }
            else
            {
                switch (this.InterviewQuestion.InterviewQuestionType)
                {
                    case InterviewQuestionType.Area:
                        ((InterviewTreeAreaQuestion)this.InterviewQuestion).SetAnswer((AreaAnswer)answer);
                        break;
                    case InterviewQuestionType.Audio:
                        ((InterviewTreeAudioQuestion)this.InterviewQuestion).SetAnswer((AudioAnswer)answer);
                        break;
                    case InterviewQuestionType.Text:
                        ((InterviewTreeTextQuestion)this.InterviewQuestion).SetAnswer((TextAnswer)answer);
                        break;
                    case InterviewQuestionType.Integer:
                        ((InterviewTreeIntegerQuestion)this.InterviewQuestion).SetAnswer((NumericIntegerAnswer)answer);
                        break;
                    case InterviewQuestionType.Double:
                        ((InterviewTreeDoubleQuestion)this.InterviewQuestion).SetAnswer((NumericRealAnswer)answer);
                        break;
                    case InterviewQuestionType.DateTime:
                        ((InterviewTreeDateTimeQuestion)this.InterviewQuestion).SetAnswer((DateTimeAnswer)answer);
                        break;
                    case InterviewQuestionType.Multimedia:
                        ((InterviewTreeMultimediaQuestion)this.InterviewQuestion).SetAnswer((MultimediaAnswer)answer);
                        break;
                    case InterviewQuestionType.QRBarcode:
                        ((InterviewTreeQRBarcodeQuestion)this.InterviewQuestion).SetAnswer((QRBarcodeAnswer)answer);
                        break;
                    case InterviewQuestionType.Gps:
                        ((InterviewTreeGpsQuestion)this.InterviewQuestion).SetAnswer((GpsAnswer)answer);
                        break;
                    case InterviewQuestionType.SingleFixedOption:
                    case InterviewQuestionType.Cascading:
                        ((InterviewTreeSingleOptionQuestion)this.InterviewQuestion).SetAnswer((CategoricalFixedSingleOptionAnswer)answer);
                        break;
                    case InterviewQuestionType.SingleLinkedOption:
                        ((InterviewTreeSingleLinkedToRosterQuestion)this.InterviewQuestion).SetAnswer((CategoricalLinkedSingleOptionAnswer)answer);
                        break;
                    case InterviewQuestionType.MultiFixedOption:
                        ((InterviewTreeMultiOptionQuestion)this.InterviewQuestion).SetAnswer((CategoricalFixedMultiOptionAnswer)answer);
                        break;
                    case InterviewQuestionType.MultiLinkedOption:
                        ((InterviewTreeMultiLinkedToRosterQuestion)this.InterviewQuestion).SetAnswer((CategoricalLinkedMultiOptionAnswer)answer);
                        break;
                    case InterviewQuestionType.YesNo:
                        ((InterviewTreeYesNoQuestion)this.InterviewQuestion).SetAnswer((YesNoAnswer)answer);
                        break;
                    case InterviewQuestionType.TextList:
                        ((InterviewTreeTextListQuestion)this.InterviewQuestion).SetAnswer((TextListAnswer)answer);
                        break;
                    case InterviewQuestionType.MultiLinkedToList:
                        ((InterviewTreeMultiOptionLinkedToListQuestion)this.InterviewQuestion).SetAnswer((CategoricalFixedMultiOptionAnswer)answer);
                        break;
                    case InterviewQuestionType.SingleLinkedToList:
                        ((InterviewTreeSingleOptionLinkedToListQuestion)this.InterviewQuestion).SetAnswer((CategoricalFixedSingleOptionAnswer)answer);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public override string ToString()
            => $"{GetTypeAsText()} Question {this.Identity} '{this.VariableName}'. " +
               $"{(this.IsAnswered() ? $"Answer = '{this.GetAnswerAsString()}'" : "No answer")}. " +
               $"{(this.IsDisabled() ? "Disabled" : "Enabled")}. " +
               $"{(this.IsValid ? "Valid" : "Invalid")}";

        public class LinkedOptionAndParent
        {
            public RosterVector Option { get; set; }
            public Identity ParenRoster { get; set; }
        }
        public LinkedOptionAndParent[] GetCalculatedLinkedOptions()
        {
            if (!this.IsLinked) return null;

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
                .Where(x => (x as InterviewTreeQuestion)?.IsAnswered() ?? !string.IsNullOrWhiteSpace((x as InterviewTreeRoster)?.RosterTitle))
                .Select(x => new LinkedOptionAndParent {
                    Option = x.Identity.RosterVector,
                    ParenRoster = x is InterviewTreeRoster? x.Identity : x.Parents.LastOrDefault(p => p is InterviewTreeRoster)?.Identity
                }).ToArray();

            return options;
        }

        public void CalculateLinkedToListOptions(bool updateAnswerOnOptionChange = true)
        {
            if (!this.IsLinkedToListQuestion) return;
            var linkedToListQuestion = this.AsLinkedToList;

            var refQuestion = this.Tree.FindEntityInQuestionBranch(linkedToListQuestion.LinkedSourceId, Identity) as InterviewTreeQuestion;
           
            var options = (refQuestion?.IsDisabled() ?? false)
                ? EmptyArray<int>.Value
                : ((InterviewTreeTextListQuestion)refQuestion?.InterviewQuestion)?.GetAnswer()?.Rows.Select(x => x.Value).ToArray() ?? EmptyArray<int>.Value;

            var previousOptions = this.AsLinkedToList.Options;
            this.AsLinkedToList.SetOptions(options);

            if (!this.AsLinkedToList.IsAnswered()) return;

            if (!updateAnswerOnOptionChange) return;

            var optionsAreIdentical = previousOptions.SequenceEqual(options);
                if (optionsAreIdentical) return;

            if (this.IsSingleLinkedToList)
            {
                if (!options.Contains(this.GetAsInterviewTreeSingleOptionLinkedToListQuestion().GetAnswer().SelectedValue))
                    this.InterviewQuestion.RemoveAnswer();
            }
            else
            {
                var previousAnswer = this.GetAsInterviewTreeMultiOptionLinkedToListQuestion().GetAnswer().CheckedValues;
                var exsitingAnswerOptions = previousAnswer.Where(x => options.Contains(x)).ToList();

                if (exsitingAnswerOptions.Count == 0)
                {
                    this.InterviewQuestion.RemoveAnswer();
                    return;
                }

                if (exsitingAnswerOptions.Count < previousAnswer.Count)
                    this.GetAsInterviewTreeMultiOptionLinkedToListQuestion().SetAnswer(CategoricalFixedMultiOptionAnswer.Convert(exsitingAnswerOptions));
            }
        }

        [Obsolete("use SetAnswer instead")]
        public void SetObjectAnswer(object answer)
        {
            if (this.IsText) { ((InterviewTreeTextQuestion)this.InterviewQuestion).SetAnswer(TextAnswer.FromString(answer as string)); return; }
            if (this.IsInteger) { ((InterviewTreeIntegerQuestion)this.InterviewQuestion).SetAnswer(NumericIntegerAnswer.FromInt(Convert.ToInt32(answer))); return; }
            if (this.IsDouble) { ((InterviewTreeDoubleQuestion)this.InterviewQuestion).SetAnswer(NumericRealAnswer.FromDouble(Convert.ToDouble(answer))); return; }
            if (this.IsDateTime) { ((InterviewTreeDateTimeQuestion)this.InterviewQuestion).SetAnswer(DateTimeAnswer.FromDateTime((DateTime)answer)); return; }
            if (this.IsMultimedia) { ((InterviewTreeMultimediaQuestion)this.InterviewQuestion).SetAnswer(MultimediaAnswer.FromString(answer as string, null)); return; }
            if (this.IsQRBarcode) { ((InterviewTreeQRBarcodeQuestion)this.InterviewQuestion).SetAnswer(QRBarcodeAnswer.FromString(answer as string)); return; }
            if (this.IsGps) { ((InterviewTreeGpsQuestion)this.InterviewQuestion).SetAnswer(GpsAnswer.FromGeoPosition((GeoPosition)answer)); return; }
            if (this.IsSingleFixedOption || this.IsCascading) { ((InterviewTreeSingleOptionQuestion)this.InterviewQuestion).SetAnswer(CategoricalFixedSingleOptionAnswer.FromInt(Convert.ToInt32(answer))); return; }
            if (this.IsMultiFixedOption)
            {
                RosterVector answerAsRosterVector = RosterVector.Convert(answer);
                var categoricalFixedMultiOptionAnswer = CategoricalFixedMultiOptionAnswer.Convert(answerAsRosterVector.Array);
                ((InterviewTreeMultiOptionQuestion)this.InterviewQuestion).SetAnswer(categoricalFixedMultiOptionAnswer);
                return;
            }
            if (this.IsSingleLinkedOption)
            {
                RosterVector answerAsRosterVector = RosterVector.Convert(answer);
                var categoricalLinkedSingleOptionAnswer = CategoricalLinkedSingleOptionAnswer.FromRosterVector(answerAsRosterVector);
                ((InterviewTreeSingleLinkedToRosterQuestion)this.InterviewQuestion).SetAnswer(categoricalLinkedSingleOptionAnswer);
                return;
            }
            if (this.IsMultiLinkedOption)
            {
                var answerAsRosterVector = RosterVector.ConvertToArray(answer);
                var categoricalLinkedMultiOptionAnswer = CategoricalLinkedMultiOptionAnswer.FromRosterVectors(answerAsRosterVector);

                ((InterviewTreeMultiLinkedToRosterQuestion)this.InterviewQuestion).SetAnswer(categoricalLinkedMultiOptionAnswer);
                return;
            }
            if (this.IsYesNo) { ((InterviewTreeYesNoQuestion)this.InterviewQuestion).SetAnswer(YesNoAnswer.FromAnsweredYesNoOptions((AnsweredYesNoOption[])answer)); return; }
            if (this.IsTextList) { ((InterviewTreeTextListQuestion)this.InterviewQuestion).SetAnswer(TextListAnswer.FromTupleArray((Tuple<decimal, string>[])answer)); return; }
            if (this.IsSingleLinkedToList) { ((InterviewTreeSingleOptionLinkedToListQuestion)this.InterviewQuestion).SetAnswer(CategoricalFixedSingleOptionAnswer.FromInt(Convert.ToInt32(answer))); return; }
            if (this.IsMultiLinkedToList)
            {
                var answerAsCategoricalFixedMultiOptionAnswer = CategoricalFixedMultiOptionAnswer.Convert(answer);
                ((InterviewTreeMultiOptionLinkedToListQuestion)this.InterviewQuestion).SetAnswer(answerAsCategoricalFixedMultiOptionAnswer);
                return;
            }
            if (this.IsArea) { ((InterviewTreeAreaQuestion)this.InterviewQuestion).SetAnswer(AreaAnswer.FromArea((Area)answer)); return; }
            if (this.IsAudio) { ((InterviewTreeAudioQuestion)this.InterviewQuestion).SetAnswer((AudioAnswer)answer); return; }
        }

        public void SetObjectProtectedAnswer(object answer)
        {
            if (this.IsInteger) { ((InterviewTreeIntegerQuestion)this.InterviewQuestion).SetProtectedAnswer(NumericIntegerAnswer.FromInt(Convert.ToInt32(answer))); return; }
            if (this.IsMultiFixedOption) { ((InterviewTreeMultiOptionQuestion)this.InterviewQuestion).SetProtectedAnswer(CategoricalFixedMultiOptionAnswer.FromIntArray((int[])answer)); return; }
            if (this.IsYesNo) { ((InterviewTreeYesNoQuestion)this.InterviewQuestion).SetProtectedAnswer(YesNoAnswer.FromAnsweredYesNoOptions((AnsweredYesNoOption[])answer)); return; }
            if (this.IsTextList) { ((InterviewTreeTextListQuestion)this.InterviewQuestion).SetProtectedAnswer(TextListAnswer.FromTupleArray((Tuple<decimal, string>[])answer)); return; }
        }

        public string GetAnswerAsString(CultureInfo cultureInfo = null)
        {
            if (!this.IsAnswered()) return null;

            if (this.IsText) return ((InterviewTreeTextQuestion)this.InterviewQuestion).GetAnswer()?.Value;
            if (this.IsMultimedia) return ((InterviewTreeMultimediaQuestion)this.InterviewQuestion).GetAnswer()?.FileName;
            if (this.IsQRBarcode) return ((InterviewTreeQRBarcodeQuestion)this.InterviewQuestion).GetAnswer()?.DecodedText;
            if (this.IsArea) return ((InterviewTreeAreaQuestion)this.InterviewQuestion).GetAnswer()?.Value.ToString();
            if (this.IsInteger)
                return AnswerUtils.AnswerToString(Convert.ToDecimal(((InterviewTreeIntegerQuestion)this.InterviewQuestion).GetAnswer()?.Value), GetCategoricalAnswerOptionText);
            if (this.IsDouble)
                return AnswerUtils.AnswerToString(Convert.ToDecimal(((InterviewTreeDoubleQuestion)this.InterviewQuestion).GetAnswer()?.Value), GetCategoricalAnswerOptionText);
            if (this.IsDateTime)
            {
                var interviewTreeDateTimeQuestion = (InterviewTreeDateTimeQuestion)this.InterviewQuestion;
                DateTime? dateTime = interviewTreeDateTimeQuestion.GetAnswer()?.Value;
                return AnswerUtils.AnswerToString(dateTime, isTimestamp: ((InterviewTreeDateTimeQuestion)this.InterviewQuestion).IsTimestamp);
            }
            if (this.IsGps) return AnswerUtils.AnswerToString(((InterviewTreeGpsQuestion)this.InterviewQuestion).GetAnswer()?.Value);
            if (this.IsTextList) return AnswerUtils.AnswerToString(((InterviewTreeTextListQuestion)this.InterviewQuestion).GetAnswer()?.ToTupleArray());

            if (this.IsSingleLinkedOption)
            {
                var interviewTreeSingleLinkedToRosterQuestion = ((InterviewTreeSingleLinkedToRosterQuestion)this.InterviewQuestion);
                var linkedToEntityId = new Identity(interviewTreeSingleLinkedToRosterQuestion.LinkedSourceId, interviewTreeSingleLinkedToRosterQuestion.GetAnswer()?.SelectedValue);
                
                return this.Tree.GetQuestion(linkedToEntityId)?.GetAnswerAsString() ?? 
                       this.Tree.GetRoster(linkedToEntityId)?.RosterTitle ?? string.Empty;
            }
            if (this.IsMultiLinkedOption)
            {
                var formattedAnswers = ((InterviewTreeMultiLinkedToRosterQuestion)this.InterviewQuestion).GetAnswer()?.CheckedValues
                    .Select(x => new Identity(((InterviewTreeMultiLinkedToRosterQuestion)this.InterviewQuestion).LinkedSourceId, x))
                    .Select(x => this.Tree.GetQuestion(x)?.GetAnswerAsString() ?? this.Tree.GetRoster(x)?.RosterTitle ?? string.Empty);
                return string.Join(", ", formattedAnswers);
            }

            string GetCategoricalAnswerOptionText(decimal answerOptionValue) =>
                this.Tree.GetOptionForQuestionByOptionValue(this.Identity.Id, answerOptionValue);

            if (this.IsSingleFixedOption || this.IsCascading)
                return AnswerUtils.AnswerToString(Convert.ToDecimal(((InterviewTreeSingleOptionQuestion)this.InterviewQuestion).GetAnswer()?.SelectedValue), GetCategoricalAnswerOptionText);
            if (this.IsMultiFixedOption)
                return AnswerUtils.AnswerToString(((InterviewTreeMultiOptionQuestion)this.InterviewQuestion).GetAnswer()?.ToDecimals()?.ToArray(), GetCategoricalAnswerOptionText);
            if (this.IsYesNo)
                return AnswerUtils.AnswerToString(((InterviewTreeYesNoQuestion)this.InterviewQuestion).GetAnswer()?.ToAnsweredYesNoOptions()?.ToArray(), GetCategoricalAnswerOptionText);

            if (this.IsSingleLinkedToList)
            {
                var singleToListAnswer = Convert.ToDecimal(((InterviewTreeSingleOptionLinkedToListQuestion)this.InterviewQuestion).GetAnswer().SelectedValue);
                var refListQuestion = this.Tree.FindEntityInQuestionBranch(((InterviewTreeSingleOptionLinkedToListQuestion)this.InterviewQuestion).LinkedSourceId, Identity) as InterviewTreeQuestion;
                var refListOption = ((InterviewTreeTextListQuestion)refListQuestion?.InterviewQuestion)?.GetAnswer()?.Rows.SingleOrDefault(x => x.Value == singleToListAnswer);
                return refListOption?.Text;
            }
            if (this.IsMultiLinkedToList)
            {
                var multiToListAnswers = ((InterviewTreeMultiOptionLinkedToListQuestion)this.InterviewQuestion).GetAnswer()?.ToDecimals()?.ToHashSet();
                var refListQuestion = this.Tree.FindEntityInQuestionBranch(((InterviewTreeMultiOptionLinkedToListQuestion)this.InterviewQuestion).LinkedSourceId, Identity) as InterviewTreeQuestion;
                var refListQuestionAllOptions = ((InterviewTreeTextListQuestion)refListQuestion?.InterviewQuestion)?.GetAnswer()?.Rows;
                var refListOptions = refListQuestionAllOptions?.Where(x => multiToListAnswers?.Contains(x.Value) ?? false).ToArray();
                return string.Join(", ", refListOptions.Select(o => o.Text));
            }

            return string.Empty;
        }

        public static object GetAnswerAsObject(InterviewTreeQuestion question)
        {
            //for backward compatibility answers were casted to other types
            //please take into concideration in case of changes

            if (!question.IsAnswered()) return null;

            if (question.IsText) return ((InterviewTreeTextQuestion)question.InterviewQuestion).GetAnswer()?.Value;
            if (question.IsMultimedia) return ((InterviewTreeMultimediaQuestion)question.InterviewQuestion).GetAnswer()?.FileName;
            if (question.IsQRBarcode) return ((InterviewTreeQRBarcodeQuestion)question.InterviewQuestion).GetAnswer()?.DecodedText;
            if (question.IsInteger) return ((InterviewTreeIntegerQuestion)question.InterviewQuestion).GetAnswer()?.Value;
            if (question.IsDouble) return ((InterviewTreeDoubleQuestion)question.InterviewQuestion).GetAnswer()?.Value;
            if (question.IsDateTime) return ((InterviewTreeDateTimeQuestion)question.InterviewQuestion).GetAnswer()?.Value;
            if (question.IsGps) return ((InterviewTreeGpsQuestion)question.InterviewQuestion).GetAnswer()?.Value;
            if (question.IsTextList) return ((InterviewTreeTextListQuestion)question.InterviewQuestion).GetAnswer()?.ToTupleArray();
            if (question.IsSingleLinkedOption) return ((InterviewTreeSingleLinkedToRosterQuestion)question.InterviewQuestion).GetAnswer()?.SelectedValue.CoordinatesAsDecimals;
            if (question.IsMultiLinkedOption) return ((InterviewTreeMultiLinkedToRosterQuestion)question.InterviewQuestion).GetAnswer()?.CheckedValues;
            if (question.IsSingleFixedOption || question.IsCascading) return ((InterviewTreeSingleOptionQuestion)question.InterviewQuestion).GetAnswer()?.SelectedValue;
            if (question.IsMultiFixedOption) return ((InterviewTreeMultiOptionQuestion)question.InterviewQuestion).GetAnswer()?.ToDecimals()?.ToArray();
            if (question.IsYesNo) return ((InterviewTreeYesNoQuestion)question.InterviewQuestion).GetAnswer()?.ToAnsweredYesNoOptions()?.ToArray();
            if (question.IsSingleLinkedToList) return ((InterviewTreeSingleOptionLinkedToListQuestion)question.InterviewQuestion).GetAnswer().SelectedValue;
            if (question.IsMultiLinkedToList) return ((InterviewTreeMultiOptionLinkedToListQuestion)question.InterviewQuestion).GetAnswer()?.ToDecimals()?.ToArray();
            if (question.IsArea) return ((InterviewTreeAreaQuestion)question.InterviewQuestion).GetAnswer()?.Value;
            if (question.IsAudio) return ((InterviewTreeAudioQuestion)question.InterviewQuestion).GetAnswer();

            return null;
        }

        public static object GetProtectedAnswerAsObject(InterviewTreeQuestion question)
        {
            if (!question.HasProtectedAnswer()) return null;

            if (question.IsInteger) return ((InterviewTreeIntegerQuestion)question.InterviewQuestion).ProtectedAnswer?.Value;
            if (question.IsTextList) return ((InterviewTreeTextListQuestion)question.InterviewQuestion).ProtectedAnswer?.ToTupleArray();
            if (question.IsMultiFixedOption) return ((InterviewTreeMultiOptionQuestion)question.InterviewQuestion).ProtectedAnswer?.ToInts()?.ToArray();
            if (question.IsYesNo) return ((InterviewTreeYesNoQuestion)question.InterviewQuestion).ProtectedAnswer?.ToAnsweredYesNoOptions()?.ToArray();

            return null;
        }

        public void UpdateLinkedOptionsAndUpdateAnswerIfNeeded(RosterVector[] options, bool updateAnswer = true)
        {
            if (!this.IsLinked) return;
            var previousOptions = this.AsLinked.Options;
            var orderedOptions = this.GetOptionsInCorrectOrder(options);
            this.AsLinked.SetOptions(orderedOptions);

            if (!updateAnswer) return;

            if (!this.AsLinked.IsAnswered()) return;

            var optionsAreIdentical = previousOptions.SequenceEqual(orderedOptions);
            if (optionsAreIdentical) return;

            if (this.IsSingleLinkedOption)
            {
                if(!orderedOptions.Contains(this.GetAsInterviewTreeSingleLinkedToRosterQuestion().GetAnswer().SelectedValue))
                    this.InterviewQuestion.RemoveAnswer();
            }
            else
            {
                var previousAnswer = this.GetAsInterviewTreeMultiLinkedToRosterQuestion().GetAnswer().CheckedValues;
                var exsitingAnswerOptions = previousAnswer.Where(x => orderedOptions.Contains(x)).ToList();

                if (exsitingAnswerOptions.Count == 0)
                {
                        this.InterviewQuestion.RemoveAnswer();
                        return;
                }

                if(exsitingAnswerOptions.Count < previousAnswer.Count)
                    this.GetAsInterviewTreeMultiLinkedToRosterQuestion().SetAnswer(CategoricalLinkedMultiOptionAnswer.FromRosterVectors(exsitingAnswerOptions));
            }
        }

        private RosterVector[] GetOptionsInCorrectOrder(RosterVector[] options)
        {
            if (options.Length <= 1) return options;

            if (this.IsSingleLinkedOption || this.IsMultiLinkedOption)
            {
                var linkedLinkedSourceId = this.AsLinked.LinkedSourceId;
                
                var nodes = options.Select(vector => new Identity(linkedLinkedSourceId, vector))
                    .OrderBy(id => Tree.GetNodeCoordinatesInEnumeratorOrder(id), RosterVectorAsCoordinatesComparer.Instance);
                
                return nodes.Select(n => n.RosterVector).ToArray();
            }

            return options;
        }

        public void RemoveAnswer()
        {
            this.InterviewQuestion?.RemoveAnswer();
        }

        public bool IsOnTheSameOrDeeperLevel(Identity questionIdentity)
        {
            var rosterLevel = questionIdentity.RosterVector.Length;

            return questionIdentity.RosterVector.Identical(this.Identity.RosterVector, rosterLevel);
        }

        public override IInterviewTreeNode Clone()
        {
            var clonedQuestion = (InterviewTreeQuestion)this.MemberwiseClone();

            clonedQuestion.InterviewQuestion = this.InterviewQuestion.Clone();

            if(clonedQuestion.IsCascading)
                ((InterviewTreeCascadingQuestion)clonedQuestion.InterviewQuestion).SetQuestion(clonedQuestion);

            clonedQuestion.Title = this.Title?.Clone();
            clonedQuestion.Instructions = this.Instructions?.Clone();
            clonedQuestion.ValidationMessages = this.ValidationMessages.Select(x => x.Clone()).ToArray();
            clonedQuestion.FailedErrors = this.FailedErrors?
                .Select(v => new FailedValidationCondition(v.FailedConditionIndex))
                .ToReadOnlyCollection();
            clonedQuestion.FailedWarnings = this.FailedWarnings?
                .Select(v => new FailedValidationCondition(v.FailedConditionIndex))
                .ToReadOnlyCollection();
            clonedQuestion.AnswerComments = this.AnswerComments?
                .Select(a => new AnswerComment(a.UserId, a.UserRole, a.CommentTime, a.Comment, a.QuestionIdentity))
                .ToList();

            return clonedQuestion;
        }

        public override void Accept(IInterviewTreeUpdater updater)
        {
            updater.UpdateEnablement(this);
            if (this.IsSingleFixedOption)
            {
                updater.UpdateSingleOptionQuestion(this); 
            }
            else if (this.IsMultiFixedOption)
            {
                updater.UpdateMultiOptionQuestion(this);
            }
            else if (this.IsYesNo)
            {
                updater.UpdateYesNoQuestion(this);
            }
            else if (this.IsLinked)
            {
                updater.UpdateLinkedQuestion(this);
            }
            else if (this.IsLinkedToListQuestion)
            {
                updater.UpdateLinkedToListQuestion(this);
            }

            if (this.IsCascading) // is IsSingleFixedOption too
            {
                updater.UpdateSingleOptionQuestion(this);
                updater.UpdateCascadingQuestion(this);
            }
        }

        public void AcceptValidity(IInterviewTreeUpdater updater)
        {
            updater.UpdateValidations(this);
        }

        public void ReplaceSubstitutions()
        {
            this.Title.ReplaceSubstitutions();
            this.Instructions.ReplaceSubstitutions();
            foreach (var messagesWithSubstition in this.ValidationMessages)
            {
                messagesWithSubstition.ReplaceSubstitutions();
            }
        }

        public override void SetTree(InterviewTree tree)
        {
            base.SetTree(tree);
            this.Title?.SetTree(tree);
            this.Instructions?.SetTree(tree);
            foreach (var messagesWithSubstition in this.ValidationMessages)
            {
                messagesWithSubstition.SetTree(tree);
            }
        }

        public void MarkAsReadonly()
        {
            this.IsReadonly = true;
        }

        public void ProtectAnswer()
        {
            if (this.IsMultiFixedOption) this.GetAsInterviewTreeMultiOptionQuestion().ProtectAnswer();
            else if (this.IsTextList) this.GetAsInterviewTreeTextListQuestion().ProtectAnswer();
            else if (this.IsNumericInteger) this.GetAsInterviewTreeIntegerQuestion().ProtectAnswer();
            else if (this.IsYesNo) this.GetAsInterviewTreeYesNoQuestion().ProtectAnswers();
            else 
                throw new InvalidOperationException($"Can't protect answers for question of type {InterviewQuestionType}");
        }

        public bool HasProtectedAnswer()
        {
            if (this.IsMultiFixedOption) return this.GetAsInterviewTreeMultiOptionQuestion().ProtectedAnswer?.CheckedValues.Count > 0;
            if (this.IsTextList) return this.GetAsInterviewTreeTextListQuestion().ProtectedAnswer?.Rows.Count > 0;
            if (this.IsInteger) return this.GetAsInterviewTreeIntegerQuestion().ProtectedAnswer != null;
            if (this.IsYesNo) return this.GetAsInterviewTreeYesNoQuestion().ProtectedAnswer?.CheckedOptions.Count > 0;

            return false;
        }

        public bool IsAnswerProtected(decimal value)
        {
            if (this.IsMultiFixedOption) return this.GetAsInterviewTreeMultiOptionQuestion().IsAnswerProtected(value);
            if (this.IsTextList) return this.GetAsInterviewTreeTextListQuestion().IsAnswerProtected(value);
            if (this.IsInteger) return this.GetAsInterviewTreeIntegerQuestion().IsAnswerProtected(value);
            if (this.IsYesNo) return this.GetAsInterviewTreeYesNoQuestion().IsAnswerProtected(value);

            return false;
        }
    }

    public abstract class BaseInterviewQuestion
    {
        protected BaseInterviewQuestion(InterviewQuestionType interviewQuestionType)
        {
            this.InterviewQuestionType = interviewQuestionType;
        }
        public InterviewQuestionType InterviewQuestionType { get;}
        public abstract bool IsAnswered();

        public virtual BaseInterviewQuestion Clone()
        {
            return (BaseInterviewQuestion)MemberwiseClone();
        }

        public virtual void RunImportInvariants(InterviewQuestionInvariants questionInvariants)
        {
        }

        public abstract void RemoveAnswer();
        public abstract bool EqualByAnswer(BaseInterviewQuestion question);

        public abstract AbstractAnswer Answer { get; }
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeGpsQuestion : BaseInterviewQuestion
    {
        private GpsAnswer answer;

        public InterviewTreeGpsQuestion() : base(InterviewQuestionType.Gps)
        {
        }

        public InterviewTreeGpsQuestion(object answer) : base(InterviewQuestionType.Gps)
        {
            this.answer = GpsAnswer.FromGeoPosition(answer as GeoPosition);
        }

        public override bool IsAnswered() => this.answer != null;
        public GpsAnswer GetAnswer() => this.answer;
        public void SetAnswer(GpsAnswer answer) => this.answer = answer;
        public override void RemoveAnswer() => this.answer = null;

        public override bool EqualByAnswer(BaseInterviewQuestion question) => (question as InterviewTreeGpsQuestion)?.answer == this.answer;
        public override AbstractAnswer Answer => this.answer;

        public override BaseInterviewQuestion Clone() => (InterviewTreeGpsQuestion) this.MemberwiseClone();

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";

        public override void RunImportInvariants(InterviewQuestionInvariants questionInvariants)
        {
            questionInvariants.RequireGpsCoordinatesPreloadValueAllowed(answer.Value);
        }
    }
    
    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeAudioQuestion : BaseInterviewQuestion
    {
        private AudioAnswer answer;

        public InterviewTreeAudioQuestion() : base(InterviewQuestionType.Audio)
        {
        }

        public InterviewTreeAudioQuestion(object answer, TimeSpan? length) : base(InterviewQuestionType.Audio)
        {
            this.answer = AudioAnswer.FromString(answer as string, length);
        }

        public override bool IsAnswered() => this.answer != null;
        public AudioAnswer GetAnswer() => this.answer;
        public void SetAnswer(AudioAnswer answer) => this.answer = answer;
        public override void RemoveAnswer() => this.answer = null;

        public override bool EqualByAnswer(BaseInterviewQuestion question) => (question as InterviewTreeAudioQuestion)?.answer == this.answer;
        public override AbstractAnswer Answer => this.answer;

        public override BaseInterviewQuestion Clone() => (InterviewTreeAudioQuestion)this.MemberwiseClone();

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeMultimediaQuestion : BaseInterviewQuestion
    {
        private MultimediaAnswer answer;

        public InterviewTreeMultimediaQuestion() : base(InterviewQuestionType.Multimedia)
        {
        }

        public InterviewTreeMultimediaQuestion(object answer, DateTime? answerTime):base(InterviewQuestionType.Multimedia)
        {
            this.answer = MultimediaAnswer.FromString(answer as string, answerTime);
        }

        public override bool IsAnswered() => this.answer != null;
        public MultimediaAnswer GetAnswer() => this.answer;
        public void SetAnswer(MultimediaAnswer answer) => this.answer = answer;
        public override void RemoveAnswer() => this.answer = null;

        public override bool EqualByAnswer(BaseInterviewQuestion question) => (question as InterviewTreeMultimediaQuestion)?.answer == this.answer;

        public override BaseInterviewQuestion Clone() => (InterviewTreeMultimediaQuestion) this.MemberwiseClone();

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";
        public override AbstractAnswer Answer => this.answer;
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeAreaQuestion : BaseInterviewQuestion 
    {
        private AreaAnswer answer;

        public InterviewTreeAreaQuestion() : base(InterviewQuestionType.Area)
        {
        }

        public InterviewTreeAreaQuestion(object answer): base(InterviewQuestionType.Area)
        {
            this.answer = AreaAnswer.FromArea(answer as Area);
        }

        public override bool IsAnswered() => this.answer != null;
        public AreaAnswer GetAnswer() => this.answer;
        public void SetAnswer(AreaAnswer answer) => this.answer = answer;
        public override void RemoveAnswer() => this.answer = null;

        public override bool EqualByAnswer(BaseInterviewQuestion question) => (question as InterviewTreeAreaQuestion)?.answer == this.answer;

        public override BaseInterviewQuestion Clone() => (InterviewTreeAreaQuestion)this.MemberwiseClone();

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";
        public override AbstractAnswer Answer => this.answer;
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeIntegerQuestion : BaseInterviewQuestion
    {
        private NumericIntegerAnswer answer;

        public InterviewTreeIntegerQuestion() : base(InterviewQuestionType.Integer)
        {
        }

        public InterviewTreeIntegerQuestion(object answer) : base (InterviewQuestionType.Integer)
        {
            this.answer = answer == null ? null : NumericIntegerAnswer.FromInt(Convert.ToInt32(answer));
        }

        public NumericIntegerAnswer ProtectedAnswer { get; private set; }

        public override bool IsAnswered() => this.answer != null;
        public virtual NumericIntegerAnswer GetAnswer() => this.answer;
        public void SetAnswer(NumericIntegerAnswer answer) => this.answer = answer;
        public override void RemoveAnswer() => this.answer = null;

        public override bool EqualByAnswer(BaseInterviewQuestion question) => (question as InterviewTreeIntegerQuestion)?.answer == this.answer;

        public override BaseInterviewQuestion Clone() => (InterviewTreeIntegerQuestion)this.MemberwiseClone();

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";

        public override void RunImportInvariants(InterviewQuestionInvariants questionInvariants)
        {
            questionInvariants.RequireNumericIntegerPreloadValueAllowed(answer.Value);
        }

        public void ProtectAnswer()
        {
            this.ProtectedAnswer = GetAnswer();
        }

        public void SetProtectedAnswer(NumericIntegerAnswer value)
        {
            this.ProtectedAnswer = value;
        }

        public bool IsAnswerProtected(decimal value)
        {
            return this.ProtectedAnswer?.Value == (int) value;
        }

        public override AbstractAnswer Answer => this.answer;
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeDoubleQuestion : BaseInterviewQuestion
    {
        private NumericRealAnswer answer;

        public InterviewTreeDoubleQuestion() : base(InterviewQuestionType.Double)
        {
        }

        public InterviewTreeDoubleQuestion(object answer) : base(InterviewQuestionType.Double)
        {
            this.answer = answer == null ? null : NumericRealAnswer.FromDouble(Convert.ToDouble(answer));
        }

        public override bool IsAnswered() => this.answer != null;
        public NumericRealAnswer GetAnswer() => this.answer;
        public void SetAnswer(NumericRealAnswer answer) => this.answer = answer;
        public override void RemoveAnswer() => this.answer = null;

        public override bool EqualByAnswer(BaseInterviewQuestion question) => (question as InterviewTreeDoubleQuestion)?.answer == this.answer;

        public override BaseInterviewQuestion Clone() => (InterviewTreeDoubleQuestion)this.MemberwiseClone();

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";

        public override void RunImportInvariants(InterviewQuestionInvariants questionInvariants)
        {
            questionInvariants.RequireNumericRealPreloadValueAllowed();
        }

        public override AbstractAnswer Answer => this.answer;
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeQRBarcodeQuestion : BaseInterviewQuestion
    {
        private QRBarcodeAnswer answer;

        public InterviewTreeQRBarcodeQuestion() : base(InterviewQuestionType.QRBarcode)
        {
        }

        public InterviewTreeQRBarcodeQuestion(object answer) : base(InterviewQuestionType.QRBarcode)
        {
            this.answer = QRBarcodeAnswer.FromString(answer as string);
        }

        public override bool IsAnswered() => this.answer != null;
        public QRBarcodeAnswer GetAnswer() => this.answer;
        public void SetAnswer(QRBarcodeAnswer answer) => this.answer = answer;
        public override void RemoveAnswer() => this.answer = null;
        public override bool EqualByAnswer(BaseInterviewQuestion question) => (question as InterviewTreeQRBarcodeQuestion)?.answer == this.answer;

        public override BaseInterviewQuestion Clone() => (InterviewTreeQRBarcodeQuestion)this.MemberwiseClone();

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";

        public override void RunImportInvariants(InterviewQuestionInvariants questionInvariants)
        {
            questionInvariants.RequireQRBarcodePreloadValueAllowed();
        }

        public override AbstractAnswer Answer => this.answer;
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeTextQuestion : BaseInterviewQuestion
    {
        private TextAnswer answer;

        public InterviewTreeTextQuestion() : base(InterviewQuestionType.Text)
        {
        }

        public InterviewTreeTextQuestion(object answer) : base(InterviewQuestionType.Text)
        {
            this.answer = TextAnswer.FromString(answer as string);
        }

        public override bool IsAnswered() => this.answer != null;
        public TextAnswer GetAnswer() => this.answer;
        public void SetAnswer(TextAnswer answer) => this.answer = answer;
        public override void RemoveAnswer() => this.answer = null;

        public override bool EqualByAnswer(BaseInterviewQuestion question) => (question as InterviewTreeTextQuestion)?.answer == this.answer;
        
        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";

        public override void RunImportInvariants(InterviewQuestionInvariants questionInvariants)
        {
            questionInvariants.RequireTextPreloadValueAllowed();
        }

        public override AbstractAnswer Answer => this.answer;
    }
    
    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeYesNoQuestion : BaseInterviewQuestion
    {
        private YesNoAnswer answer;

        public InterviewTreeYesNoQuestion() : base(InterviewQuestionType.YesNo)
        {
        }

        public InterviewTreeYesNoQuestion(object answer): this()
        {
            this.answer = YesNoAnswer.FromAnsweredYesNoOptions(answer as AnsweredYesNoOption[]);
        }

        public override bool IsAnswered() => this.answer != null && this.answer.CheckedOptions.Count > 0;
        public YesNoAnswer GetAnswer() => this.answer;
        public void SetAnswer(YesNoAnswer newAnswer) => this.answer = newAnswer;

        public override void RemoveAnswer() => this.answer = null;

        public override bool EqualByAnswer(BaseInterviewQuestion question)
        {
            var interviewTreeYesNoQuestion = question as InterviewTreeYesNoQuestion;
            if (interviewTreeYesNoQuestion == null)
                return false;

            if (interviewTreeYesNoQuestion.answer == null && this.answer == null)
                return true;

            if (interviewTreeYesNoQuestion.answer != null && this.answer != null)
                return interviewTreeYesNoQuestion.answer.ToAnsweredYesNoOptions().SequenceEqual(this.answer.ToAnsweredYesNoOptions());

            return false;
        }

        public void ProtectAnswers()
        {
            this.ProtectedAnswer = GetAnswer();
        }

        public void SetProtectedAnswer(YesNoAnswer yesNoAnswer)
        {
            this.ProtectedAnswer = yesNoAnswer;
        }

        public YesNoAnswer ProtectedAnswer { get; private set; }

        public bool IsAnswerProtected(decimal value)
        {
            return this.ProtectedAnswer?.CheckedOptions.Any(a => a.Value == (int)value) ?? false;
        }

        public override BaseInterviewQuestion Clone() => (InterviewTreeYesNoQuestion) this.MemberwiseClone();

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";

        public override void RunImportInvariants(InterviewQuestionInvariants questionInvariants)
        {
            questionInvariants.RequireYesNoPreloadValueAllowed(answer);
        }

        public override AbstractAnswer Answer => this.answer;
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeTextListQuestion : BaseInterviewQuestion
    {
        private TextListAnswer answer;

        public InterviewTreeTextListQuestion() : base(InterviewQuestionType.TextList)
        {
            this.ProtectedAnswer = null;
        }

        public InterviewTreeTextListQuestion(object answer) : this()
        {
            this.answer = TextListAnswer.FromTupleArray(answer as Tuple<decimal, string>[]);
        }

        public TextListAnswer ProtectedAnswer { get; private set; }

        public override bool IsAnswered() => this.answer != null && this.answer.Rows.Count > 0;
        public virtual TextListAnswer GetAnswer() => this.answer;
        public void SetAnswer(TextListAnswer answer) => this.answer = answer;
        public override void RemoveAnswer() => this.answer = null;

        public override bool EqualByAnswer(BaseInterviewQuestion question)
        {
            var interviewTreeTextListQuestion = question as InterviewTreeTextListQuestion;
            if (interviewTreeTextListQuestion == null)
                return false;
            if (interviewTreeTextListQuestion?.answer == null && this.answer == null)
                return true;

            if (interviewTreeTextListQuestion?.answer != null && this.answer != null)
                return interviewTreeTextListQuestion.answer.ToTupleArray().SequenceEqual(this.answer.ToTupleArray());

            return false;
        }

        public string GetTitleByItemCode(decimal code)
        {
            if (!IsAnswered())
                return string.Empty;

            return this.answer.Rows.FirstOrDefault(row => row.Value == code)?.Text ?? String.Empty;
        }

        public override BaseInterviewQuestion Clone() => (InterviewTreeTextListQuestion) this.MemberwiseClone();

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";

        public override void RunImportInvariants(InterviewQuestionInvariants questionInvariants)
        {
            questionInvariants.RequireTextListPreloadValueAllowed(answer.ToTupleArray());
        }

        public void ProtectAnswer()
        {
            this.ProtectedAnswer = this.GetAnswer();
        }

        public void SetProtectedAnswer(TextListAnswer textListAnswer)
        {
            this.ProtectedAnswer = textListAnswer;
        }

        public bool IsAnswerProtected(decimal value)
        {
            return this.ProtectedAnswer?.Rows.Any(x => x.Value == value) ?? false;
        }

        public override AbstractAnswer Answer => this.answer;
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeSingleOptionQuestion : BaseInterviewQuestion
    {
        private CategoricalFixedSingleOptionAnswer answer;

        public InterviewTreeSingleOptionQuestion() : base(InterviewQuestionType.SingleFixedOption)
        {
        }

        public InterviewTreeSingleOptionQuestion(object answer) : this(answer, InterviewQuestionType.SingleFixedOption)
        {
            this.answer = answer == null ? null : CategoricalFixedSingleOptionAnswer.FromInt(Convert.ToInt32(answer));
        }

        protected InterviewTreeSingleOptionQuestion(object answer, InterviewQuestionType interviewQuestionType) : base(interviewQuestionType)
        {
            this.answer = answer == null ? null : CategoricalFixedSingleOptionAnswer.FromInt(Convert.ToInt32(answer));
        }

        public override bool IsAnswered() => this.answer != null;

        public virtual CategoricalFixedSingleOptionAnswer GetAnswer() => this.answer;

        public void SetAnswer(CategoricalFixedSingleOptionAnswer answer) => this.answer = answer;
        public override void RemoveAnswer() => this.answer = null;

        public override bool EqualByAnswer(BaseInterviewQuestion question) => (question as InterviewTreeSingleOptionQuestion)?.answer == this.answer;

        public override BaseInterviewQuestion Clone() => (InterviewTreeSingleOptionQuestion) this.MemberwiseClone();

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";

        public override void RunImportInvariants(InterviewQuestionInvariants questionInvariants)
        {
            questionInvariants.RequireFixedSingleOptionPreloadValueAllowed(answer.SelectedValue);
        }

        public override AbstractAnswer Answer => this.answer;
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeMultiOptionQuestion : BaseInterviewQuestion
    {
        private CategoricalFixedMultiOptionAnswer answer;

        public InterviewTreeMultiOptionQuestion() : base(InterviewQuestionType.MultiFixedOption)
        {
            this.ProtectedAnswer = null;
        }

        public InterviewTreeMultiOptionQuestion(object answer) : this()
        {
            this.answer = CategoricalFixedMultiOptionAnswer.Convert(answer);
        }

        public override bool IsAnswered() => this.answer != null && this.answer.CheckedValues.Count > 0;
        public CategoricalFixedMultiOptionAnswer GetAnswer() => this.answer;
        public void SetAnswer(CategoricalFixedMultiOptionAnswer answer) => this.answer = answer;
        public override void RemoveAnswer() => this.answer = null;

        public override bool EqualByAnswer(BaseInterviewQuestion question)
        {
            var interviewTreeMultiOptionQuestion = question as InterviewTreeMultiOptionQuestion;
            if (interviewTreeMultiOptionQuestion == null)
                return false;
             if (interviewTreeMultiOptionQuestion.answer == null && this.answer == null)
                return true;

            if (interviewTreeMultiOptionQuestion.answer != null && this.answer != null)
                return interviewTreeMultiOptionQuestion.answer.CheckedValues.SequenceEqual(this.answer.CheckedValues);

            return false;
        }

        public override BaseInterviewQuestion Clone() => (InterviewTreeMultiOptionQuestion) this.MemberwiseClone();

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";

        public override void RunImportInvariants(InterviewQuestionInvariants questionInvariants)
        {
            questionInvariants.RequireFixedMultipleOptionsPreloadValueAllowed(answer.CheckedValues);
        }

        public void ProtectAnswer()
        {
            this.ProtectedAnswer = GetAnswer();
        }

        public void SetProtectedAnswer(CategoricalFixedMultiOptionAnswer answer)
        {
            this.ProtectedAnswer = answer;
        }

        public CategoricalFixedMultiOptionAnswer ProtectedAnswer { get; private set; }

        public bool IsAnswerProtected(decimal value)
        {
            return this.ProtectedAnswer?.CheckedValues.Contains((int) value) ?? false;
        }

        public override AbstractAnswer Answer => this.answer;
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeSingleLinkedToRosterQuestion : InterviewTreeLinkedToRosterQuestion
    {
        private CategoricalLinkedSingleOptionAnswer answer;

        public InterviewTreeSingleLinkedToRosterQuestion(): base(InterviewQuestionType.SingleLinkedOption)
        {
        }

        public InterviewTreeSingleLinkedToRosterQuestion(IEnumerable<RosterVector> linkedOptions, object answer, 
            Guid linkedSourceId, Identity commonParentRosterIdForLinkedQuestion)
            : base(linkedOptions, linkedSourceId, commonParentRosterIdForLinkedQuestion, InterviewQuestionType.SingleLinkedOption)
        {
            this.answer = CategoricalLinkedSingleOptionAnswer.FromRosterVector(answer as RosterVector);
        }

        public override bool IsAnswered() => this.answer != null && this.answer.SelectedValue.Length > 0;
        public virtual CategoricalLinkedSingleOptionAnswer GetAnswer() => this.answer;
        public void SetAnswer(CategoricalLinkedSingleOptionAnswer answer) => this.answer = answer;
        public override void RemoveAnswer() => this.answer = null;

        public override bool EqualByAnswer(BaseInterviewQuestion question) => (question as InterviewTreeSingleLinkedToRosterQuestion)?.answer == this.answer;

        public override BaseInterviewQuestion Clone()
        {
            var clone = (InterviewTreeSingleLinkedToRosterQuestion) this.MemberwiseClone();
            clone.SetOptions(this.Options);
            return clone;
        }

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";

        public override AbstractAnswer Answer => this.answer;
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeMultiLinkedToRosterQuestion : InterviewTreeLinkedToRosterQuestion
    {
        private CategoricalLinkedMultiOptionAnswer answer;
        public InterviewTreeMultiLinkedToRosterQuestion(IEnumerable<RosterVector> linkedOptions, object answer, 
            Guid linkedSourceId, Identity commonParentRosterIdForLinkedQuestion)
            : base(linkedOptions, linkedSourceId, commonParentRosterIdForLinkedQuestion, InterviewQuestionType.MultiLinkedOption)
        {
            this.answer = CategoricalLinkedMultiOptionAnswer.FromRosterVectors(answer as RosterVector[]);
        }

        public override bool IsAnswered() => this.answer != null && this.answer.CheckedValues.Count > 0;
        public CategoricalLinkedMultiOptionAnswer GetAnswer() => this.answer;
        public void SetAnswer(CategoricalLinkedMultiOptionAnswer answer) => this.answer = answer;

        public override void RemoveAnswer() => this.answer = null;

        public override bool EqualByAnswer(BaseInterviewQuestion question)
        {
            var interviewTreeMultiLinkedToRosterQuestion = question as InterviewTreeMultiLinkedToRosterQuestion;
            if (interviewTreeMultiLinkedToRosterQuestion == null)
                return false;
            if (interviewTreeMultiLinkedToRosterQuestion?.answer == null && this.answer == null)
                return true;

            if (interviewTreeMultiLinkedToRosterQuestion?.answer != null && this.answer != null)
                return interviewTreeMultiLinkedToRosterQuestion.answer.CheckedValues.SelectMany(x => x.Coordinates).SequenceEqual(this.answer.CheckedValues.SelectMany(x => x.Coordinates));

            return false;
        }

        public override BaseInterviewQuestion Clone()
        {
            var clone = (InterviewTreeMultiLinkedToRosterQuestion) this.MemberwiseClone();
            clone.SetOptions(this.Options);
            return clone;
        }

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";

        public override AbstractAnswer Answer => this.answer;
    }

    [DebuggerDisplay("{ToString()}")]
    public abstract class InterviewTreeLinkedToRosterQuestion : BaseInterviewQuestion
    {
        public Guid LinkedSourceId { get; private set; }
        public Identity CommonParentRosterIdForLinkedQuestion { get; private set; }

        protected InterviewTreeLinkedToRosterQuestion(InterviewQuestionType interviewQuestionType) :
            base(interviewQuestionType)
        {
        }

        protected InterviewTreeLinkedToRosterQuestion(IEnumerable<RosterVector> linkedOptions, 
            Guid linkedSourceId, Identity commonParentRosterIdForLinkedQuestion, 
            InterviewQuestionType interviewQuestionType) : base(interviewQuestionType)
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
    public abstract class InterviewTreeLinkedToListQuestion : BaseInterviewQuestion
    {
        public Guid LinkedSourceId { get; protected set; }

        public int[] Options { get; protected set; }

        public void SetOptions(IEnumerable<int> options)
        {
            this.Options = options?.ToArray() ?? EmptyArray<int>.Value;
        }

        protected InterviewTreeLinkedToListQuestion(Guid linkedToQuestionId, InterviewQuestionType interviewQuestionType) : base(interviewQuestionType)
        {
            LinkedSourceId = linkedToQuestionId;
        }
    }
    
    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeMultiOptionLinkedToListQuestion : InterviewTreeLinkedToListQuestion
    {
        public InterviewTreeMultiOptionLinkedToListQuestion(object answer, Guid linkedToQuestionId) : base(linkedToQuestionId, InterviewQuestionType.MultiLinkedToList)
        {
            this.answer = CategoricalFixedMultiOptionAnswer.Convert(answer);
            this.Options = EmptyArray<int>.Value;
        }

        private CategoricalFixedMultiOptionAnswer answer;
        public override bool IsAnswered() => this.answer != null && this.answer.CheckedValues.Count > 0;
        public CategoricalFixedMultiOptionAnswer GetAnswer() => this.answer;
        public void SetAnswer(CategoricalFixedMultiOptionAnswer answer) => this.answer = answer;

        public override bool EqualByAnswer(BaseInterviewQuestion question)
        {
            var interviewTreeMultiOptionLinkedToListQuestion = question as InterviewTreeMultiOptionLinkedToListQuestion;
            if (interviewTreeMultiOptionLinkedToListQuestion == null)
                return false;

            if (interviewTreeMultiOptionLinkedToListQuestion?.answer == null && this.answer == null)
                return true;

            if (interviewTreeMultiOptionLinkedToListQuestion?.answer != null && this.answer != null)
                return interviewTreeMultiOptionLinkedToListQuestion.answer.CheckedValues.SequenceEqual(this.answer.CheckedValues);

            return false;
        }

        public override void RemoveAnswer() => this.answer = null;

        public override BaseInterviewQuestion Clone()
        {
            var clone = (InterviewTreeMultiOptionLinkedToListQuestion)this.MemberwiseClone();
            clone.SetOptions(this.Options);
            return clone;
        }

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";

        public override AbstractAnswer Answer => this.answer;
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeSingleOptionLinkedToListQuestion : InterviewTreeLinkedToListQuestion
    {
        public InterviewTreeSingleOptionLinkedToListQuestion(object answer, Guid linkedToQuestionId) : base(linkedToQuestionId, InterviewQuestionType.SingleLinkedToList)
        {
            this.answer = answer == null ? null : CategoricalFixedSingleOptionAnswer.FromInt(Convert.ToInt32(answer));
            this.Options = EmptyArray<int>.Value;
        }

        private CategoricalFixedSingleOptionAnswer answer;
        public override bool IsAnswered() => this.answer != null;
        public CategoricalFixedSingleOptionAnswer GetAnswer() => this.answer;
        public void SetAnswer(CategoricalFixedSingleOptionAnswer answer) => this.answer = answer;
        public override bool EqualByAnswer(BaseInterviewQuestion question) => (question as InterviewTreeSingleOptionLinkedToListQuestion)?.answer == this.answer;
        public override void RemoveAnswer() => this.answer = null;

        public override BaseInterviewQuestion Clone()
        {
            var clone = (InterviewTreeSingleOptionLinkedToListQuestion)this.MemberwiseClone();
            clone.SetOptions(this.Options);
            return clone;
        }

        public override string ToString() => this.answer?.ToString() ?? "NO ANSWER";

        public override AbstractAnswer Answer => this.answer;
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeCascadingQuestion : InterviewTreeSingleOptionQuestion
    {
        private InterviewTreeQuestion question;
        private readonly Guid cascadingParentQuestionId;

        public InterviewTreeCascadingQuestion() : base(InterviewQuestionType.Cascading)
        {
        }

        public InterviewTreeCascadingQuestion(InterviewTreeQuestion question, Guid cascadingParentQuestionId, object answer) : base(answer, InterviewQuestionType.Cascading)
        {
            this.question = question;
            this.cascadingParentQuestionId = cascadingParentQuestionId;
        }

        public InterviewTreeSingleOptionQuestion GetCascadingParentQuestion()
        {
            return (this.question.Parent as InterviewTreeGroup)
                ?.GetQuestionFromThisOrUpperLevel(this.cascadingParentQuestionId).InterviewQuestion as InterviewTreeSingleOptionQuestion;
        }

        public InterviewTreeQuestion GetCascadingParentTreeQuestion()
        {
            
            return (this.question.Parent as InterviewTreeGroup)?.GetQuestionFromThisOrUpperLevel(this.cascadingParentQuestionId);
        }

        public Guid CascadingParentQuestionId => this.cascadingParentQuestionId;

        public override BaseInterviewQuestion Clone()
        {
            var clone = (InterviewTreeCascadingQuestion)this.MemberwiseClone();
            return clone;
        }

        public void SetQuestion(InterviewTreeQuestion question)
        {
            this.question = question;
        }

        public override void RunImportInvariants(InterviewQuestionInvariants questionInvariants)
        {
            var questionnaire = this.question.Tree.Questionnaire;
            questionInvariants.RequireFixedSingleOptionAnswerAllowed(GetAnswer().SelectedValue, new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version));
        }

        public override string ToString() => string.Join(", ", this.GetCascadingParentQuestion()?.GetAnswer());
        
    }
}

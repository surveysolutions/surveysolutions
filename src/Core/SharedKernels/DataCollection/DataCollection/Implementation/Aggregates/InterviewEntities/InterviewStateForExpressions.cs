using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewStateForExpressions : IInterviewStateForExpressions
    {
        private readonly InterviewTree tree;

        public InterviewStateForExpressions(InterviewTree tree, 
            IInterviewPropertiesForExpressions interviewProperties)
        {
            this.tree = tree;
            this.Properties = interviewProperties;
        }

        public T GetAnswer<T>(Guid questionId, IEnumerable<int> rosterVector)
        {  
            T answer = GetAnswerImpl<T>(questionId, rosterVector is RosterVector rv ? rv : new RosterVector(rosterVector));
            return answer;
        }

        public T GetAnswer<T>(Guid questionId, RosterVector rosterVector)
        {
            T answer = GetAnswerImpl<T>(questionId, rosterVector);
            return answer;
        }

        private T GetAnswerImpl<T>(Guid questionId, RosterVector rosterVector)
        {
            var question = this.tree.GetQuestion(new Identity(questionId, rosterVector));

            if ((!question.IsAnswered() || question.IsDisabled()) && !question.IsYesNo) // because of missing field
                return default(T);

            switch (question.InterviewQuestionType)
            {
                case InterviewQuestionType.Integer:
                    var numericIntegerAnswer = question.GetAsInterviewTreeIntegerQuestion().GetAnswer();
                    if (numericIntegerAnswer == null) return default(T);
                    return numericIntegerAnswer.Value.To<T>(); //"int?"
                case InterviewQuestionType.Double:
                    var numericRealAnswer = question.GetAsInterviewTreeDoubleQuestion().GetAnswer();
                    if (numericRealAnswer == null) return default(T);
                    return numericRealAnswer.Value.To<T>(); // double?
                case InterviewQuestionType.SingleFixedOption:
                    var categoricalFixedSingleOptionAnswer = question.GetAsInterviewTreeSingleOptionQuestion().GetAnswer();
                    if (categoricalFixedSingleOptionAnswer == null) return default(T);
                    return categoricalFixedSingleOptionAnswer.SelectedValue.To<T>();//int?
                case InterviewQuestionType.Cascading:
                    var fixedSingleOptionAnswer = question.GetAsInterviewTreeCascadingQuestion().GetAnswer();
                    if (fixedSingleOptionAnswer == null) return default(T);
                    return fixedSingleOptionAnswer.SelectedValue.To<T>();//int?
                case InterviewQuestionType.MultiFixedOption:
                    var categoricalFixedMultiOptionAnswer = question.GetAsInterviewTreeMultiOptionQuestion().GetAnswer();
                    if (categoricalFixedMultiOptionAnswer == null) return default(T);
                    return categoricalFixedMultiOptionAnswer.ToInts().ToArray().To<T>();//int[]
                case InterviewQuestionType.YesNo:
                    if (!question.IsAnswered() || question.IsDisabled())
                    {
                        return new YesNoAndAnswersMissings(
                            this.tree.GetOptionsForQuestion(questionId, null, "").Select(x => x.Value),
                            Array.Empty<CheckedYesNoAnswerOption>()).To<T>(); 
                    }

                    return new YesNoAndAnswersMissings(
                        this.tree.GetOptionsForQuestion(questionId, null, "").Select(x => x.Value),
                        question.GetAsInterviewTreeYesNoQuestion().GetAnswer()?.CheckedOptions).To<T>(); 
                case InterviewQuestionType.SingleLinkedOption:
                    var categoricalLinkedSingleOptionAnswer = question.GetAsInterviewTreeSingleLinkedToRosterQuestion().GetAnswer();
                    if (categoricalLinkedSingleOptionAnswer == null) return default(T);
                    return categoricalLinkedSingleOptionAnswer.SelectedValue.To<T>();//RosterVector
                case InterviewQuestionType.MultiLinkedOption:
                    var categoricalLinkedMultiOptionAnswer = question.GetAsInterviewTreeMultiLinkedToRosterQuestion().GetAnswer();
                    if (categoricalLinkedMultiOptionAnswer == null) return default(T);
                    return categoricalLinkedMultiOptionAnswer.CheckedValues.ToArray().To<T>();//RosterVector[]
                case InterviewQuestionType.Gps:
                    var gpsAnswer = question.GetAsInterviewTreeGpsQuestion().GetAnswer();
                    if (gpsAnswer == null) return default(T);
                    return gpsAnswer.ToGeoLocation().To<T>(); //GeoLocation
                case InterviewQuestionType.TextList:
                    var textListAnswer = question.GetAsInterviewTreeTextListQuestion().GetAnswer();
                    if (textListAnswer == null) return default(T);
                    return textListAnswer.Rows.ToArray().To<T>(); // TextListAnswerRow[]
                case InterviewQuestionType.DateTime:
                    var dateTimeAnswer = question.GetAsInterviewTreeDateTimeQuestion().GetAnswer();
                    if (dateTimeAnswer == null) return default(T);
                    return dateTimeAnswer.Value.To<T>(); //DateTime?
                case InterviewQuestionType.Text:
                    var textAnswer = question.GetAsInterviewTreeTextQuestion().GetAnswer();
                    if (textAnswer == null) return default(T);
                    return textAnswer.Value.To<T>();//string
                case InterviewQuestionType.SingleLinkedToList:
                    var singleOptionAnswer = question.GetAsInterviewTreeSingleOptionLinkedToListQuestion().GetAnswer();
                    if (singleOptionAnswer == null) return default(T);
                    return singleOptionAnswer.SelectedValue.To<T>(); //int?
                case InterviewQuestionType.MultiLinkedToList:
                    var fixedMultiOptionAnswer = question.GetAsInterviewTreeMultiOptionLinkedToListQuestion().GetAnswer();
                    if (fixedMultiOptionAnswer == null) return default(T);
                    return fixedMultiOptionAnswer.ToInts().ToArray().To<T>(); // int[]
                case InterviewQuestionType.Multimedia:
                    var multimediaAnswer = question.GetAsInterviewTreeMultimediaQuestion().GetAnswer();
                    if (multimediaAnswer == null) return default(T);
                    return multimediaAnswer.FileName.To<T>();//string
                case InterviewQuestionType.QRBarcode:
                    var qrBarcodeAnswer = question.GetAsInterviewTreeQRBarcodeQuestion().GetAnswer();
                    if (qrBarcodeAnswer == null) return default(T);
                    return qrBarcodeAnswer.DecodedText.To<T>();//string
                case InterviewQuestionType.Audio:
                    var audioAnswer = question.GetAsInterviewTreeAudioQuestion().GetAnswer();
                    if (audioAnswer == null) return default(T);
                    return audioAnswer.ToAudioAnswerForContions().To<T>(); //AudioAnswerForConditions
                case InterviewQuestionType.Area:
                    var areaAnswer = question.GetAsInterviewTreeAreaQuestion().GetAnswer();
                    if(areaAnswer == null) return default(T);
                    return areaAnswer.ToGeorgaphy().To<T>(); //Area
                default:
                    return default(T);
            }
        }

        public T GetVariable<T>(Guid questionId, IEnumerable<int> rosterVector)
        {
            var variable = this.tree.GetVariable(new Identity(questionId, rosterVector is RosterVector rv ? rv : new RosterVector(rosterVector)));

            if (variable.IsDisabled())
                return default(T);

            return variable.Value.To<T>();
        }

        public IInterviewPropertiesForExpressions Properties { get; }

        public IEnumerable<Identity> FindEntitiesFromSameOrDeeperLevel(Guid entityIdToSearch, Identity startingSearchPointIdentity)
        {
            return this.tree.FindEntitiesFromSameOrDeeperLevel(entityIdToSearch, startingSearchPointIdentity);
        }

        public int GetRosterIndex(Identity rosterIdentity)
        {
            return this.tree.GetRoster(rosterIdentity).SortIndex;
        }

        public string GetRosterTitle(Identity rosterIdentity)
        {
            return this.tree.GetRoster(rosterIdentity).RosterTitle;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewStateForExpressions : IInterviewStateForExpressions
    {
        private readonly InterviewTree tree;
        private readonly IQuestionnaire questionnaire;

        public InterviewStateForExpressions(InterviewTree tree, IQuestionnaire questionnaire, IInterviewPropertiesForExpressions interviewProperties)
        {
            this.tree = tree;
            this.questionnaire = questionnaire;
            this.Properties = interviewProperties;
        }

        public T GetAnswer<T>(Guid questionId, IEnumerable<int> rosterVector) where T : class
        {  
            T answer = GetAnswerImpl<T>(questionId, rosterVector is RosterVector rv ? rv : new RosterVector(rosterVector));
            return answer;
        }

        public T GetAnswer<T>(Guid questionId, RosterVector rosterVector) where T : class
        {
            T answer = GetAnswerImpl<T>(questionId, rosterVector);
            return answer;
        }

        private T GetAnswerImpl<T>(Guid questionId, RosterVector rosterVector) where T: class
        {
            var question = this.tree.GetQuestion(new Identity(questionId, rosterVector));

            if ((!question.IsAnswered() || question.IsDisabled()) && !question.IsYesNo) // because of missing field
                return default(T);

            switch (question.InterviewQuestionType)
            {
                case InterviewQuestionType.Integer:
                    return question.GetAsInterviewTreeIntegerQuestion()?.GetAnswer()?.Value as T; //"int?"
                case InterviewQuestionType.Double:
                    return question.GetAsInterviewTreeDoubleQuestion()?.GetAnswer()?.Value as T; // double?
                case InterviewQuestionType.SingleFixedOption:
                    return question.GetAsInterviewTreeSingleOptionQuestion()?.GetAnswer()?.SelectedValue as T;//int?
                case InterviewQuestionType.Cascading:
                    return question.GetAsInterviewTreeCascadingQuestion()?.GetAnswer()?.SelectedValue as T;//int?
                case InterviewQuestionType.MultiFixedOption:
                    return question.GetAsInterviewTreeMultiOptionQuestion()?.GetAnswer()?.ToInts().ToArray() as T;//int[]
                case InterviewQuestionType.YesNo:
                    return new YesNoAndAnswersMissings(
                        this.questionnaire.GetOptionsForQuestion(questionId, null, "").Select(x => x.Value),
                        question.GetAsInterviewTreeYesNoQuestion()?.GetAnswer()?.CheckedOptions) as T; //YesNoAndAnswersMissings
                case InterviewQuestionType.SingleLinkedOption:
                    return question.GetAsInterviewTreeSingleLinkedToRosterQuestion()?.GetAnswer()?.SelectedValue as T;//RosterVector
                case InterviewQuestionType.MultiLinkedOption:
                    return question.GetAsInterviewTreeMultiLinkedToRosterQuestion()?.GetAnswer()?.CheckedValues.ToArray() as T;//RosterVector[]
                case InterviewQuestionType.Gps:
                    return question.GetAsInterviewTreeGpsQuestion()?.GetAnswer()?.ToGeoLocation() as T; //GeoLocation
                case InterviewQuestionType.TextList:
                    return question.GetAsInterviewTreeTextListQuestion()?.GetAnswer()?.Rows.ToArray() as T; // TextListAnswerRow[]
                case InterviewQuestionType.DateTime:
                    return question.GetAsInterviewTreeDateTimeQuestion()?.GetAnswer()?.Value as T; //DateTime?
                case InterviewQuestionType.Text:
                    return question.GetAsInterviewTreeTextQuestion()?.GetAnswer()?.Value as T;//string
                case InterviewQuestionType.SingleLinkedToList:
                    return question.GetAsInterviewTreeSingleOptionLinkedToListQuestion()?.GetAnswer()?.SelectedValue as T; //int?
                case InterviewQuestionType.MultiLinkedToList:
                    return question.GetAsInterviewTreeMultiOptionLinkedToListQuestion()?.GetAnswer()?.ToInts().ToArray() as T; // int[]
                case InterviewQuestionType.Multimedia:
                    return question.GetAsInterviewTreeMultimediaQuestion()?.GetAnswer()?.FileName as T;//string
                case InterviewQuestionType.QRBarcode:
                    return question.GetAsInterviewTreeQRBarcodeQuestion()?.GetAnswer()?.DecodedText as T;//string
                case InterviewQuestionType.Audio:
                    return question.GetAsInterviewTreeAudioQuestion()?.GetAnswer()?.ToAudioAnswerForContions() as T; //AudioAnswerForConditions
                case InterviewQuestionType.Area:
                    return question.GetAsInterviewTreeAreaQuestion()?.GetAnswer()?.Value as T; //Area
                default:
                    return null;
            }
        }

        public T GetVariable<T>(Guid questionId, IEnumerable<int> rosterVector) where T: class
        {
            var variable = this.tree.GetVariable(new Identity(questionId, rosterVector is RosterVector rv ? rv : new RosterVector(rosterVector)));

            if (variable.IsDisabled())
                return null;

            return variable.Value as T;
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
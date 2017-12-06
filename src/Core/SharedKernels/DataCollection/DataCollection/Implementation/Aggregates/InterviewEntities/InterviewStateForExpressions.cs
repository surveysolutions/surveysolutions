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

        public T GetAnswer<T>(Guid questionId, IEnumerable<int> rosterVector)
        {
            var question = this.tree.GetQuestion(new Identity(questionId, new RosterVector(rosterVector)));
            T answer = GetAnswerImpl<T>(questionId, question);
            return answer;
        }

        public T GetAnswer<T>(Guid questionId, RosterVector rosterVector)
        {
            var question = this.tree.GetQuestion(new Identity(questionId, rosterVector));
            T answer = GetAnswerImpl<T>(questionId, question);
            return answer;
        }

        private T GetAnswerImpl<T>(Guid questionId, InterviewTreeQuestion question)
        {
            if ((!question.IsAnswered() || question.IsDisabled()) && !question.IsYesNo) // because of missing field
                return default(T);

            switch (question.InterviewQuestionType)
            {
                case InterviewQuestionType.Integer:
                    return question.GetAsInterviewTreeIntegerQuestion().GetAnswer().Value.To<T>(); //"int?"
                case InterviewQuestionType.Double:
                    return question.GetAsInterviewTreeDoubleQuestion().GetAnswer().Value.To<T>(); // double?
                case InterviewQuestionType.SingleFixedOption:
                    return question.GetAsInterviewTreeSingleOptionQuestion().GetAnswer().SelectedValue.To<T>();//int?
                case InterviewQuestionType.Cascading:
                    return question.GetAsInterviewTreeCascadingQuestion().GetAnswer().SelectedValue.To<T>();//int?
                case InterviewQuestionType.MultiFixedOption:
                    return question.GetAsInterviewTreeMultiOptionQuestion().GetAnswer().ToInts().ToArray().To<T>();//int[]
                case InterviewQuestionType.YesNo:
                    return new YesNoAndAnswersMissings(
                        this.questionnaire.GetOptionsForQuestion(questionId, null, "").Select(x => x.Value),
                        question.GetAsInterviewTreeYesNoQuestion().GetAnswer()?.CheckedOptions).To<T>(); //YesNoAndAnswersMissings
                case InterviewQuestionType.SingleLinkedOption:
                    return question.GetAsInterviewTreeSingleLinkedToRosterQuestion().GetAnswer().SelectedValue.To<T>();//RosterVector
                case InterviewQuestionType.MultiLinkedOption:
                    return question.GetAsInterviewTreeMultiLinkedToRosterQuestion().GetAnswer().CheckedValues.ToArray().To<T>();//RosterVector[]
                case InterviewQuestionType.Gps:
                    return question.GetAsInterviewTreeGpsQuestion().GetAnswer().ToGeoLocation().To<T>(); //GeoLocation
                case InterviewQuestionType.TextList:
                    return question.GetAsInterviewTreeTextListQuestion().GetAnswer().Rows.ToArray().To<T>(); // TextListAnswerRow[]
                case InterviewQuestionType.DateTime:
                    return question.GetAsInterviewTreeDateTimeQuestion().GetAnswer().Value.To<T>(); //DateTime?
                case InterviewQuestionType.Text:
                    return question.GetAsInterviewTreeTextQuestion().GetAnswer().Value.To<T>();//string
                case InterviewQuestionType.SingleLinkedToList:
                    return question.GetAsInterviewTreeSingleOptionLinkedToListQuestion().GetAnswer().SelectedValue.To<T>(); //int?
                case InterviewQuestionType.MultiLinkedToList:
                    return question.GetAsInterviewTreeMultiOptionLinkedToListQuestion().GetAnswer().ToInts().ToArray().To<T>(); // int[]
                case InterviewQuestionType.Multimedia:
                    return question.GetAsInterviewTreeMultimediaQuestion().GetAnswer().FileName.To<T>();//string
                case InterviewQuestionType.QRBarcode:
                    return question.GetAsInterviewTreeQRBarcodeQuestion().GetAnswer().DecodedText.To<T>();//string
                case InterviewQuestionType.Audio:
                    return question.GetAsInterviewTreeAudioQuestion().GetAnswer().ToAudioAnswerForContions().To<T>(); //AudioAnswerForConditions
                case InterviewQuestionType.Area:
                    return question.GetAsInterviewTreeAreaQuestion().GetAnswer().Value.To<T>(); //Area
                default:
                    return default(T);
            }
        }

        public T GetVariable<T>(Guid questionId, IEnumerable<int> rosterVector)
        {
            var variable = this.tree.GetVariable(new Identity(questionId, new RosterVector(rosterVector)));

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
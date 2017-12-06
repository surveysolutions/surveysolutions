using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage;
using WB.Core.SharedKernels.DataCollection.Portable;

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
            return default(T);
        }
        public T GetAnswer<T>(Guid questionId, RosterVector rosterVector)
        {
            return default(T);
        }

        private InterviewTreeQuestion GetQuestion(Guid questionId, IEnumerable<int> rosterVector)
        {
            var question = this.tree.GetQuestion(new Identity(questionId, new RosterVector(rosterVector)));
            if ((!question.IsAnswered() || question.IsDisabled()) && !question.IsYesNo) // because of missing field
                return null;
            return question;
        }
        private InterviewTreeQuestion GetQuestion(Guid questionId, RosterVector rosterVector)
        {
            var question = this.tree.GetQuestion(new Identity(questionId, rosterVector));
            if ((!question.IsAnswered() || question.IsDisabled()) && !question.IsYesNo) // because of missing field
                return null;
            return question;
        }

        public AudioAnswerForConditions GetAudioAnswer(Guid questionId, RosterVector rosterVector) 
            => GetAudioAnswerImpl(GetQuestion(questionId, rosterVector));

        public AudioAnswerForConditions GetAudioAnswer(Guid questionId, IEnumerable<int> rosterVector) 
            => GetAudioAnswerImpl(GetQuestion(questionId, rosterVector));

        private AudioAnswerForConditions GetAudioAnswerImpl(InterviewTreeQuestion question)
        {
            return question?.GetAsInterviewTreeAudioQuestion().GetAnswer().ToAudioAnswerForContions();
        }

        public DateTime? GetDateTimeAnswer(Guid questionId, RosterVector rosterVector)
            => GetDateTimeAnswerImpl(GetQuestion(questionId, rosterVector));

        public DateTime? GetDateTimeAnswer(Guid questionId, IEnumerable<int> rosterVector)
            => GetDateTimeAnswerImpl(GetQuestion(questionId, rosterVector));

        private DateTime? GetDateTimeAnswerImpl(InterviewTreeQuestion question)
        {
            return question?.GetAsInterviewTreeDateTimeQuestion().GetAnswer().Value;
        }

        public TextListAnswerRow[] GetTextListAnswer(Guid questionId, RosterVector rosterVector)
            => GetTextListAnswerImpl(GetQuestion(questionId, rosterVector));

        public TextListAnswerRow[] GetTextListAnswer(Guid questionId, IEnumerable<int> rosterVector)
            => GetTextListAnswerImpl(GetQuestion(questionId, rosterVector));

        private TextListAnswerRow[] GetTextListAnswerImpl(InterviewTreeQuestion question)
        {
            return question?.GetAsInterviewTreeTextListQuestion().GetAnswer().Rows.ToArray();
        }

        public GeoLocation GetGeoLocationAnswer(Guid questionId, RosterVector rosterVector)
            => GetGeoLocationAnswerImpl(GetQuestion(questionId, rosterVector));

        public GeoLocation GetGeoLocationAnswer(Guid questionId, IEnumerable<int> rosterVector)
            => GetGeoLocationAnswerImpl(GetQuestion(questionId, rosterVector));

        private GeoLocation GetGeoLocationAnswerImpl(InterviewTreeQuestion question)
        {
            return question?.GetAsInterviewTreeGpsQuestion().GetAnswer().ToGeoLocation();
        }

        public RosterVector[] GetRosterVectorArrayAnswer(Guid questionId, RosterVector rosterVector)
            => GetRosterVectorArrayAnswerImpl(GetQuestion(questionId, rosterVector));

        public RosterVector[] GetRosterVectorArrayAnswer(Guid questionId, IEnumerable<int> rosterVector)
            => GetRosterVectorArrayAnswerImpl(GetQuestion(questionId, rosterVector));

        private RosterVector[] GetRosterVectorArrayAnswerImpl(InterviewTreeQuestion question)
        {
            return question?.GetAsInterviewTreeMultiLinkedToRosterQuestion().GetAnswer().CheckedValues.ToArray();
        }

        public RosterVector GetRosterVectorAnswer(Guid questionId, RosterVector rosterVector)
            => GetRosterVectorAnswerImpl(GetQuestion(questionId, rosterVector));

        public RosterVector GetRosterVectorAnswer(Guid questionId, IEnumerable<int> rosterVector)
            => GetRosterVectorAnswerImpl(GetQuestion(questionId, rosterVector));

        private RosterVector GetRosterVectorAnswerImpl(InterviewTreeQuestion question)
        {
            return question.GetAsInterviewTreeSingleLinkedToRosterQuestion().GetAnswer().SelectedValue;
        }

        public YesNoAndAnswersMissings GetYesNoAnswer(Guid questionId, RosterVector rosterVector)
            => GetYesNoAnswerImpl(GetQuestion(questionId, rosterVector));

        public YesNoAndAnswersMissings GetYesNoAnswer(Guid questionId, IEnumerable<int> rosterVector)
            => GetYesNoAnswerImpl(GetQuestion(questionId, rosterVector));

        private YesNoAndAnswersMissings GetYesNoAnswerImpl(InterviewTreeQuestion question)
        {
            if (question == null)
                return new YesNoAndAnswersMissings();
            return new YesNoAndAnswersMissings(
                this.questionnaire.GetOptionsForQuestion(question.Identity.Id, null, "").Select(x => x.Value),
                question.GetAsInterviewTreeYesNoQuestion().GetAnswer()?.CheckedOptions);
        }

        public int[] GetIntArrayAnswer(Guid questionId, RosterVector rosterVector)
            => GetIntArrayAnswerImpl(GetQuestion(questionId, rosterVector));

        public int[] GetIntArrayAnswer(Guid questionId, IEnumerable<int> rosterVector)
            => GetIntArrayAnswerImpl(GetQuestion(questionId, rosterVector));

        private int[] GetIntArrayAnswerImpl(InterviewTreeQuestion question)
        {
            switch (question?.InterviewQuestionType)
            {
                case InterviewQuestionType.MultiFixedOption:
                    return question.GetAsInterviewTreeMultiOptionQuestion().GetAnswer().ToInts().ToArray();
                case InterviewQuestionType.MultiLinkedToList:
                    return question.GetAsInterviewTreeMultiOptionLinkedToListQuestion().GetAnswer().ToInts().ToArray();
            }
            return null;
        }

        public double? GetDoubleAnswer(Guid questionId, RosterVector rosterVector)
            => GetDoubleAnswerImpl(GetQuestion(questionId, rosterVector));

        public double? GetDoubleAnswer(Guid questionId, IEnumerable<int> rosterVector)
            => GetDoubleAnswerImpl(GetQuestion(questionId, rosterVector));

        private double? GetDoubleAnswerImpl(InterviewTreeQuestion question)
        {
            return question?.GetAsInterviewTreeDoubleQuestion().GetAnswer().Value;
        }

        public int? GetIntAnswer(Guid questionId, RosterVector rosterVector)
            => GetIntAnswerImpl(GetQuestion(questionId, rosterVector));

        public int? GetIntAnswer(Guid questionId, IEnumerable<int> rosterVector)
            => GetIntAnswerImpl(GetQuestion(questionId, rosterVector));

        private int? GetIntAnswerImpl(InterviewTreeQuestion question)
        {
            switch (question?.InterviewQuestionType)
            {
                case InterviewQuestionType.Integer:
                    return question.GetAsInterviewTreeIntegerQuestion().GetAnswer().Value;
                case InterviewQuestionType.SingleFixedOption:
                    return question.GetAsInterviewTreeSingleOptionQuestion().GetAnswer().SelectedValue;
                case InterviewQuestionType.Cascading:
                    return question.GetAsInterviewTreeCascadingQuestion().GetAnswer().SelectedValue;
                case InterviewQuestionType.SingleLinkedToList:
                    return question.GetAsInterviewTreeSingleOptionLinkedToListQuestion().GetAnswer().SelectedValue;
            }
            return null;
        }

        public string GetStringAnswer(Guid questionId, RosterVector rosterVector)
            => GetStringAnswerImpl(GetQuestion(questionId, rosterVector));

        public string GetStringAnswer(Guid questionId, IEnumerable<int> rosterVector)
            => GetStringAnswerImpl(GetQuestion(questionId, rosterVector));

        private string GetStringAnswerImpl(InterviewTreeQuestion question)
        {
            switch (question?.InterviewQuestionType)
            {
                case InterviewQuestionType.Text:
                    return question.GetAsInterviewTreeTextQuestion().GetAnswer().Value;
                case InterviewQuestionType.Multimedia:
                    return question.GetAsInterviewTreeMultimediaQuestion().GetAnswer().FileName;
                case InterviewQuestionType.QRBarcode:
                    return question.GetAsInterviewTreeQRBarcodeQuestion().GetAnswer().DecodedText;
            }
            return null;
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
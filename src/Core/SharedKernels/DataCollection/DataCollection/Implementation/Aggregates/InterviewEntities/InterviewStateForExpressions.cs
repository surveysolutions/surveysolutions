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
            var question = this.tree.GetQuestion(questionId, new RosterVector(rosterVector));

            if ((!question.IsAnswered() || question.IsDisabled()) && !question.IsYesNo) // because of missing field
                return default(T);

            if (question.IsInteger) return question.GetAsInterviewTreeIntegerQuestion().GetAnswer().Value.To<T>(); //"int?"
            if (question.IsDouble) return question.GetAsInterviewTreeDoubleQuestion().GetAnswer().Value.To<T>(); // double?

            if (question.IsSingleFixedOption) return question.GetAsInterviewTreeSingleOptionQuestion().GetAnswer().SelectedValue.To<T>();//int?
            if (question.IsMultiFixedOption) return question.GetAsInterviewTreeMultiOptionQuestion().GetAnswer().ToInts().ToArray().To<T>();//int[]
            if (question.IsYesNo)
            {
                return new YesNoAndAnswersMissings(
                    this.questionnaire.GetOptionsForQuestion(questionId, null, "").Select(x => x.Value), 
                    question.GetAsInterviewTreeYesNoQuestion().GetAnswer()?.CheckedOptions).To<T>(); //YesNoAndAnswersMissings
            }

            if (question.IsSingleLinkedOption) return question.GetAsInterviewTreeSingleLinkedToRosterQuestion().GetAnswer().SelectedValue.To<T>();//RosterVector
            if (question.IsMultiLinkedOption) return question.GetAsInterviewTreeMultiLinkedToRosterQuestion().GetAnswer().CheckedValues.ToArray().To<T>();//RosterVector[]

            if (question.IsGps) return question.GetAsInterviewTreeGpsQuestion().GetAnswer().ToGeoLocation().To<T>(); //GeoLocation
            if (question.IsTextList) return question.GetAsInterviewTreeTextListQuestion().GetAnswer().Rows.ToArray().To<T>(); // TextListAnswerRow[]

            if (question.IsDateTime) return question.GetAsInterviewTreeDateTimeQuestion().GetAnswer().Value.To<T>(); //DateTime?
            if (question.IsText) return question.GetAsInterviewTreeTextQuestion().GetAnswer().Value.To<T>();//string

            if (question.IsSingleLinkedToList) return question.GetAsInterviewTreeSingleOptionLinkedToListQuestion().GetAnswer().SelectedValue.To<T>(); //int?
            if (question.IsMultiLinkedToList) return question.GetAsInterviewTreeMultiOptionLinkedToListQuestion().GetAnswer().ToInts().ToArray().To<T>(); // int[]
           
            if (question.IsMultimedia) return question.GetAsInterviewTreeMultimediaQuestion().GetAnswer().FileName.To<T>();//string
            if (question.IsQRBarcode) return question.GetAsInterviewTreeQRBarcodeQuestion().GetAnswer().DecodedText.To<T>();//string

            if (question.IsAudio) return question.GetAsInterviewTreeAudioQuestion().GetAnswer().ToAudioAnswerForContions().To<T>(); //AudioAnswerForConditions

            return default(T);
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
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

            if (question.IsInteger) return question.GetAsIntegerAnswer().Value.To<T>(); //"int?"
            if (question.IsDouble) return question.GetAsDoubleAnswer().Value.To<T>(); // double?

            if (question.IsSingleFixedOption) return question.GetAsSingleFixedOptionAnswer().SelectedValue.To<T>();//int?
            if (question.IsMultiFixedOption) return question.GetAsMultiFixedOptionAnswer().ToInts().ToArray().To<T>();//int[]
            if (question.IsYesNo)
            {
                return new YesNoAndAnswersMissings(
                    this.questionnaire.GetOptionsForQuestion(questionId, null, "").Select(x => x.Value), 
                    question.GetAsYesNoAnswer()?.CheckedOptions).To<T>(); //YesNoAndAnswersMissings
            }

            if (question.IsSingleLinkedOption) return question.GetAsSingleLinkedOptionAnswer().SelectedValue.To<T>();//RosterVector
            if (question.IsMultiLinkedOption) return question.GetAsMultiLinkedOptionAnswer().CheckedValues.ToArray().To<T>();//RosterVector[]

            if (question.IsGps) return question.GetAsGpsAnswer().ToGeoLocation().To<T>(); //GeoLocation
            if (question.IsTextList) return question.GetAsTextListAnswer().Rows.ToArray().To<T>(); // TextListAnswerRow[]

            if (question.IsDateTime) return question.GetAsDateTimeAnswer().Value.To<T>(); //DateTime?
            if (question.IsText) return question.GetAsTextAnswer().Value.To<T>();//string

            if (question.IsSingleLinkedToList) return question.GetAsSingleLinkedToListAnswer().SelectedValue.To<T>(); //int?
            if (question.IsMultiLinkedToList) return question.GetAsMultiLinkedToListAnswer().ToInts().ToArray().To<T>(); // int[]
           
            if (question.IsMultimedia) return question.GetAsMultimediaAnswer().FileName.To<T>();//string
            if (question.IsQRBarcode) return question.GetAsQRBarcodeAnswer().DecodedText.To<T>();//string

            if (question.IsAudio) return question.GetAsAudioAnswer().ToAudioAnswerForContions().To<T>(); //AudioAnswerForConditions

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
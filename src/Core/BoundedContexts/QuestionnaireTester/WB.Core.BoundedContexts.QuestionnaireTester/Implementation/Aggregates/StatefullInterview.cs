using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewInfrastructure
{
    internal class StatefullInterview : Interview
    {
        public Guid QuestionaryId
        {
            get { return base.questionnaireId; }
        }

        public object GetAnswerOnQuestion(Guid questionId, decimal[] rosterVector = null)
        {
//            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(questionId, rosterVector);
//            return this.interviewState.AnswersSupportedInExpressions[questionKey];
            return null;
        }

        #region Interview initialization
        internal override void Apply(InterviewCreated @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(InterviewForTestingCreated @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(InterviewOnClientCreated @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(InterviewSynchronized @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(SynchronizationMetadataApplied @event)
        {
            base.Apply(@event);
        } 
        #endregion

        #region Applying answers

        internal override void Apply(TextQuestionAnswered @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(QRBarcodeQuestionAnswered @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(PictureQuestionAnswered @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(NumericRealQuestionAnswered @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(NumericIntegerQuestionAnswered @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(DateTimeQuestionAnswered @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(SingleOptionQuestionAnswered @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(MultipleOptionsQuestionAnswered @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(GeoLocationQuestionAnswered @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(TextListQuestionAnswered @event)
        {
            base.Apply(@event);
        }


        internal override void Apply(SingleOptionLinkedQuestionAnswered @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(MultipleOptionsLinkedQuestionAnswered @event)
        {
            base.Apply(@event);
        }
        
        #endregion

        internal override void Apply(AnswerCommented @event)
        {
            base.Apply(@event);
        }


        #region Group and question status and validity
        internal override void Apply(AnswersRemoved @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(AnswersDeclaredValid @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(AnswersDeclaredInvalid @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(GroupsDisabled @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(GroupsEnabled @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(QuestionsDisabled @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(QuestionsEnabled @event)
        {
            base.Apply(@event);
        }
        
        #endregion

        #region Roster instances and titles

        internal override void Apply(RosterInstancesTitleChanged @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(RosterInstancesAdded @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(RosterInstancesRemoved @event)
        {
            base.Apply(@event);
        }

        #endregion

        #region Interview status and validity
        
        internal override void Apply(InterviewStatusChanged @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(InterviewCompleted @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(InterviewRestarted @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(InterviewDeclaredValid @event)
        {
            base.Apply(@event);
        }

        internal override void Apply(InterviewDeclaredInvalid @event)
        {
            base.Apply(@event);
        } 

        #endregion
    }
}
using Microsoft.Practices.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class StatefullInterview : Interview
    {
        private InterviewModel interview = null;
        private static IPlainInterviewRepository<InterviewModel> InterviewRepository
        {
            get { return ServiceLocator.Current.GetInstance<IPlainInterviewRepository<InterviewModel>>(); }
        }

        new internal void Apply(InterviewOnClientCreated @event)
        {
            base.Apply(@event);
            interview = new InterviewModel()
            {
                Id = EventSourceId,
                QuestionnaireId = @event.QuestionnaireId,
                QuestionnaireVersion = @event.QuestionnaireVersion
            };
            InterviewRepository.StoreInterview(interview, EventSourceId);
        }

        #region Applying answers

        new internal void Apply(TextQuestionAnswered @event)
        {
            base.Apply(@event);
            
            var id = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            var question = (interview.Answers.ContainsKey(id)) 
                ? (MaskedTextQuestionViewModel)interview.Answers[id]
                : new MaskedTextQuestionViewModel(@event.QuestionId, @event.PropagationVector);

            question.Answer = @event.Answer;

            interview.Answers[id] = question;
        }

        internal override void Apply(QRBarcodeQuestionAnswered @event)
        {
            base.Apply(@event);
            var id = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            var question = (interview.Answers.ContainsKey(id))
                ? (QrBarcodeQuestionViewModel)interview.Answers[id]
                : new QrBarcodeQuestionViewModel(@event.QuestionId, @event.PropagationVector);

            question.Answer = @event.Answer;

            interview.Answers[id] = question;
        }

        internal override void Apply(PictureQuestionAnswered @event)
        {
            base.Apply(@event);
            var id = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            var question = (interview.Answers.ContainsKey(id))
                ? (MultimediaQuestionViewModel)interview.Answers[id]
                : new MultimediaQuestionViewModel(@event.QuestionId, @event.PropagationVector);

            question.PictureFileName = @event.PictureFileName;

            interview.Answers[id] = question;
        }

        internal override void Apply(NumericRealQuestionAnswered @event)
        {
            base.Apply(@event);
            var id = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            var question = (interview.Answers.ContainsKey(id))
                ? (RealNumericQuestionViewModel)interview.Answers[id]
                : new RealNumericQuestionViewModel(@event.QuestionId, @event.PropagationVector);

            question.Answer = @event.Answer;

            interview.Answers[id] = question;
        }

        internal override void Apply(NumericIntegerQuestionAnswered @event)
        {
            base.Apply(@event);
            var id = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            var question = (interview.Answers.ContainsKey(id))
                ? (IntegerNumericQuestionViewModel)interview.Answers[id]
                : new IntegerNumericQuestionViewModel(@event.QuestionId, @event.PropagationVector);

            question.Answer = @event.Answer;

            interview.Answers[id] = question;
        }

        internal override void Apply(DateTimeQuestionAnswered @event)
        {
            base.Apply(@event);
            var id = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            var question = (interview.Answers.ContainsKey(id))
                ? (DateTimeQuestionViewModel)interview.Answers[id]
                : new DateTimeQuestionViewModel(@event.QuestionId, @event.PropagationVector);

            question.Answer = @event.Answer;

            interview.Answers[id] = question;
        }

        internal override void Apply(SingleOptionQuestionAnswered @event)
        {
            base.Apply(@event);
            var id = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            var question = (interview.Answers.ContainsKey(id))
                ? (SingleOptionQuestionModel)interview.Answers[id]
                : new SingleOptionQuestionModel(@event.QuestionId, @event.PropagationVector);

            question.Answer = @event.SelectedValue;

            interview.Answers[id] = question;
        }

        internal override void Apply(MultipleOptionsQuestionAnswered @event)
        {
            base.Apply(@event);
            var id = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            var question = (interview.Answers.ContainsKey(id))
                ? (MultiOptionQuestionViewModel)interview.Answers[id]
                : new MultiOptionQuestionViewModel(@event.QuestionId, @event.PropagationVector);

            question.Answers = @event.SelectedValues;

            interview.Answers[id] = question;
        }

        internal override void Apply(GeoLocationQuestionAnswered @event)
        {
            base.Apply(@event);
            var id = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            var question = (interview.Answers.ContainsKey(id))
                ? (GpsCoordinatesQuestionViewModel)interview.Answers[id]
                : new GpsCoordinatesQuestionViewModel(@event.QuestionId, @event.PropagationVector);

            question.Latitude = @event.Latitude;
            question.Longitude = @event.Longitude;
            question.Accuracy = @event.Accuracy;
            question.Altitude = @event.Altitude;

            interview.Answers[id] = question;
        }

        internal override void Apply(TextListQuestionAnswered @event)
        {
            base.Apply(@event);
            var id = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            var textAnswer = (interview.Answers.ContainsKey(id))
                ? (TextListQuestionViewModel)interview.Answers[id]
                : new TextListQuestionViewModel(@event.QuestionId, @event.PropagationVector);

            textAnswer.Answers = @event.Answers;

            interview.Answers[id] = textAnswer;
        }


        internal override void Apply(SingleOptionLinkedQuestionAnswered @event)
        {
            base.Apply(@event);
            var id = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            var textAnswer = (interview.Answers.ContainsKey(id))
                ? (LinkedSingleOptionQuestionViewModel)interview.Answers[id]
                : new LinkedSingleOptionQuestionViewModel(@event.QuestionId, @event.PropagationVector);

            textAnswer.Answer = @event.SelectedPropagationVector;

            interview.Answers[id] = textAnswer;
        }

        internal override void Apply(MultipleOptionsLinkedQuestionAnswered @event)
        {
            base.Apply(@event);
            var id = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            var textAnswer = (interview.Answers.ContainsKey(id))
                ? (LinkedMultiOptionQuestionViewModel)interview.Answers[id]
                : new LinkedMultiOptionQuestionViewModel(@event.QuestionId, @event.PropagationVector);

            textAnswer.Answers = @event.SelectedPropagationVectors;

            interview.Answers[id] = textAnswer;
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
using System;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class StatefullInterview : Interview
    {
        private InterviewModel interview;
        private static IPlainInterviewRepository<InterviewModel> InterviewRepository
        {
            get { return ServiceLocator.Current.GetInstance<IPlainInterviewRepository<InterviewModel>>(); }
        }

        internal new void Apply(InterviewOnClientCreated @event)
        {
            base.Apply(@event);
            interview = new InterviewModel
            {
                Id = EventSourceId,
                QuestionnaireId = @event.QuestionnaireId,
                QuestionnaireVersion = @event.QuestionnaireVersion
            };

            IQuestionnaire questionnaire = GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            questionnaire
                .GetAllUnderlyingQuestions(this.questionnaireId)
                .ForEach(questionId => 
                    interview.QuestionIdToQuestionModelTypeMap.Add(questionId, GetQuestionModelType(questionnaire, questionId)));

            InterviewRepository.StoreInterview(interview, EventSourceId);
        }

        // should migrate to QuestionnaireModel
        private static QuestionModelType GetQuestionModelType(IQuestionnaire questionnaire, Guid questionId)
        {
            var questionType = questionnaire.GetQuestionType(questionId);
            switch (questionType)
            {
                case QuestionType.SingleOption:
                    return questionnaire.IsQuestionLinked(questionId) 
                        ? QuestionModelType.LinkedSingleOptionQuestionModel 
                        : QuestionModelType.SingleOptionQuestionModel;

                case QuestionType.MultyOption:
                    return questionnaire.IsQuestionLinked(questionId)
                       ? QuestionModelType.LinkedMultiOptionQuestionModel
                       : QuestionModelType.MultiOptionQuestionModel;

                case QuestionType.Numeric:
                    return questionnaire.IsQuestionInteger(questionId)
                       ? QuestionModelType.IntegerNumericQuestionModel
                       : QuestionModelType.RealNumericQuestionModel;

                case QuestionType.DateTime:
                    return QuestionModelType.DateTimeQuestionModel;

                case QuestionType.GpsCoordinates:
                    return QuestionModelType.GpsCoordinatesQuestionModel;

                case QuestionType.Text:
                    return QuestionModelType.MaskedTextQuestionModel;

                case QuestionType.TextList:
                    return QuestionModelType.TextListQuestionModel;

                case QuestionType.QRBarcode:
                    return QuestionModelType.QrBarcodeQuestionModel;

                case QuestionType.Multimedia:
                    return QuestionModelType.MultimediaQuestionModel;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region Applying answers

        internal new void Apply(TextQuestionAnswered @event)
        {
            base.Apply(@event);
            UpdateAnswer<MaskedTextQuestionModel>(@event, x => x.Answer = @event.Answer);
        }

        internal new void Apply(QRBarcodeQuestionAnswered @event)
        {
            base.Apply(@event);
            UpdateAnswer<QrBarcodeQuestionModel>(@event, x => x.Answer = @event.Answer);
        }

        internal new void Apply(PictureQuestionAnswered @event)
        {
            base.Apply(@event);
            UpdateAnswer<MultimediaQuestionModel>(@event, x => x.PictureFileName = @event.PictureFileName);
        }

        internal new void Apply(NumericRealQuestionAnswered @event)
        {
            base.Apply(@event);
            UpdateAnswer<RealNumericQuestionModel>(@event, x => x.Answer = @event.Answer);
        }

        internal new void Apply(NumericIntegerQuestionAnswered @event)
        {
            base.Apply(@event);
            UpdateAnswer<IntegerNumericQuestionModel>(@event, x => x.Answer = @event.Answer);
        }

        internal new void Apply(DateTimeQuestionAnswered @event)
        {
            base.Apply(@event);
            UpdateAnswer<DateTimeQuestionModel>(@event, x => x.Answer = @event.Answer);
        }

        internal new void Apply(SingleOptionQuestionAnswered @event)
        {
            base.Apply(@event);
            UpdateAnswer<SingleOptionQuestionModel>(@event, x => x.Answer = @event.SelectedValue);
        }

        internal new void Apply(MultipleOptionsQuestionAnswered @event)
        {
            base.Apply(@event);
            UpdateAnswer<MultiOptionQuestionModel>(@event, x => x.Answers = @event.SelectedValues);
        }

        internal new void Apply(GeoLocationQuestionAnswered @event)
        {
            base.Apply(@event);
            UpdateAnswer<GpsCoordinatesQuestionModel>(@event,
                x =>
                {
                    x.Latitude = @event.Latitude;
                    x.Longitude = @event.Longitude;
                    x.Accuracy = @event.Accuracy;
                    x.Altitude = @event.Altitude;
                });
        }

        internal new void Apply(TextListQuestionAnswered @event)
        {
            base.Apply(@event);
            UpdateAnswer<TextListQuestionModel>(@event, x => x.Answers = @event.Answers);
        }

        internal new void Apply(SingleOptionLinkedQuestionAnswered @event)
        {
            base.Apply(@event);
            UpdateAnswer<LinkedSingleOptionQuestionModel>(@event, x => x.Answer = @event.SelectedPropagationVector);
        }

        internal new void Apply(MultipleOptionsLinkedQuestionAnswered @event)
        {
            base.Apply(@event);
            UpdateAnswer<LinkedMultiOptionQuestionModel>(@event, x => x.Answers = @event.SelectedPropagationVectors);
        }
       
        #endregion

        internal new void Apply(AnswerCommented @event)
        {
            base.Apply(@event);
            this.UpdateState(@event.QuestionId, @event.PropagationVector, x => x.Comments.Add(@event.Comment));
        }

        #region Group and question status and validity
        internal override void Apply(AnswersRemoved @event)
        {
            base.Apply(@event);
            @event.Questions.ForEach(x => interview.Answers.Remove(ConversionHelper.ConvertIdAndRosterVectorToString(x.Id, x.RosterVector)));
        }

        internal override void Apply(AnswersDeclaredValid @event)
        {
            base.Apply(@event);
            @event.Questions.ForEach(x => this.UpdateState(x.Id, x.RosterVector, q => q.QuestionState |= QuestionState.Valid));
        }

        internal override void Apply(AnswersDeclaredInvalid @event)
        {
            base.Apply(@event);
            @event.Questions.ForEach(x => this.UpdateState(x.Id, x.RosterVector, q => q.QuestionState &= ~QuestionState.Valid));
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
            @event.Questions.ForEach(x => this.UpdateState(x.Id, x.RosterVector, q => q.QuestionState &= ~QuestionState.Enabled));
        }

        internal override void Apply(QuestionsEnabled @event)
        {
            base.Apply(@event);
            @event.Questions.ForEach(x => this.UpdateState(x.Id, x.RosterVector, q => q.QuestionState |= QuestionState.Enabled));
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

        private void UpdateAnswer<T>(QuestionActiveEvent @event, Action<T> update)
           where T : AbstractInterviewQuestionModel, new()
        {
            var questionId = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            var question = (interview.Answers.ContainsKey(questionId))
                ? (T)interview.Answers[questionId]
                : new T { Id = @event.QuestionId, RosterVector = @event.PropagationVector };

            update(question);

            interview.Answers[questionId] = question;
        }

        private void UpdateState(Guid id, decimal[] rosterVector, Action<AbstractInterviewQuestionModel> update)
        {
            var questionId = ConversionHelper.ConvertIdAndRosterVectorToString(id, rosterVector);

            AbstractInterviewQuestionModel question;
            if (this.interview.Answers.ContainsKey(questionId))
            {
                question = this.interview.Answers[questionId];
            }
            else
            {
                var questionModelType = this.interview.QuestionIdToQuestionModelTypeMap[id];
                var questionActivator = this.interview.QuestionModelTypeToModelActivatorMap[questionModelType];
                question = questionActivator();
                question.Id = id;
                question.RosterVector = rosterVector;
            }

            update(question);

            this.interview.Answers[questionId] = question;
        }
    }
}
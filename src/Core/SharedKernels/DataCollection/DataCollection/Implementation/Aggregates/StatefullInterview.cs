using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;

using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class StatefullInterview : Interview
    {
        private InterviewModel interview;
        private static IPlainRepository<InterviewModel> InterviewRepository
        {
            get { return ServiceLocator.Current.GetInstance<IPlainRepository<InterviewModel>>(); }
        }

        private static IPlainRepository<QuestionnaireModel> QuestionnaireModelRepository
        {
            get { return ServiceLocator.Current.GetInstance<IPlainRepository<QuestionnaireModel>>(); }
        }

        internal new void Apply(InterviewOnClientCreated @event)
        {
            base.Apply(@event);
            interview = new InterviewModel
            {
                Id = EventSourceId,
                QuestionnaireId = @event.QuestionnaireId.FormatGuid(),
                QuestionnaireVersion = @event.QuestionnaireVersion
            };

            var questionnaire = QuestionnaireModelRepository.Get(questionnaireId.FormatGuid());
            interview.QuestionIdToQuestionModelTypeMap = questionnaire.Questions.ToDictionary(x => x.Key, x => x.Value.Type);
                
            InterviewRepository.Store(interview, EventSourceId.FormatGuid());
        }

        #region Applying answers

        internal new void Apply(TextQuestionAnswered @event)
        {
            base.Apply(@event);
            UpdateAnswer<MaskedTextAnswerModel>(@event, x => x.Answer = @event.Answer);
        }

        internal new void Apply(QRBarcodeQuestionAnswered @event)
        {
            base.Apply(@event);
            UpdateAnswer<QrBarcodeAnswerModel>(@event, x => x.Answer = @event.Answer);
        }

        internal new void Apply(PictureQuestionAnswered @event)
        {
            base.Apply(@event);
            UpdateAnswer<MultimediaAnswerModel>(@event, x => x.PictureFileName = @event.PictureFileName);
        }

        internal new void Apply(NumericRealQuestionAnswered @event)
        {
            base.Apply(@event);
            UpdateAnswer<RealNumericAnswerModel>(@event, x => x.Answer = @event.Answer);
        }

        internal new void Apply(NumericIntegerQuestionAnswered @event)
        {
            base.Apply(@event);
            UpdateAnswer<IntegerNumericAnswerModel>(@event, x => x.Answer = @event.Answer);
        }

        internal new void Apply(DateTimeQuestionAnswered @event)
        {
            base.Apply(@event);
            UpdateAnswer<DateTimeAnswerModel>(@event, x => x.Answer = @event.Answer);
        }

        internal new void Apply(SingleOptionQuestionAnswered @event)
        {
            base.Apply(@event);
            UpdateAnswer<SingleOptionAnswerModel>(@event, x => x.Answer = @event.SelectedValue);
        }

        internal new void Apply(MultipleOptionsQuestionAnswered @event)
        {
            base.Apply(@event);
            UpdateAnswer<MultiOptionAnswerModel>(@event, x => x.Answers = @event.SelectedValues);
        }

        internal new void Apply(GeoLocationQuestionAnswered @event)
        {
            base.Apply(@event);
            UpdateAnswer<GpsCoordinatesAnswerModel>(@event,
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
            UpdateAnswer<TextListAnswerModel>(@event, x => x.Answers = @event.Answers);
        }

        internal new void Apply(SingleOptionLinkedQuestionAnswered @event)
        {
            base.Apply(@event);
            UpdateAnswer<LinkedSingleOptionAnswerModel>(@event, x => x.Answer = @event.SelectedPropagationVector);
        }

        internal new void Apply(MultipleOptionsLinkedQuestionAnswered @event)
        {
            base.Apply(@event);
            UpdateAnswer<LinkedMultiOptionAnswerModel>(@event, x => x.Answers = @event.SelectedPropagationVectors);
        }
       
        #endregion

        internal new void Apply(AnswerCommented @event)
        {
            base.Apply(@event);
            this.UpdateAnswerState(@event.QuestionId, @event.PropagationVector, x => x.Comments.Add(@event.Comment));
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
            @event.Questions.ForEach(x => this.UpdateAnswerState(x.Id, x.RosterVector, q => q.QuestionState |= QuestionState.Valid));
        }

        internal override void Apply(AnswersDeclaredInvalid @event)
        {
            base.Apply(@event);
            @event.Questions.ForEach(x => this.UpdateAnswerState(x.Id, x.RosterVector, q => q.QuestionState &= ~QuestionState.Valid));
        }

        internal override void Apply(GroupsDisabled @event)
        {
            base.Apply(@event);
            @event.Groups.ForEach(x => this.UpdateGroupState(x.Id, x.RosterVector, q => q.IsDisabled = true));
        }

        internal override void Apply(GroupsEnabled @event)
        {
            base.Apply(@event);
            @event.Groups.ForEach(x => this.UpdateGroupState(x.Id, x.RosterVector, q => q.IsDisabled = false));
        }

        internal override void Apply(QuestionsDisabled @event)
        {
            base.Apply(@event);
            @event.Questions.ForEach(x => this.UpdateAnswerState(x.Id, x.RosterVector, q => q.QuestionState &= ~QuestionState.Enabled));
        }

        internal override void Apply(QuestionsEnabled @event)
        {
            base.Apply(@event);
            @event.Questions.ForEach(x => this.UpdateAnswerState(x.Id, x.RosterVector, q => q.QuestionState |= QuestionState.Enabled));
        }
        
        #endregion

        #region Roster instances and titles

        internal override void Apply(RosterInstancesTitleChanged @event)
        {
            base.Apply(@event);
            foreach (var changedRosterInstanceTitle in @event.ChangedInstances)
            {
                var rosterId = ConversionHelper.ConvertIdAndRosterVectorToString(changedRosterInstanceTitle.RosterInstance.GroupId, GetFullRosterVector(changedRosterInstanceTitle.RosterInstance));
                var roster = (InterviewRosterModel)interview.Groups[rosterId];
                roster.Title = changedRosterInstanceTitle.Title;
            }
        }

        internal override void Apply(RosterInstancesAdded @event)
        {
            base.Apply(@event);

            foreach (var rosterInstance in @event.Instances)
            {
                var rosterId = ConversionHelper.ConvertIdAndRosterVectorToString(rosterInstance.GroupId, GetFullRosterVector(rosterInstance));
                var rosterParentId = ConversionHelper.ConvertIdAndRosterVectorToString(rosterInstance.GroupId, rosterInstance.OuterRosterVector);

                interview.Groups[rosterId] = new InterviewRosterModel
                                             {
                                                 Id = rosterInstance.GroupId,
                                                 RosterVector = GetFullRosterVector(rosterInstance),
                                                 ParentRosterVector = rosterInstance.OuterRosterVector,
                                                 RowCode = rosterInstance.RosterInstanceId
                                             };
                if (!interview.RosterInstancesIds.ContainsKey(rosterParentId))
                    interview.RosterInstancesIds.Add(rosterParentId, new List<string>());

                interview.RosterInstancesIds[rosterParentId].Add(rosterId);
            }
        }

        internal override void Apply(RosterInstancesRemoved @event)
        {
            base.Apply(@event);
            foreach (var rosterInstance in @event.Instances)
            {
                var rosterId = ConversionHelper.ConvertIdAndRosterVectorToString(rosterInstance.GroupId, this.GetFullRosterVector(rosterInstance));
                var rosterParentId = ConversionHelper.ConvertIdAndRosterVectorToString(rosterInstance.GroupId, rosterInstance.OuterRosterVector);

                interview.Groups.Remove(rosterId);
                interview.RosterInstancesIds[rosterParentId].Remove(rosterId);
            }
        }

        #endregion

        #region Interview status and validity
        
        internal override void Apply(InterviewCompleted @event)
        {
            base.Apply(@event);
            interview.IsInProgress = false;
        }

        internal override void Apply(InterviewRestarted @event)
        {
            base.Apply(@event);
            interview.IsInProgress = true;
        }

        internal override void Apply(InterviewDeclaredValid @event)
        {
            base.Apply(@event);
            interview.HasErrors = false;
        }

        internal override void Apply(InterviewDeclaredInvalid @event)
        {
            base.Apply(@event);
            interview.HasErrors = true;
        }
        
        #endregion

        private void UpdateAnswer<T>(QuestionActiveEvent @event, Action<T> update)
           where T : AbstractInterviewAnswerModel, new()
        {
            var questionId = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            var question = (interview.Answers.ContainsKey(questionId))
                ? (T)interview.Answers[questionId]
                : new T { Id = @event.QuestionId, RosterVector = @event.PropagationVector };

            update(question);

            interview.Answers[questionId] = question;
        }

        private void UpdateGroupState(Guid id, decimal[] rosterVector, Action<InterviewGroupModel> update)
        {
            var groupId = ConversionHelper.ConvertIdAndRosterVectorToString(id, rosterVector);

            InterviewGroupModel groupOrRoster;
            if (this.interview.Groups.ContainsKey(groupId))
            {
                groupOrRoster = this.interview.Groups[groupId];
            }
            else
            {
                // rosters cannot be created here
                groupOrRoster = new InterviewGroupModel { Id = id, RosterVector = rosterVector };
            }

            update(groupOrRoster);

            this.interview.Groups[groupId] = groupOrRoster;
        }

        private void UpdateAnswerState(Guid id, decimal[] rosterVector, Action<AbstractInterviewAnswerModel> update)
        {
            var questionId = ConversionHelper.ConvertIdAndRosterVectorToString(id, rosterVector);

            AbstractInterviewAnswerModel answer;
            if (this.interview.Answers.ContainsKey(questionId))
            {
                answer = this.interview.Answers[questionId];
            }
            else
            {
                var questionModelType = this.interview.QuestionIdToQuestionModelTypeMap[id];
                var questionActivator = this.interview.QuestionModelTypeToModelActivatorMap[questionModelType];
                answer = questionActivator();
                answer.Id = id;
                answer.RosterVector = rosterVector;
            }

            update(answer);

            this.interview.Answers[questionId] = answer;
        }

        private decimal[] GetFullRosterVector(RosterInstance instance)
        {
            return instance.OuterRosterVector.Concat(new[] { instance.RosterInstanceId }).ToArray();
        }
    }
}
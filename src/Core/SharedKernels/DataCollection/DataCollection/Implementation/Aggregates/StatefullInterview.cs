using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class StatefullInterview : Interview
    {
        private InterviewModel interview;

        public InterviewModel InterviewModel
        {
            get { return interview; }
        }

        private static IPlainKeyValueStorage<QuestionnaireModel> QuestionnaireModelRepository
        {
            get { return ServiceLocator.Current.GetInstance<IPlainKeyValueStorage<QuestionnaireModel>>(); }
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

            var questionnaire = QuestionnaireModelRepository.GetById(questionnaireId.FormatGuid());
            interview.QuestionIdToQuestionModelTypeMap = questionnaire.Questions.ToDictionary(x => x.Key, x => x.Value.GetType());
        }

        #region Applying answers

        internal new void Apply(TextQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<MaskedTextAnswerModel>(@event);
            answer.Answer = @event.Answer;
        }

        internal new void Apply(QRBarcodeQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<QrBarcodeAnswerModel>(@event);
            answer.Answer = @event.Answer;
        }

        internal new void Apply(PictureQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<MultimediaAnswerModel>(@event);
            answer.PictureFileName = @event.PictureFileName;
        }

        internal new void Apply(NumericRealQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<RealNumericAnswerModel>(@event);

            answer.Answer = @event.Answer;
        }

        internal new void Apply(NumericIntegerQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<IntegerNumericAnswerModel>(@event);
            answer.Answer = @event.Answer;
        }

        internal new void Apply(DateTimeQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<DateTimeAnswerModel>(@event);
            answer.Answer = @event.Answer;
        }

        internal new void Apply(SingleOptionQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<SingleOptionAnswerModel>(@event);
            answer.Answer = @event.SelectedValue;
        }

        internal new void Apply(MultipleOptionsQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<MultiOptionAnswerModel>(@event);
            answer.Answers = @event.SelectedValues;
        }

        internal new void Apply(GeoLocationQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<GpsCoordinatesAnswerModel>(@event);
            answer.Latitude = @event.Latitude;
            answer.Longitude = @event.Longitude;
            answer.Accuracy = @event.Accuracy;
            answer.Altitude = @event.Altitude;
        }

        internal new void Apply(TextListQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<TextListAnswerModel>(@event);
            answer.Answers = @event.Answers;
        }

        internal new void Apply(SingleOptionLinkedQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<LinkedSingleOptionAnswerModel>(@event);
            answer.Answer = @event.SelectedPropagationVector;
        }

        internal new void Apply(MultipleOptionsLinkedQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<LinkedMultiOptionAnswerModel>(@event);
            answer.Answers = @event.SelectedPropagationVectors;
        }
       
        #endregion

        internal new void Apply(AnswerCommented @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer(@event.QuestionId, @event.PropagationVector);
            answer.Comments.Add(@event.Comment);
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
            @event.Questions.ForEach(x => {
                var answer = this.GetOrCreateAnswer(x.Id, x.RosterVector);
                answer.QuestionState |= QuestionState.Valid;
            });
        }

        internal override void Apply(AnswersDeclaredInvalid @event)
        {
            base.Apply(@event);
            @event.Questions.ForEach(x => {
                var answer = this.GetOrCreateAnswer(x.Id, x.RosterVector);
                answer.QuestionState &= ~QuestionState.Valid;
            });
        }

        internal override void Apply(GroupsDisabled @event)
        {
            base.Apply(@event);
            @event.Groups.ForEach(x => {
                var groupOrRoster = this.GetOrCreateGroupOrRoster(x.Id, x.RosterVector);
                groupOrRoster.IsDisabled = true;
            });
        }

        internal override void Apply(GroupsEnabled @event)
        {
            base.Apply(@event);
            @event.Groups.ForEach(x =>
            {
                var groupOrRoster = this.GetOrCreateGroupOrRoster(x.Id, x.RosterVector);
                groupOrRoster.IsDisabled = false;
            });
        }

        internal override void Apply(QuestionsDisabled @event)
        {
            base.Apply(@event);
            @event.Questions.ForEach(x => {
                var answer = this.GetOrCreateAnswer(x.Id, x.RosterVector);
                answer.QuestionState &= ~QuestionState.Enabled;
            });
        }

        internal override void Apply(QuestionsEnabled @event)
        {
            base.Apply(@event);
            @event.Questions.ForEach(x => {
                var answer = this.GetOrCreateAnswer(x.Id, x.RosterVector);
                answer.QuestionState |= QuestionState.Enabled;
            });
        }
        
        #endregion

        #region Roster instances and titles

        internal override void Apply(RosterInstancesTitleChanged @event)
        {
            base.Apply(@event);
            foreach (var changedRosterInstanceTitle in @event.ChangedInstances)
            {
                var rosterKey = ConversionHelper.ConvertIdAndRosterVectorToString(changedRosterInstanceTitle.RosterInstance.GroupId, GetFullRosterVector(changedRosterInstanceTitle.RosterInstance));
                var roster = (InterviewRosterModel)interview.Groups[rosterKey];
                roster.Title = changedRosterInstanceTitle.Title;
            }
        }

        internal override void Apply(RosterInstancesAdded @event)
        {
            base.Apply(@event);

            foreach (var rosterInstance in @event.Instances)
            {
                var rosterKey = ConversionHelper.ConvertIdAndRosterVectorToString(rosterInstance.GroupId, GetFullRosterVector(rosterInstance));
                var rosterParentKey = ConversionHelper.ConvertIdAndRosterVectorToString(rosterInstance.GroupId, rosterInstance.OuterRosterVector);

                interview.Groups[rosterKey] = new InterviewRosterModel
                {
                    Id = rosterInstance.GroupId,
                    RosterVector = GetFullRosterVector(rosterInstance),
                    ParentRosterVector = rosterInstance.OuterRosterVector,
                    RowCode = rosterInstance.RosterInstanceId
                };
                if (!interview.RosterInstancesIds.ContainsKey(rosterParentKey))
                    interview.RosterInstancesIds.Add(rosterParentKey, new List<string>());

                interview.RosterInstancesIds[rosterParentKey].Add(rosterKey);
            }
        }

        internal override void Apply(RosterInstancesRemoved @event)
        {
            base.Apply(@event);
            foreach (var rosterInstance in @event.Instances)
            {
                var rosterKey = ConversionHelper.ConvertIdAndRosterVectorToString(rosterInstance.GroupId, GetFullRosterVector(rosterInstance));
                var rosterParentKey = ConversionHelper.ConvertIdAndRosterVectorToString(rosterInstance.GroupId, rosterInstance.OuterRosterVector);

                interview.Groups.Remove(rosterKey);
                interview.RosterInstancesIds[rosterParentKey].Remove(rosterKey);
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

        private T GetOrCreateAnswer<T>(QuestionActiveEvent @event) where T : AbstractInterviewAnswerModel, new()
        {
            var questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            var question = (this.interview.Answers.ContainsKey(questionKey))
                ? (T)this.interview.Answers[questionKey]
                : new T { Id = @event.QuestionId, RosterVector = @event.PropagationVector };

            this.interview.Answers[questionKey] = question;
            return question;
        }

        private InterviewGroupModel GetOrCreateGroupOrRoster(Guid id, decimal[] rosterVector)
        {
            var groupKey = ConversionHelper.ConvertIdAndRosterVectorToString(id, rosterVector);

            InterviewGroupModel groupOrRoster;
            if (this.interview.Groups.ContainsKey(groupKey))
            {
                groupOrRoster = this.interview.Groups[groupKey];
            }
            else
            {
                // rosters must be created with event RosterInstancesAdded. Groups has no such event, so
                // they should be created eventually. So if group model was not found that means that
                // we got event about group that was not created previously. Rosters should not be created here.
                groupOrRoster = new InterviewGroupModel { Id = id, RosterVector = rosterVector };
            }

            this.interview.Groups[groupKey] = groupOrRoster;
            return groupOrRoster;
        }

        private AbstractInterviewAnswerModel GetOrCreateAnswer(Guid id, decimal[] rosterVector)
        {
            var questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(id, rosterVector);

            AbstractInterviewAnswerModel answer;
            if (this.interview.Answers.ContainsKey(questionKey))
            {
                answer = this.interview.Answers[questionKey];
            }
            else
            {
                var questionModelType = this.interview.QuestionIdToQuestionModelTypeMap[id];
                var questionActivator = this.interview.QuestionModelTypeToAnswerModelActivatorMap[questionModelType];
                answer = questionActivator();
                answer.Id = id;
                answer.RosterVector = rosterVector;
            }
            this.interview.Answers[questionKey] = answer;
            return answer;
        }

        private static decimal[] GetFullRosterVector(RosterInstance instance)
        {
            return instance.OuterRosterVector.Concat(new[] { instance.RosterInstanceId }).ToArray();
        }
    }
}
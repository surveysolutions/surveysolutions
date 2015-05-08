using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

using Identity = WB.Core.SharedKernels.DataCollection.Identity;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates
{
    public class StatefulInterview : Interview, IStatefulInterview
    {
        private Dictionary<string, BaseInterviewAnswer> answers;
        private Dictionary<string, InterviewGroup> groups;
        private Dictionary<string, List<string>> rosterInstancesIds;
        private Dictionary<Guid, Type> questionIdToQuestionModelTypeMap;

        public string QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }

        public Guid Id { get; set; }

        public Dictionary<string, BaseInterviewAnswer> Answers
        {
            get { return this.answers ?? (this.answers = new Dictionary<string, BaseInterviewAnswer>()); }
        }

        public Dictionary<string, InterviewGroup> Groups
        {
            get { return this.groups ?? (this.groups = new Dictionary<string, InterviewGroup>()); }
        }

        public Dictionary<string, List<string>> RosterInstancesIds
        {
            get { return this.rosterInstancesIds ?? (this.rosterInstancesIds = new Dictionary<string, List<string>>()); }
        }

        public Dictionary<Guid, Type> QuestionIdToQuestionModelTypeMap
        {
            get { return this.questionIdToQuestionModelTypeMap ?? (this.questionIdToQuestionModelTypeMap = new Dictionary<Guid, Type>()); }
            set { this.questionIdToQuestionModelTypeMap = value; }
        }

        public bool HasErrors { get; set; }
        public bool IsInProgress { get; set; }

        private readonly Dictionary<Type, Func<BaseInterviewAnswer>> questionModelTypeToAnswerModelActivatorMap = new Dictionary<Type, Func<BaseInterviewAnswer>>
        {
            { typeof(SingleOptionQuestionModel), () => new SingleOptionAnswer()},
            { typeof(LinkedSingleOptionQuestionModel), () => new LinkedSingleOptionAnswer()},
            { typeof(MultiOptionQuestionModel), () => new MultiOptionAnswer()},
            { typeof(LinkedMultiOptionQuestionModel), () => new LinkedMultiOptionAnswer()},
            { typeof(IntegerNumericQuestionModel), () => new IntegerNumericAnswer()},
            { typeof(RealNumericQuestionModel), () => new RealNumericAnswer()},
            { typeof(MaskedTextQuestionModel), () => new MaskedTextAnswer()},
            { typeof(TextListQuestionModel), () => new TextListAnswer()},
            { typeof(QrBarcodeQuestionModel), () => new QrBarcodeAnswer()},
            { typeof(MultimediaQuestionModel), () => new MultimediaAnswer()},
            { typeof(DateTimeQuestionModel), () => new DateTimeAnswer()},
            { typeof(GpsCoordinatesQuestionModel), () => new GpsCoordinatesAnswer()}
        };

        public GpsCoordinatesAnswer GetGpsCoordinatesAnswerModel(Identity identity)
        {
            return this.GetQuestionModel<GpsCoordinatesAnswer>(identity);
        }

        public DateTimeAnswer GetDateTimeAnswerModel(Identity identity)
        {
            return this.GetQuestionModel<DateTimeAnswer>(identity);
        }

        public MultimediaAnswer GetMultimediaAnswerModel(Identity identity)
        {
            return this.GetQuestionModel<MultimediaAnswer>(identity);
        }

        public QrBarcodeAnswer GetQrBarcodeAnswerModel(Identity identity)
        {
            return this.GetQuestionModel<QrBarcodeAnswer>(identity);
        }

        public TextListAnswer GetTextListAnswerModel(Identity identity)
        {
            return this.GetQuestionModel<TextListAnswer>(identity);
        }

        public LinkedSingleOptionAnswer GetLinkedSingleOptionAnswerModel(Identity identity)
        {
            return this.GetQuestionModel<LinkedSingleOptionAnswer>(identity);
        }

        public MultiOptionAnswer GetMultiOptionAnswerModel(Identity identity)
        {
            return this.GetQuestionModel<MultiOptionAnswer>(identity);
        }

        public LinkedMultiOptionAnswer GetLinkedMultiOptionAnswerModel(Identity identity)
        {
            return this.GetQuestionModel<LinkedMultiOptionAnswer>(identity);
        }

        public IntegerNumericAnswer GetIntegerNumericAnswerModel(Identity identity)
        {
            return this.GetQuestionModel<IntegerNumericAnswer>(identity);
        }

        public RealNumericAnswer GetRealNumericAnswerModel(Identity identity)
        {
            return this.GetQuestionModel<RealNumericAnswer>(identity);
        }

        public MaskedTextAnswer GetTextAnswerModel(Identity identity)
        {
            return this.GetQuestionModel<MaskedTextAnswer>(identity);
        }

        public SingleOptionAnswer GetSingleOptionAnswerModel(Identity identity)
        {
            return this.GetQuestionModel<SingleOptionAnswer>(identity);
        }

        private T GetQuestionModel<T>(Identity identity) where T : BaseInterviewAnswer
        {
            var questionId = ConversionHelper.ConvertIdentityToString(identity);
            if (!this.Answers.ContainsKey(questionId)) return null;
            return (T)this.Answers[questionId];
        }


        public bool IsValid(Identity identity)
        {
            var questionId = ConversionHelper.ConvertIdentityToString(identity);
            if (!this.Answers.ContainsKey(questionId))
                return true;

            var interviewAnswerModel = this.Answers[questionId];
            return !interviewAnswerModel.IsInvalid();
        }

        public bool IsEnabled(Identity entityIdentity)
        {
            var entityKey = ConversionHelper.ConvertIdentityToString(entityIdentity);

            if (this.Groups.ContainsKey(entityKey))
            {
                return !this.Groups[entityKey].IsDisabled;
            }

            if (this.Answers.ContainsKey(entityKey))
            {
                return !this.Answers[entityKey].IsDisabled();
            }

            return true;
        }

        private static IPlainKeyValueStorage<QuestionnaireModel> QuestionnaireModelRepository
        {
            get { return ServiceLocator.Current.GetInstance<IPlainKeyValueStorage<QuestionnaireModel>>(); }
        }

        protected new void Apply(InterviewOnClientCreated @event)
        {
            base.Apply(@event);

            this.Id = this.EventSourceId;
            this.QuestionnaireId = @event.QuestionnaireId.FormatGuid();
            this.QuestionnaireVersion = @event.QuestionnaireVersion;

            var questionnaire = QuestionnaireModelRepository.GetById(this.questionnaireId.FormatGuid());
            this.QuestionIdToQuestionModelTypeMap = questionnaire.Questions.ToDictionary(x => x.Key, x => x.Value.GetType());
        }

        #region Applying answers

        internal new void Apply(TextQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<MaskedTextAnswer>(@event);
            answer.Answer = @event.Answer;
        }

        internal new void Apply(QRBarcodeQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<QrBarcodeAnswer>(@event);
            answer.Answer = @event.Answer;
        }

        internal new void Apply(PictureQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<MultimediaAnswer>(@event);
            answer.PictureFileName = @event.PictureFileName;
        }

        internal new void Apply(NumericRealQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<RealNumericAnswer>(@event);

            answer.Answer = @event.Answer;
        }

        internal new void Apply(NumericIntegerQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<IntegerNumericAnswer>(@event);
            answer.Answer = @event.Answer;
        }

        internal new void Apply(DateTimeQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<DateTimeAnswer>(@event);
            answer.Answer = @event.Answer;
        }

        internal new void Apply(SingleOptionQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<SingleOptionAnswer>(@event);
            answer.Answer = @event.SelectedValue;
        }

        internal new void Apply(MultipleOptionsQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<MultiOptionAnswer>(@event);
            answer.Answers = @event.SelectedValues;
        }

        internal new void Apply(GeoLocationQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<GpsCoordinatesAnswer>(@event);
            answer.Latitude = @event.Latitude;
            answer.Longitude = @event.Longitude;
            answer.Accuracy = @event.Accuracy;
            answer.Altitude = @event.Altitude;
        }

        internal new void Apply(TextListQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<TextListAnswer>(@event);
            answer.Answers = @event.Answers;
        }

        internal new void Apply(SingleOptionLinkedQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<LinkedSingleOptionAnswer>(@event);
            answer.Answer = @event.SelectedPropagationVector;
        }

        internal new void Apply(MultipleOptionsLinkedQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<LinkedMultiOptionAnswer>(@event);
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

        public virtual void Apply(AnswersRemoved @event)
        {
            base.Apply(@event);
            @event.Questions.ForEach(x => this.Answers.Remove(ConversionHelper.ConvertIdAndRosterVectorToString(x.Id, x.RosterVector)));
        }

        public virtual void Apply(AnswersDeclaredValid @event)
        {
            base.Apply(@event);
            @event.Questions.ForEach(x => {
                var answer = this.GetOrCreateAnswer(x.Id, x.RosterVector);
                answer.QuestionState |= QuestionState.Valid;
            });
        }

        public override void Apply(AnswersDeclaredInvalid @event)
        {
            base.Apply(@event);
            @event.Questions.ForEach(x => {
                var answer = this.GetOrCreateAnswer(x.Id, x.RosterVector);
                answer.QuestionState &= ~QuestionState.Valid;
            });
        }

        public virtual void Apply(GroupsDisabled @event)
        {
            base.Apply(@event);
            @event.Groups.ForEach(x => {
                var groupOrRoster = this.GetOrCreateGroupOrRoster(x.Id, x.RosterVector);
                groupOrRoster.IsDisabled = true;
            });
        }

        public virtual void Apply(GroupsEnabled @event)
        {
            base.Apply(@event);
            @event.Groups.ForEach(x =>
            {
                var groupOrRoster = this.GetOrCreateGroupOrRoster(x.Id, x.RosterVector);
                groupOrRoster.IsDisabled = false;
            });
        }

        public virtual void Apply(QuestionsDisabled @event)
        {
            base.Apply(@event);
            @event.Questions.ForEach(x => {
                var answer = this.GetOrCreateAnswer(x.Id, x.RosterVector);
                answer.QuestionState &= ~QuestionState.Enabled;
            });
        }

        public virtual void Apply(QuestionsEnabled @event)
        {
            base.Apply(@event);
            @event.Questions.ForEach(x => {
                var answer = this.GetOrCreateAnswer(x.Id, x.RosterVector);
                answer.QuestionState |= QuestionState.Enabled;
            });
        }
        
        #endregion

        #region Roster instances and titles

        public virtual void Apply(RosterInstancesTitleChanged @event)
        {
            base.Apply(@event);
            foreach (var changedRosterInstanceTitle in @event.ChangedInstances)
            {
                var rosterKey = ConversionHelper.ConvertIdAndRosterVectorToString(changedRosterInstanceTitle.RosterInstance.GroupId, GetFullRosterVector(changedRosterInstanceTitle.RosterInstance));
                var roster = (InterviewRoster)this.Groups[rosterKey];
                roster.Title = changedRosterInstanceTitle.Title;
            }
        }

        public virtual void Apply(RosterInstancesAdded @event)
        {
            base.Apply(@event);

            foreach (var rosterInstance in @event.Instances)
            {
                var rosterKey = ConversionHelper.ConvertIdAndRosterVectorToString(rosterInstance.GroupId, GetFullRosterVector(rosterInstance));
                var rosterParentKey = ConversionHelper.ConvertIdAndRosterVectorToString(rosterInstance.GroupId, rosterInstance.OuterRosterVector);

                this.Groups[rosterKey] = new InterviewRoster
                {
                    Id = rosterInstance.GroupId,
                    RosterVector = GetFullRosterVector(rosterInstance),
                    ParentRosterVector = rosterInstance.OuterRosterVector,
                    RowCode = rosterInstance.RosterInstanceId
                };
                if (!this.RosterInstancesIds.ContainsKey(rosterParentKey))
                    this.RosterInstancesIds.Add(rosterParentKey, new List<string>());

                this.RosterInstancesIds[rosterParentKey].Add(rosterKey);
            }
        }

        public virtual void Apply(RosterInstancesRemoved @event)
        {
            base.Apply(@event);
            foreach (var rosterInstance in @event.Instances)
            {
                var rosterKey = ConversionHelper.ConvertIdAndRosterVectorToString(rosterInstance.GroupId, GetFullRosterVector(rosterInstance));
                var rosterParentKey = ConversionHelper.ConvertIdAndRosterVectorToString(rosterInstance.GroupId, rosterInstance.OuterRosterVector);

                this.Groups.Remove(rosterKey);
                this.RosterInstancesIds[rosterParentKey].Remove(rosterKey);
            }
        }

        #endregion

        #region Interview status and validity

        public virtual void Apply(InterviewCompleted @event)
        {
            base.Apply(@event);
            this.IsInProgress = false;
        }

        public virtual void Apply(InterviewRestarted @event)
        {
            base.Apply(@event);
            this.IsInProgress = true;
        }

        public virtual void Apply(InterviewDeclaredValid @event)
        {
            base.Apply(@event);
            this.HasErrors = false;
        }

        public virtual void Apply(InterviewDeclaredInvalid @event)
        {
            base.Apply(@event);
            this.HasErrors = true;
        }
        
        #endregion

        private T GetOrCreateAnswer<T>(QuestionActiveEvent @event) where T : BaseInterviewAnswer, new()
        {
            var questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            var question = (this.Answers.ContainsKey(questionKey))
                ? (T)this.Answers[questionKey]
                : new T { Id = @event.QuestionId, RosterVector = @event.PropagationVector };

            this.Answers[questionKey] = question;
            return question;
        }

        private InterviewGroup GetOrCreateGroupOrRoster(Guid id, decimal[] rosterVector)
        {
            var groupKey = ConversionHelper.ConvertIdAndRosterVectorToString(id, rosterVector);

            InterviewGroup groupOrRoster;
            if (this.Groups.ContainsKey(groupKey))
            {
                groupOrRoster = this.Groups[groupKey];
            }
            else
            {
                // rosters must be created with event RosterInstancesAdded. Groups has no such event, so
                // they should be created eventually. So if group model was not found that means that
                // we got event about group that was not created previously. Rosters should not be created here.
                groupOrRoster = new InterviewGroup { Id = id, RosterVector = rosterVector };
            }

            this.Groups[groupKey] = groupOrRoster;
            return groupOrRoster;
        }

        private BaseInterviewAnswer GetOrCreateAnswer(Guid id, decimal[] rosterVector)
        {
            var questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(id, rosterVector);

            BaseInterviewAnswer answer;
            if (this.Answers.ContainsKey(questionKey))
            {
                answer = this.Answers[questionKey];
            }
            else
            {
                var questionModelType = this.QuestionIdToQuestionModelTypeMap[id];
                var questionActivator = this.questionModelTypeToAnswerModelActivatorMap[questionModelType];
                answer = questionActivator();
                answer.Id = id;
                answer.RosterVector = rosterVector;
            }
            this.Answers[questionKey] = answer;
            return answer;
        }

        private static decimal[] GetFullRosterVector(RosterInstance instance)
        {
            return instance.OuterRosterVector.Concat(new[] { instance.RosterInstanceId }).ToArray();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

using Identity = WB.Core.SharedKernels.DataCollection.Identity;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates
{
    public class StatefulInterview : Interview, IStatefulInterview
    {
        private Dictionary<string, BaseInterviewAnswer> answers;
        private Dictionary<string, InterviewGroup> groups;
        private Dictionary<string, List<Identity>> rosterInstancesIds;
        private Dictionary<Guid, Type> questionIdToQuestionModelTypeMap;

        private static IPlainKeyValueStorage<QuestionnaireModel> QuestionnaireModelRepository
        {
            get { return ServiceLocator.Current.GetInstance<IPlainKeyValueStorage<QuestionnaireModel>>(); }
        }
      
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
            { typeof(QRBarcodeQuestionModel), () => new QRBarcodeAnswer()},
            { typeof(MultimediaQuestionModel), () => new MultimediaAnswer()},
            { typeof(DateTimeQuestionModel), () => new DateTimeAnswer()},
            { typeof(GpsCoordinatesQuestionModel), () => new GpsCoordinatesAnswer()}
        };

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
            answer.SetAnswer(@event.Answer);
        }

        internal new void Apply(QRBarcodeQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<QRBarcodeAnswer>(@event);
            answer.SetAnswer(@event.Answer);
        }

        internal new void Apply(PictureQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<MultimediaAnswer>(@event);
            answer.SetAnswer(@event.PictureFileName);
        }

        internal new void Apply(NumericRealQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<RealNumericAnswer>(@event);
            answer.SetAnswer(@event.Answer);
        }

        internal new void Apply(NumericIntegerQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<IntegerNumericAnswer>(@event);
            answer.SetAnswer(@event.Answer);
        }

        internal new void Apply(DateTimeQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<DateTimeAnswer>(@event);
            answer.SetAnswer(@event.Answer);
        }

        internal new void Apply(SingleOptionQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<SingleOptionAnswer>(@event);
            answer.SetAnswer(@event.SelectedValue);
        }

        internal new void Apply(MultipleOptionsQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<MultiOptionAnswer>(@event);
            answer.SetAnswers(@event.SelectedValues);
        }

        internal new void Apply(GeoLocationQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<GpsCoordinatesAnswer>(@event);
            answer.SetAnswer(@event.Latitude, @event.Longitude, @event.Accuracy, @event.Altitude);
        }

        internal new void Apply(TextListQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<TextListAnswer>(@event);
            answer.SetAnswers(@event.Answers);
        }

        internal new void Apply(SingleOptionLinkedQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<LinkedSingleOptionAnswer>(@event);
            answer.SetAnswer(@event.SelectedPropagationVector);
        }

        internal new void Apply(MultipleOptionsLinkedQuestionAnswered @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer<LinkedMultiOptionAnswer>(@event);
            answer.SetAnswers(@event.SelectedPropagationVectors);
        }

        #endregion

        internal new void Apply(AnswerCommented @event)
        {
            base.Apply(@event);
            var answer = this.GetOrCreateAnswer(@event.QuestionId, @event.PropagationVector);
            answer.InterviewerComment = @event.Comment;
        }

        #region Group and question status and validity

        public virtual void Apply(AnswersRemoved @event)
        {
            base.Apply(@event);
            @event.Questions.ForEach(x =>
                this.Answers[ConversionHelper.ConvertIdAndRosterVectorToString(x.Id, x.RosterVector)].RemoveAnswer()
            );
        }

        public virtual void Apply(AnswersDeclaredValid @event)
        {
            base.Apply(@event);
            @event.Questions.ForEach(x =>
            {
                var answer = this.GetOrCreateAnswer(x.Id, x.RosterVector);
                answer.IsValid = true;
            });
        }

        public override void Apply(AnswersDeclaredInvalid @event)
        {
            base.Apply(@event);
            @event.Questions.ForEach(x =>
            {
                var answer = this.GetOrCreateAnswer(x.Id, x.RosterVector);
                answer.IsValid = false;
            });
        }

        public virtual void Apply(GroupsDisabled @event)
        {
            base.Apply(@event);
            @event.Groups.ForEach(x =>
            {
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
            @event.Questions.ForEach(x =>
            {
                var answer = this.GetOrCreateAnswer(x.Id, x.RosterVector);
                answer.IsEnabled = false;
            });
        }

        public virtual void Apply(QuestionsEnabled @event)
        {
            base.Apply(@event);
            @event.Questions.ForEach(x =>
            {
                var answer = this.GetOrCreateAnswer(x.Id, x.RosterVector);
                answer.IsEnabled = true;
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
                var rosterIdentity = new Identity(rosterInstance.GroupId, GetFullRosterVector(rosterInstance));

                var rosterKey = ConversionHelper.ConvertIdentityToString(rosterIdentity);
                var rosterParentKey = ConversionHelper.ConvertIdAndRosterVectorToString(rosterInstance.GroupId, rosterInstance.OuterRosterVector);

                this.Groups[rosterKey] = new InterviewRoster
                {
                    Id = rosterIdentity.Id,
                    RosterVector = rosterIdentity.RosterVector,
                    ParentRosterVector = rosterInstance.OuterRosterVector,
                    RowCode = rosterInstance.RosterInstanceId
                };
                if (!this.RosterInstancesIds.ContainsKey(rosterParentKey))
                    this.RosterInstancesIds.Add(rosterParentKey, new List<Identity>());

                this.RosterInstancesIds[rosterParentKey].Add(rosterIdentity);
            }
        }

        public virtual void Apply(RosterInstancesRemoved @event)
        {
            base.Apply(@event);
            foreach (var rosterInstance in @event.Instances)
            {
                var fullRosterVector = GetFullRosterVector(rosterInstance);

                var rosterKey = ConversionHelper.ConvertIdAndRosterVectorToString(rosterInstance.GroupId, fullRosterVector);
                var rosterParentKey = ConversionHelper.ConvertIdAndRosterVectorToString(rosterInstance.GroupId, rosterInstance.OuterRosterVector);

                var rosterIdentity = this.RosterInstancesIds[rosterParentKey].Find(roster => roster.Id == rosterInstance.GroupId &&
                                                                                             roster.RosterVector.SequenceEqual(fullRosterVector));

                this.Groups.Remove(rosterKey);
                this.RosterInstancesIds[rosterParentKey].Remove(rosterIdentity);
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

        public Dictionary<string, List<Identity>> RosterInstancesIds
        {
            get { return this.rosterInstancesIds ?? (this.rosterInstancesIds = new Dictionary<string, List<Identity>>()); }
        }

        public Dictionary<Guid, Type> QuestionIdToQuestionModelTypeMap
        {
            get { return this.questionIdToQuestionModelTypeMap ?? (this.questionIdToQuestionModelTypeMap = new Dictionary<Guid, Type>()); }
            set { this.questionIdToQuestionModelTypeMap = value; }
        }

        public bool HasErrors { get; set; }

        public bool IsInProgress { get; set; }

        public GpsCoordinatesAnswer GetGpsCoordinatesAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<GpsCoordinatesAnswer>(identity);
        }

        public DateTimeAnswer GetDateTimeAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<DateTimeAnswer>(identity);
        }

        public MultimediaAnswer GetMultimediaAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<MultimediaAnswer>(identity);
        }

        public QRBarcodeAnswer GetQRBarcodeAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<QRBarcodeAnswer>(identity);
        }

        public TextListAnswer GetTextListAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<TextListAnswer>(identity);
        }

        public LinkedSingleOptionAnswer GetLinkedSingleOptionAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<LinkedSingleOptionAnswer>(identity);
        }

        public MultiOptionAnswer GetMultiOptionAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<MultiOptionAnswer>(identity);
        }

        public LinkedMultiOptionAnswer GetLinkedMultiOptionAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<LinkedMultiOptionAnswer>(identity);
        }

        public IntegerNumericAnswer GetIntegerNumericAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<IntegerNumericAnswer>(identity);
        }

        public RealNumericAnswer GetRealNumericAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<RealNumericAnswer>(identity);
        }

        public MaskedTextAnswer GetTextAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<MaskedTextAnswer>(identity);
        }

        public SingleOptionAnswer GetSingleOptionAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<SingleOptionAnswer>(identity);
        }

        public bool IsValid(Identity identity)
        {
            var questionKey = ConversionHelper.ConvertIdentityToString(identity);
            if (!this.Answers.ContainsKey(questionKey))
                return true;

            var interviewAnswerModel = this.Answers[questionKey];
            return interviewAnswerModel.IsValid;
        }

        public bool IsEnabled(Identity entityIdentity)
        {
            var entityKey = ConversionHelper.ConvertIdentityToString(entityIdentity);

            if (this.Groups.ContainsKey(entityKey))
            {
                var group = this.Groups[entityKey];
                return !group.IsDisabled;
            }

            if (this.Answers.ContainsKey(entityKey))
            {
                var answer = this.Answers[entityKey];
                return answer.IsEnabled;
            }

            return true;
        }

        public bool WasAnswered(Identity entityIdentity)
        {
            var questionKey = ConversionHelper.ConvertIdentityToString(entityIdentity);
            if (!Answers.ContainsKey(questionKey))
                return false;

            var interviewAnswerModel = Answers[questionKey];
            return interviewAnswerModel.IsAnswered;
        }

        public string GetInterviewerAnswerComment(Identity entityIdentity)
        {
            var questionKey = ConversionHelper.ConvertIdentityToString(entityIdentity);
            if (!Answers.ContainsKey(questionKey))
                return null;

            var interviewAnswerModel = Answers[questionKey];
            return interviewAnswerModel.InterviewerComment;
        }

        private T GetQuestionAnswer<T>(Identity identity) where T : BaseInterviewAnswer
        {
            var questionKey = ConversionHelper.ConvertIdentityToString(identity);
            if (!this.Answers.ContainsKey(questionKey)) return null;
            return (T)this.Answers[questionKey];
        }

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
                this.Answers[questionKey] = answer;
            }
            return answer;
        }

        private static decimal[] GetFullRosterVector(RosterInstance instance)
        {
            return instance.OuterRosterVector.Concat(new[] { instance.RosterInstanceId }).ToArray();
        }
    }
}
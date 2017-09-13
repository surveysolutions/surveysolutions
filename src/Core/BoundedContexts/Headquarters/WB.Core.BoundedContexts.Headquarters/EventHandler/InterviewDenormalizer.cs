using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class InterviewDenormalizer : BaseDenormalizer,
        IEventHandler<GroupPropagated>,
        IEventHandler<RosterInstancesAdded>,
        IEventHandler<RosterInstancesRemoved>,
        IEventHandler<MultipleOptionsQuestionAnswered>,
        IEventHandler<NumericRealQuestionAnswered>,
        IEventHandler<NumericIntegerQuestionAnswered>,
        IEventHandler<TextQuestionAnswered>,
        IEventHandler<TextListQuestionAnswered>,
        IEventHandler<SingleOptionQuestionAnswered>,
        IEventHandler<SingleOptionLinkedQuestionAnswered>,
        IEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        IEventHandler<DateTimeQuestionAnswered>,
        IEventHandler<GeoLocationQuestionAnswered>,
        IEventHandler<QRBarcodeQuestionAnswered>,
        IEventHandler<PictureQuestionAnswered>,
        IEventHandler<YesNoQuestionAnswered>,
        IEventHandler<AnswersRemoved>,
        IEventHandler<GroupsDisabled>,
        IEventHandler<GroupsEnabled>,
        IEventHandler<StaticTextsEnabled>,
        IEventHandler<StaticTextsDisabled>,
        IEventHandler<StaticTextsDeclaredInvalid>,
        IEventHandler<StaticTextsDeclaredValid>,
        IEventHandler<QuestionsDisabled>,
        IEventHandler<QuestionsEnabled>,
        IEventHandler<AnswersDeclaredInvalid>,
        IEventHandler<AnswersDeclaredValid>,
        IEventHandler<InterviewHardDeleted>,
        IEventHandler<AnswerRemoved>,
        IEventHandler<QuestionsMarkedAsReadonly>,

        IEventHandler<VariablesChanged>,
        IEventHandler<VariablesDisabled>,
        IEventHandler<VariablesEnabled>,
        IEventHandler<AreaQuestionAnswered>,
        IEventHandler<AudioQuestionAnswered>
    {
        private readonly IInterviewFactory repository;

        public InterviewDenormalizer(IInterviewFactory interviewFactory)
        {
            this.repository = interviewFactory;
        }


        public override object[] Writers => new object[0];

        public void Handle(IPublishedEvent<GroupPropagated> evnt)
            => this.repository.AddRosters(evnt.EventSourceId, new[] {Identity.Create(evnt.Payload.GroupId, evnt.Payload.OuterScopeRosterVector)});

        public void Handle(IPublishedEvent<RosterInstancesAdded> evnt)
            => this.repository.AddRosters(evnt.EventSourceId, evnt.Payload.Instances.Select(x => x.GetIdentity()).ToArray());

        public void Handle(IPublishedEvent<RosterInstancesRemoved> evnt)
            => this.repository.RemoveRosters(evnt.EventSourceId, evnt.Payload.Instances.Select(x => x.GetIdentity()).ToArray());

        public void Handle(IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
            => this.repository.UpdateAnswer(evnt.EventSourceId, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), evnt.Payload.SelectedValues);

        public void Handle(IPublishedEvent<NumericRealQuestionAnswered> evnt)
            => this.repository.UpdateAnswer(evnt.EventSourceId, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), evnt.Payload.Answer);

        public void Handle(IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
            => this.repository.UpdateAnswer(evnt.EventSourceId, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), evnt.Payload.Answer);

        public void Handle(IPublishedEvent<TextQuestionAnswered> evnt) 
            => this.repository.UpdateAnswer(evnt.EventSourceId, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), evnt.Payload.Answer);

        public void Handle(IPublishedEvent<TextListQuestionAnswered> evnt)
            => this.repository.UpdateAnswer(evnt.EventSourceId, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), evnt.Payload.Answers);

        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> evnt)
            => this.repository.UpdateAnswer(evnt.EventSourceId, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), evnt.Payload.SelectedValue);

        public void Handle(IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt)
            => this.repository.UpdateAnswer(evnt.EventSourceId, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), evnt.Payload.SelectedRosterVector);

        public void Handle(IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt)
            => this.repository.UpdateAnswer(evnt.EventSourceId, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), evnt.Payload.SelectedRosterVectors);

        public void Handle(IPublishedEvent<DateTimeQuestionAnswered> evnt)
            => this.repository.UpdateAnswer(evnt.EventSourceId, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), evnt.Payload.Answer);

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> evnt)
            => this.repository.UpdateAnswer(evnt.EventSourceId, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector),
                new GeoPosition(evnt.Payload.Latitude, evnt.Payload.Longitude, evnt.Payload.Accuracy, evnt.Payload.Altitude, evnt.Payload.Timestamp));

        public void Handle(IPublishedEvent<QRBarcodeQuestionAnswered> evnt)
            => this.repository.UpdateAnswer(evnt.EventSourceId, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), evnt.Payload.Answer);

        public void Handle(IPublishedEvent<PictureQuestionAnswered> evnt)
            => this.repository.UpdateAnswer(evnt.EventSourceId, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), evnt.Payload.PictureFileName);

        public void Handle(IPublishedEvent<YesNoQuestionAnswered> evnt)
            => this.repository.UpdateAnswer(evnt.EventSourceId, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), evnt.Payload.AnsweredOptions);

        public void Handle(IPublishedEvent<AreaQuestionAnswered> evnt)
            => this.repository.UpdateAnswer(evnt.EventSourceId, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), new Area(evnt.Payload.Geometry,
                    evnt.Payload.MapName, evnt.Payload.AreaSize, evnt.Payload.Length, evnt.Payload.Coordinates, evnt.Payload.DistanceToEditor));

        public void Handle(IPublishedEvent<AudioQuestionAnswered> evnt)
            => this.repository.UpdateAnswer(evnt.EventSourceId, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector),
                AudioAnswer.FromString(evnt.Payload.FileName, evnt.Payload.Length));

        public void Handle(IPublishedEvent<AnswersRemoved> evnt)
            => this.repository.RemoveAnswers(evnt.EventSourceId, evnt.Payload.Questions);

        public void Handle(IPublishedEvent<GroupsDisabled> evnt)
            => this.repository.EnableEntities(evnt.EventSourceId, evnt.Payload.Groups, EntityType.Section, false);

        public void Handle(IPublishedEvent<GroupsEnabled> evnt)
            => this.repository.EnableEntities(evnt.EventSourceId, evnt.Payload.Groups, EntityType.Section, true);

        public void Handle(IPublishedEvent<StaticTextsEnabled> evnt)
            => this.repository.EnableEntities(evnt.EventSourceId, evnt.Payload.StaticTexts, EntityType.StaticText, true);

        public void Handle(IPublishedEvent<StaticTextsDisabled> evnt)
            => this.repository.EnableEntities(evnt.EventSourceId, evnt.Payload.StaticTexts, EntityType.StaticText, false);

        public void Handle(IPublishedEvent<StaticTextsDeclaredInvalid> evnt)
            => this.repository.MakeEntitiesInvalid(evnt.EventSourceId, evnt.Payload.GetFailedValidationConditionsDictionary(), EntityType.StaticText);

        public void Handle(IPublishedEvent<StaticTextsDeclaredValid> evnt)
            => this.repository.MakeEntitiesValid(evnt.EventSourceId, evnt.Payload.StaticTexts, EntityType.StaticText);

        public void Handle(IPublishedEvent<QuestionsDisabled> evnt)
            => this.repository.EnableEntities(evnt.EventSourceId, evnt.Payload.Questions, EntityType.Question, false);

        public void Handle(IPublishedEvent<QuestionsEnabled> evnt)
            => this.repository.EnableEntities(evnt.EventSourceId, evnt.Payload.Questions, EntityType.Question, true);

        public void Handle(IPublishedEvent<AnswersDeclaredInvalid> evnt)
            => this.repository.MakeEntitiesInvalid(evnt.EventSourceId, evnt.Payload.FailedValidationConditions, EntityType.Question);

        public void Handle(IPublishedEvent<AnswersDeclaredValid> evnt)
            => this.repository.MakeEntitiesValid(evnt.EventSourceId, evnt.Payload.Questions, EntityType.Question);

        public void Handle(IPublishedEvent<InterviewHardDeleted> evnt) 
            => this.repository.RemoveInterview(evnt.EventSourceId);

        public void Handle(IPublishedEvent<AnswerRemoved> evnt)
            => this.repository.RemoveAnswers(evnt.EventSourceId, new[] {Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector)});

        public void Handle(IPublishedEvent<QuestionsMarkedAsReadonly> evnt)
            => this.repository.MarkQuestionsAsReadOnly(evnt.EventSourceId, evnt.Payload.Questions);

        public void Handle(IPublishedEvent<VariablesChanged> evnt)
            => this.repository.UpdateVariables(evnt.EventSourceId, evnt.Payload.ChangedVariables);

        public void Handle(IPublishedEvent<VariablesDisabled> evnt)
            => this.repository.EnableEntities(evnt.EventSourceId, evnt.Payload.Variables, EntityType.Variable, false);

        public void Handle(IPublishedEvent<VariablesEnabled> evnt)
            => this.repository.EnableEntities(evnt.EventSourceId, evnt.Payload.Variables, EntityType.Variable, true);
    }
}

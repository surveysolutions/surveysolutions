using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    internal class QuestionsAndGroupsCollectionDenormalizer : AbstractFunctionalEventHandler<QuestionsAndGroupsCollectionView, IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView>>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, NewQuestionnaireCreated>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, QuestionnaireCloned>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, TemplateImported>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, NewQuestionAdded>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, QuestionChanged>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, QuestionCloned>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, QuestionDeleted>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, NumericQuestionChanged>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, NumericQuestionCloned>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, TextListQuestionChanged>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, TextListQuestionCloned>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, QRBarcodeQuestionUpdated>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, QRBarcodeQuestionCloned>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, MultimediaQuestionUpdated>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, StaticTextAdded>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, StaticTextUpdated>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, StaticTextCloned>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, StaticTextDeleted>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, QuestionnaireItemMoved>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, NewGroupAdded>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, GroupDeleted>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, GroupCloned>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, RosterChanged>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, GroupStoppedBeingARoster>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, GroupUpdated>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, QuestionnaireDeleted>
    {
        private readonly IQuestionDetailsViewMapper questionDetailsViewMapper;
        private readonly IQuestionnaireEntityFactory questionnaireEntityFactory;

        public QuestionsAndGroupsCollectionDenormalizer(
            IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView> readSideStorage,
            IQuestionDetailsViewMapper questionDetailsViewMapper, IQuestionnaireEntityFactory questionnaireEntityFactory)
            : base(readSideStorage)
        {
            this.questionDetailsViewMapper = questionDetailsViewMapper;
            this.questionnaireEntityFactory = questionnaireEntityFactory;
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state, IPublishedEvent<NewQuestionnaireCreated> @event)
        {
            return this.CreateStateWithAllQuestions(new QuestionnaireDocument());
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state, IPublishedEvent<QuestionnaireCloned> @event)
        {
            return this.CreateStateWithAllQuestions(@event.Payload.QuestionnaireDocument);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state, IPublishedEvent<TemplateImported> @event)
        {
            return this.CreateStateWithAllQuestions(@event.Payload.Source);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state, IPublishedEvent<NewQuestionAdded> @event)
        {
            IQuestion question = this.questionnaireEntityFactory.CreateQuestion(EventConverter.NewQuestionAddedToQuestionData(@event));
            return this.UpdateStateWithAddedQuestion(state, @event.Payload.GroupPublicKey.Value, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state, IPublishedEvent<QuestionCloned> @event)
        {
            IQuestion question = this.questionnaireEntityFactory.CreateQuestion(EventConverter.QuestionClonedToQuestionData(@event));
            return this.UpdateStateWithAddedQuestion(state, @event.Payload.GroupPublicKey.Value, question);

        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state, IPublishedEvent<QuestionChanged> @event)
        {
            IQuestion question = this.questionnaireEntityFactory.CreateQuestion(EventConverter.QuestionChangedToQuestionData(@event));
            return this.UpdateStateWithUpdatedQuestion(state, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state,
            IPublishedEvent<NumericQuestionChanged> @event)
        {
            IQuestion question = this.questionnaireEntityFactory.CreateQuestion(EventConverter.NumericQuestionChangedToQuestionData(@event));
            return this.UpdateStateWithUpdatedQuestion(state, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state,
            IPublishedEvent<NumericQuestionCloned> @event)
        {
            IQuestion question = this.questionnaireEntityFactory.CreateQuestion(EventConverter.NumericQuestionClonedToQuestionData(@event));
            return this.UpdateStateWithAddedQuestion(state, @event.Payload.GroupPublicKey, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state,
            IPublishedEvent<TextListQuestionCloned> @event)
        {
            IQuestion question = this.questionnaireEntityFactory.CreateQuestion(EventConverter.TextListQuestionClonedToQuestionData(@event));
            return this.UpdateStateWithAddedQuestion(state, @event.Payload.GroupId, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state,
            IPublishedEvent<TextListQuestionChanged> @event)
        {
            IQuestion question = this.questionnaireEntityFactory.CreateQuestion(EventConverter.TextListQuestionChangedToQuestionData(@event));
            return this.UpdateStateWithUpdatedQuestion(state, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state,
            IPublishedEvent<QRBarcodeQuestionUpdated> @event)
        {
            IQuestion question = this.questionnaireEntityFactory.CreateQuestion(EventConverter.QRBarcodeQuestionUpdatedToQuestionData(@event));
            return this.UpdateStateWithUpdatedQuestion(state, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state,
            IPublishedEvent<QRBarcodeQuestionCloned> @event)
        {
            IQuestion question = this.questionnaireEntityFactory.CreateQuestion(EventConverter.QRBarcodeQuestionClonedToQuestionData(@event));
            return this.UpdateStateWithAddedQuestion(state, @event.Payload.ParentGroupId, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state, IPublishedEvent<MultimediaQuestionUpdated> @event)
        {
            IQuestion question = this.questionnaireEntityFactory.CreateQuestion(EventConverter.MultimediaQuestionUpdatedToQuestionData(@event));
            return this.UpdateStateWithUpdatedQuestion(state, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state,
            IPublishedEvent<StaticTextAdded> @event)
        {
            IStaticText staticText = this.questionnaireEntityFactory.CreateStaticText(entityId: @event.Payload.EntityId,
                text: @event.Payload.Text, attachmentName: null);
            return this.UpdateStateWithAddedStaticText(state, @event.Payload.ParentId, staticText);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state,
            IPublishedEvent<StaticTextUpdated> @event)
        {
            IStaticText staticText = this.questionnaireEntityFactory.CreateStaticText(entityId: @event.Payload.EntityId,
                text: @event.Payload.Text, attachmentName: @event.Payload.AttachmentName);
            return this.UpdateStateWithUpdatedStaticText(state, staticText);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state,
            IPublishedEvent<StaticTextCloned> @event)
        {
            IStaticText staticText = this.questionnaireEntityFactory.CreateStaticText(entityId: @event.Payload.EntityId,
                text: @event.Payload.Text, attachmentName: @event.Payload.AttachmentName);
            return this.UpdateStateWithAddedStaticText(state, @event.Payload.ParentId, staticText);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state,
            IPublishedEvent<StaticTextDeleted> @event)
        {
            var staticText = state.StaticTexts.FirstOrDefault(x => x.Id == @event.Payload.EntityId);
            if (staticText != null)
            {
                state.StaticTexts.Remove(staticText);   
            }
            return state;
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state, IPublishedEvent<QuestionDeleted> @event)
        {
            if (state == null)
            {
                return null;
            }

            var oldQuestion = state.Questions.FirstOrDefault(x => x.Id == @event.Payload.QuestionId);
            state.Questions.Remove(oldQuestion);
            return state;
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state,
            IPublishedEvent<QuestionnaireItemMoved> @event)
        {
            if (state == null)
            {
                return null;
            }

            var question = state.Questions.FirstOrDefault(x => x.Id == @event.Payload.PublicKey);
            if (question != null)
            {
                question.ParentGroupId = @event.Payload.GroupKey.Value;
                UpdateBreadcrumbs(state, question, question.ParentGroupId);
                return state;
            }
            var staticText = state.StaticTexts.FirstOrDefault(x => x.Id == @event.Payload.PublicKey);
            if (staticText != null)
            {
                staticText.ParentGroupId = @event.Payload.GroupKey.Value;
                UpdateBreadcrumbs(state, staticText, staticText.ParentGroupId);
                return state;
            }

            var group = state.Groups.FirstOrDefault(x => x.Id == @event.Payload.PublicKey);
            if (group != null)
            {
                group.ParentGroupId = @event.Payload.GroupKey ?? @event.EventSourceId;
                UpdateBreadcrumbs(state, group, group.Id);

                var descendantGroups = this.GetAllDescendantGroups(state, group.Id);
                descendantGroups.ForEach(x => UpdateBreadcrumbs(state, x, x.Id));

                var descendantQuestion = this.GetAllDescendantQuestions(state, group.Id);
                descendantQuestion.ForEach(x => UpdateBreadcrumbs(state, x, x.ParentGroupId));

                var descendantStaticTexts = this.GetAllDescendantStaticTexts(state, group.Id);
                descendantStaticTexts.ForEach(x => UpdateBreadcrumbs(state, x, x.ParentGroupId));
            }
            return state;
        }

        //public void Delete(QuestionDetailsCollectionView state, IPublishedEvent<QuestionnaireDeleted> event) { }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state, IPublishedEvent<NewGroupAdded> @event)
        {
            return UpdateStateWithAddedGroup(state, @event.Payload.PublicKey, @event.Payload.GroupText, @event.Payload.VariableName, @event.Payload.Description,
                @event.Payload.ConditionExpression, @event.Payload.HideIfDisabled, @event.Payload.ParentGroupPublicKey ?? Guid.Empty);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state, IPublishedEvent<GroupCloned> @event)
        {
            return UpdateStateWithAddedGroup(state, @event.Payload.PublicKey, @event.Payload.GroupText, @event.Payload.VariableName, @event.Payload.Description,
                @event.Payload.ConditionExpression, @event.Payload.HideIfDisabled, @event.Payload.ParentGroupPublicKey ?? Guid.Empty);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state, IPublishedEvent<GroupDeleted> @event)
        {
            if (state == null)
            {
                return null;
            }

            var shouldBeDeletedGroups = GetAllDescendantGroups(state, @event.Payload.GroupPublicKey);
            shouldBeDeletedGroups.ForEach(x => state.Groups.Remove(x));

            var shouldBeDeletedQuestions = GetAllDescendantQuestions(state, @event.Payload.GroupPublicKey);
            shouldBeDeletedQuestions.ForEach(x => state.Questions.Remove(x));

            var shouldBeDeletedStaticTexts = GetAllDescendantStaticTexts(state, @event.Payload.GroupPublicKey);
            shouldBeDeletedStaticTexts.ForEach(x => state.StaticTexts.Remove(x));


            return state;
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state, IPublishedEvent<RosterChanged> @event)
        {
            if (state == null)
            {
                return null;
            }

            var group = state.Groups.FirstOrDefault(x => x.Id == @event.Payload.GroupId);
            if (group == null)
                return state;

            group.IsRoster = true;
            group.RosterSizeQuestionId = @event.Payload.RosterSizeQuestionId;
            group.RosterSizeSourceType = @event.Payload.RosterSizeSource;
            group.FixedRosterTitles = @event.Payload.FixedRosterTitles;
            group.RosterTitleQuestionId = @event.Payload.RosterTitleQuestionId;

            var groups = this.GetAllDescendantGroups(state, @event.Payload.GroupId);
            var questions = this.GetAllDescendantQuestions(state, @event.Payload.GroupId);
            var staticTexts = this.GetAllDescendantStaticTexts(state, @event.Payload.GroupId);
            groups.ForEach(x => UpdateBreadcrumbs(state, x, x.Id));
            questions.ForEach(x => UpdateBreadcrumbs(state, x, x.ParentGroupId));
            staticTexts.ForEach(x => UpdateBreadcrumbs(state, x, x.ParentGroupId));

            return state;
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state,
            IPublishedEvent<GroupStoppedBeingARoster> @event)
        {
            if (state == null)
            {
                return null;
            }

            var group = state.Groups.FirstOrDefault(x => x.Id == @event.Payload.GroupId);
            if (group == null)
                return state;

            group.IsRoster = false;
            group.RosterSizeQuestionId = null;
            group.FixedRosterTitles = new FixedRosterTitle[0];
            group.RosterTitleQuestionId = null;

            var groups = this.GetAllDescendantGroups(state, @event.Payload.GroupId);
            var questions = this.GetAllDescendantQuestions(state, @event.Payload.GroupId);
            var staticTexts = this.GetAllDescendantStaticTexts(state, @event.Payload.GroupId);
            groups.ForEach(x => UpdateBreadcrumbs(state, x, x.Id));
            questions.ForEach(x => UpdateBreadcrumbs(state, x, x.ParentGroupId));
            staticTexts.ForEach(x => UpdateBreadcrumbs(state, x, x.ParentGroupId));

            return state;
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state, IPublishedEvent<GroupUpdated> @event)
        {
            if (state == null)
            {
                return null;
            }

            var group = state.Groups.FirstOrDefault(x => x.Id == @event.Payload.GroupPublicKey);
            if (group == null)
                return state;

            group.Title = @event.Payload.GroupText;
            group.EnablementCondition = @event.Payload.ConditionExpression;
            group.HideIfDisabled = @event.Payload.HideIfDisabled;
            group.VariableName = @event.Payload.VariableName;

            return state;
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView state, IPublishedEvent<QuestionnaireDeleted> @event)
        {
            return null;
        }

        private List<StaticTextDetailsView> GetAllDescendantStaticTexts(QuestionsAndGroupsCollectionView currentState, Guid groupId)
        {
            var descendantStaticTexts = new List<StaticTextDetailsView>();

            currentState
                .StaticTexts
                .Where(x => x.ParentGroupsIds.Contains(groupId))
                .ForEach(descendantStaticTexts.Add);

            return descendantStaticTexts;
        }

        private List<QuestionDetailsView> GetAllDescendantQuestions(QuestionsAndGroupsCollectionView currentState, Guid groupId)
        {
            var descendantQuestions = new List<QuestionDetailsView>();

            currentState
                .Questions
                .Where(x => x.ParentGroupsIds.Contains(groupId))
                .ForEach(descendantQuestions.Add);

            return descendantQuestions;
        }

        private List<GroupAndRosterDetailsView> GetAllDescendantGroups(QuestionsAndGroupsCollectionView currentState, Guid groupId)
        {
            var descendantGroups = new List<GroupAndRosterDetailsView>();

            currentState
                .Groups
                .Where(x => x.ParentGroupsIds.Contains(groupId) || x.Id == groupId)
                .ForEach(descendantGroups.Add);

            return descendantGroups;
        }


        private static void UpdateBreadcrumbs(QuestionsAndGroupsCollectionView currentState, DescendantItemView item, Guid startGroupIdGuid)
        {
            var parentsStack = new Stack<Guid>();
            var breadcrumbs = new List<Guid>();
            var rosterScopes = new List<Guid>();
            parentsStack.Push(startGroupIdGuid);
            while (parentsStack.Any())
            {
                var ancestorId = parentsStack.Pop();
                var group = currentState.Groups.FirstOrDefault(x => x.Id == ancestorId);
                if (group == null)
                {
                    continue;
                }
                breadcrumbs.Add(group.Id);
                if (group.IsRoster)
                {
                    rosterScopes.Add(@group.RosterSizeSourceType == RosterSizeSourceType.FixedTitles
                        ? @group.Id
                        : @group.RosterSizeQuestionId.Value);
                }
                if (breadcrumbs.Contains(group.ParentGroupId))
                {
                    break;
                }
                parentsStack.Push(group.ParentGroupId);
            }
            breadcrumbs.Remove(item.Id);
            item.ParentGroupsIds = breadcrumbs.ToArray();
            item.RosterScopeIds = rosterScopes.ToArray();
        }

        private QuestionsAndGroupsCollectionView CreateStateWithAllQuestions(QuestionnaireDocument questionnaire)
        {
            questionnaire.ConnectChildrenWithParent();
            var questions = questionnaire.GetEntitiesByType<IQuestion>()
                .Select(question => this.questionDetailsViewMapper.Map(question, question.GetParent().PublicKey))
                .Where(q => q != null)
                .ToList();

            var groups = questionnaire.GetAllGroups()
                .Select(g => new GroupAndRosterDetailsView
                {
                    Id = g.PublicKey,
                    Title = g.Title,
                    IsRoster = g.IsRoster,
                    FixedRosterTitles = g.FixedRosterTitles,
                    RosterSizeQuestionId = g.RosterSizeQuestionId,
                    RosterSizeSourceType = g.RosterSizeSource,
                    RosterTitleQuestionId = g.RosterTitleQuestionId,
                    ParentGroupId = g.GetParent().PublicKey == questionnaire.PublicKey ? Guid.Empty : g.GetParent().PublicKey,
                    EnablementCondition = g.ConditionExpression,
                    HideIfDisabled = g.HideIfDisabled,
                    VariableName = g.VariableName
                })
                .ToList();

            var staticTexts =
                questionnaire.GetEntitiesByType<IStaticText>().Select(staticText => new StaticTextDetailsView()
                {
                    Id = staticText.PublicKey,
                    ParentGroupId = staticText.GetParent().PublicKey,
                    Text = staticText.Text,
                    AttachmentName = staticText.AttachmentName
                }).ToList();

            var questionCollection = new QuestionsAndGroupsCollectionView
            {
                Questions = questions,
                Groups = groups,
                StaticTexts = staticTexts
            };

            groups.ForEach(x => UpdateBreadcrumbs(questionCollection, x, x.Id));
            questions.ForEach(x => UpdateBreadcrumbs(questionCollection, x, x.ParentGroupId));
            staticTexts.ForEach(x => UpdateBreadcrumbs(questionCollection, x, x.ParentGroupId));

            return questionCollection;
        }

        private static QuestionsAndGroupsCollectionView UpdateStateWithAddedGroup(QuestionsAndGroupsCollectionView currentState,
            Guid groupId, string title,string variableName, string description, string enablementCondition, bool hideIfDisabled, Guid parentGroupId)
        {
            if (currentState == null)
            {
                return null;
            }

            var group = new GroupAndRosterDetailsView
            {
                Id = groupId,
                Title = title,
                ParentGroupId = parentGroupId,
                IsRoster = false,
                EnablementCondition = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ParentGroupsIds = new Guid[0],
                RosterScopeIds = new Guid[0],
                VariableName = variableName
            };

            currentState.Groups.Add(group);
            UpdateBreadcrumbs(currentState, group, @group.Id);
            return currentState;
        }

        private QuestionsAndGroupsCollectionView UpdateStateWithAddedQuestion(QuestionsAndGroupsCollectionView currentState,
            Guid parentGroupId, IQuestion question)
        {
            if (currentState == null)
            {
                return null;
            }

            QuestionDetailsView questionDetailsView = this.questionDetailsViewMapper.Map(question, parentGroupId);
            currentState.Questions.Add(questionDetailsView);
            UpdateBreadcrumbs(currentState, questionDetailsView, questionDetailsView.ParentGroupId);
            return currentState;
        }

        private QuestionsAndGroupsCollectionView UpdateStateWithUpdatedQuestion(QuestionsAndGroupsCollectionView currentState,
            IQuestion question)
        {
            if (currentState == null)
            {
                return null;
            }

            var oldQuestion = currentState.Questions.FirstOrDefault(x => x.Id == question.PublicKey);
            if (oldQuestion == null)
            {
                return currentState;
            }

            currentState.Questions.Remove(oldQuestion);
            var questionDetailsView = this.questionDetailsViewMapper.Map(question, oldQuestion.ParentGroupId);
            UpdateBreadcrumbs(currentState, questionDetailsView, questionDetailsView.ParentGroupId);
            currentState.Questions.Add(questionDetailsView);
            return currentState;
        }

        private QuestionsAndGroupsCollectionView UpdateStateWithAddedStaticText(
             QuestionsAndGroupsCollectionView currentState, Guid parentId, IStaticText staticText)
        {
            if (currentState == null)
            {
                return null;
            }

            var staticTextDetailsView = new StaticTextDetailsView()
            {
                Id = staticText.PublicKey,
                ParentGroupId = parentId,
                Text = staticText.Text,
                AttachmentName = staticText.AttachmentName
            };

            currentState.StaticTexts.Add(staticTextDetailsView);
            UpdateBreadcrumbs(currentState, staticTextDetailsView, staticTextDetailsView.ParentGroupId);
            return currentState;
        }

        private QuestionsAndGroupsCollectionView UpdateStateWithUpdatedStaticText(
            QuestionsAndGroupsCollectionView currentState, IStaticText staticText)
        {
            if (currentState == null)
            {
                return null;
            }

            var oldstaticTextDetailsView = currentState.StaticTexts.FirstOrDefault(x => x.Id == staticText.PublicKey);
            if (oldstaticTextDetailsView == null)
            {
                return currentState;
            }

            currentState.StaticTexts.Remove(oldstaticTextDetailsView);

            var staticTextDetailsView = new StaticTextDetailsView()
            {
                Id = staticText.PublicKey,
                ParentGroupId = oldstaticTextDetailsView.ParentGroupId,
                Text = staticText.Text,
                AttachmentName = staticText.AttachmentName
            };
            UpdateBreadcrumbs(currentState, staticTextDetailsView, staticTextDetailsView.ParentGroupId);
            currentState.StaticTexts.Add(staticTextDetailsView);
            return currentState;
        }
    }
}

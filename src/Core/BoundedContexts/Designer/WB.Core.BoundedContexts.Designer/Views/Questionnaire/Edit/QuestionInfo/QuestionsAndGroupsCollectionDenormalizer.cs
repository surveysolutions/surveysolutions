using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using Raven.Abstractions.Extensions;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

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
        , IUpdateHandler<QuestionsAndGroupsCollectionView, NumericQuestionAdded>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, NumericQuestionChanged>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, NumericQuestionCloned>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, TextListQuestionAdded>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, TextListQuestionChanged>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, TextListQuestionCloned>
        , IUpdateHandler<QuestionsAndGroupsCollectionView, QRBarcodeQuestionAdded>
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

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState, IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
            return this.CreateStateWithAllQuestions(new QuestionnaireDocument());
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState, IPublishedEvent<QuestionnaireCloned> evnt)
        {
            return this.CreateStateWithAllQuestions(evnt.Payload.QuestionnaireDocument);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState, IPublishedEvent<TemplateImported> evnt)
        {
            return this.CreateStateWithAllQuestions(evnt.Payload.Source);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState, IPublishedEvent<NewQuestionAdded> evnt)
        {
            IQuestion question = this.questionnaireEntityFactory.CreateQuestion(EventConverter.NewQuestionAddedToQuestionData(evnt));
            return this.UpdateStateWithAddedQuestion(currentState, evnt.Payload.GroupPublicKey.Value, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState, IPublishedEvent<QuestionCloned> evnt)
        {
            IQuestion question = this.questionnaireEntityFactory.CreateQuestion(EventConverter.QuestionClonedToQuestionData(evnt));
            return this.UpdateStateWithAddedQuestion(currentState, evnt.Payload.GroupPublicKey.Value, question);

        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState, IPublishedEvent<QuestionChanged> evnt)
        {
            IQuestion question = this.questionnaireEntityFactory.CreateQuestion(EventConverter.QuestionChangedToQuestionData(evnt));
            return this.UpdateStateWithUpdatedQuestion(currentState, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<NumericQuestionAdded> evnt)
        {
            IQuestion question = this.questionnaireEntityFactory.CreateQuestion(EventConverter.NumericQuestionAddedToQuestionData(evnt));
            return this.UpdateStateWithAddedQuestion(currentState, evnt.Payload.GroupPublicKey, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<NumericQuestionChanged> evnt)
        {
            IQuestion question = this.questionnaireEntityFactory.CreateQuestion(EventConverter.NumericQuestionChangedToQuestionData(evnt));
            return this.UpdateStateWithUpdatedQuestion(currentState, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<NumericQuestionCloned> evnt)
        {
            IQuestion question = this.questionnaireEntityFactory.CreateQuestion(EventConverter.NumericQuestionClonedToQuestionData(evnt));
            return this.UpdateStateWithAddedQuestion(currentState, evnt.Payload.GroupPublicKey, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<TextListQuestionAdded> evnt)
        {
            IQuestion question = this.questionnaireEntityFactory.CreateQuestion(EventConverter.TextListQuestionAddedToQuestionData(evnt));
            return this.UpdateStateWithAddedQuestion(currentState, evnt.Payload.GroupId, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<TextListQuestionCloned> evnt)
        {
            IQuestion question = this.questionnaireEntityFactory.CreateQuestion(EventConverter.TextListQuestionClonedToQuestionData(evnt));
            return this.UpdateStateWithAddedQuestion(currentState, evnt.Payload.GroupId, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<TextListQuestionChanged> evnt)
        {
            IQuestion question = this.questionnaireEntityFactory.CreateQuestion(EventConverter.TextListQuestionChangedToQuestionData(evnt));
            return this.UpdateStateWithUpdatedQuestion(currentState, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<QRBarcodeQuestionAdded> evnt)
        {
            IQuestion question = this.questionnaireEntityFactory.CreateQuestion(EventConverter.QRBarcodeQuestionAddedToQuestionData(evnt));
            return this.UpdateStateWithAddedQuestion(currentState, evnt.Payload.ParentGroupId, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<QRBarcodeQuestionUpdated> evnt)
        {
            IQuestion question = this.questionnaireEntityFactory.CreateQuestion(EventConverter.QRBarcodeQuestionUpdatedToQuestionData(evnt));
            return this.UpdateStateWithUpdatedQuestion(currentState, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<QRBarcodeQuestionCloned> evnt)
        {
            IQuestion question = this.questionnaireEntityFactory.CreateQuestion(EventConverter.QRBarcodeQuestionClonedToQuestionData(evnt));
            return this.UpdateStateWithAddedQuestion(currentState, evnt.Payload.ParentGroupId, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState, IPublishedEvent<MultimediaQuestionUpdated> evnt)
        {
            IQuestion question = this.questionnaireEntityFactory.CreateQuestion(EventConverter.MultimediaQuestionUpdatedToQuestionData(evnt));
            return this.UpdateStateWithUpdatedQuestion(currentState, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<StaticTextAdded> evnt)
        {
            IStaticText staticText = this.questionnaireEntityFactory.CreateStaticText(entityId: evnt.Payload.EntityId,
                text: evnt.Payload.Text);
            return this.UpdateStateWithAddedStaticText(currentState, evnt.Payload.ParentId, staticText);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<StaticTextUpdated> evnt)
        {
            IStaticText staticText = this.questionnaireEntityFactory.CreateStaticText(entityId: evnt.Payload.EntityId,
                text: evnt.Payload.Text);
            return this.UpdateStateWithUpdatedStaticText(currentState, staticText);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<StaticTextCloned> evnt)
        {
            IStaticText staticText = this.questionnaireEntityFactory.CreateStaticText(entityId: evnt.Payload.EntityId,
                text: evnt.Payload.Text);
            return this.UpdateStateWithAddedStaticText(currentState, evnt.Payload.ParentId, staticText);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<StaticTextDeleted> evnt)
        {
            var staticText = currentState.StaticTexts.FirstOrDefault(x => x.Id == evnt.Payload.EntityId);
            if (staticText != null)
            {
                currentState.StaticTexts.Remove(staticText);   
            }
            return currentState;
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState, IPublishedEvent<QuestionDeleted> evnt)
        {
            if (currentState == null)
            {
                return null;
            }

            var oldQuestion = currentState.Questions.FirstOrDefault(x => x.Id == evnt.Payload.QuestionId);
            currentState.Questions.Remove(oldQuestion);
            return currentState;
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<QuestionnaireItemMoved> evnt)
        {
            if (currentState == null)
            {
                return null;
            }

            var question = currentState.Questions.FirstOrDefault(x => x.Id == evnt.Payload.PublicKey);
            if (question != null)
            {
                question.ParentGroupId = evnt.Payload.GroupKey.Value;
                UpdateBreadcrumbs(currentState, question, question.ParentGroupId);
                return currentState;
            }
            var staticText = currentState.StaticTexts.FirstOrDefault(x => x.Id == evnt.Payload.PublicKey);
            if (staticText != null)
            {
                staticText.ParentGroupId = evnt.Payload.GroupKey.Value;
                UpdateBreadcrumbs(currentState, staticText, staticText.ParentGroupId);
                return currentState;
            }

            var group = currentState.Groups.FirstOrDefault(x => x.Id == evnt.Payload.PublicKey);
            if (group != null)
            {
                group.ParentGroupId = evnt.Payload.GroupKey ?? evnt.EventSourceId;
                UpdateBreadcrumbs(currentState, group, group.Id);

                var descendantGroups = this.GetAllDescendantGroups(currentState, group.Id);
                descendantGroups.ForEach(x => UpdateBreadcrumbs(currentState, x, x.Id));

                var descendantQuestion = this.GetAllDescendantQuestions(currentState, group.Id);
                descendantQuestion.ForEach(x => UpdateBreadcrumbs(currentState, x, x.ParentGroupId));

                var descendantStaticTexts = this.GetAllDescendantStaticTexts(currentState, group.Id);
                descendantStaticTexts.ForEach(x => UpdateBreadcrumbs(currentState, x, x.ParentGroupId));
            }
            return currentState;
        }

        //public void Delete(QuestionDetailsCollectionView currentState, IPublishedEvent<QuestionnaireDeleted> evnt) { }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState, IPublishedEvent<NewGroupAdded> evnt)
        {
            return UpdateStateWithAddedGroup(currentState, evnt.Payload.PublicKey, evnt.Payload.GroupText, evnt.Payload.VariableName, evnt.Payload.Description,
                evnt.Payload.ConditionExpression, evnt.Payload.ParentGroupPublicKey ?? Guid.Empty);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState, IPublishedEvent<GroupCloned> evnt)
        {
            return UpdateStateWithAddedGroup(currentState, evnt.Payload.PublicKey, evnt.Payload.GroupText, evnt.Payload.VariableName, evnt.Payload.Description,
                evnt.Payload.ConditionExpression, evnt.Payload.ParentGroupPublicKey ?? Guid.Empty);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState, IPublishedEvent<GroupDeleted> evnt)
        {
            if (currentState == null)
            {
                return null;
            }

            var shouldBeDeletedGroups = GetAllDescendantGroups(currentState, evnt.Payload.GroupPublicKey);
            shouldBeDeletedGroups.ForEach(x => currentState.Groups.Remove(x));

            var shouldBeDeletedQuestions = GetAllDescendantQuestions(currentState, evnt.Payload.GroupPublicKey);
            shouldBeDeletedQuestions.ForEach(x => currentState.Questions.Remove(x));

            var shouldBeDeletedStaticTexts = GetAllDescendantStaticTexts(currentState, evnt.Payload.GroupPublicKey);
            shouldBeDeletedStaticTexts.ForEach(x => currentState.StaticTexts.Remove(x));


            return currentState;
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState, IPublishedEvent<RosterChanged> evnt)
        {
            if (currentState == null)
            {
                return null;
            }

            var group = currentState.Groups.FirstOrDefault(x => x.Id == evnt.Payload.GroupId);
            if (group == null)
                return currentState;

            group.IsRoster = true;
            group.RosterSizeQuestionId = evnt.Payload.RosterSizeQuestionId;
            group.RosterSizeSourceType = evnt.Payload.RosterSizeSource;
            group.FixedRosterTitles = evnt.Payload.FixedRosterTitles;
            group.RosterTitleQuestionId = evnt.Payload.RosterTitleQuestionId;

            var groups = this.GetAllDescendantGroups(currentState, evnt.Payload.GroupId);
            var questions = this.GetAllDescendantQuestions(currentState, evnt.Payload.GroupId);
            var staticTexts = this.GetAllDescendantStaticTexts(currentState, evnt.Payload.GroupId);
            groups.ForEach(x => UpdateBreadcrumbs(currentState, x, x.Id));
            questions.ForEach(x => UpdateBreadcrumbs(currentState, x, x.ParentGroupId));
            staticTexts.ForEach(x => UpdateBreadcrumbs(currentState, x, x.ParentGroupId));

            return currentState;
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<GroupStoppedBeingARoster> evnt)
        {
            if (currentState == null)
            {
                return null;
            }

            var group = currentState.Groups.FirstOrDefault(x => x.Id == evnt.Payload.GroupId);
            if (group == null)
                return currentState;

            group.IsRoster = false;
            group.RosterSizeQuestionId = null;
            group.FixedRosterTitles = null;
            group.RosterTitleQuestionId = null;

            var groups = this.GetAllDescendantGroups(currentState, evnt.Payload.GroupId);
            var questions = this.GetAllDescendantQuestions(currentState, evnt.Payload.GroupId);
            var staticTexts = this.GetAllDescendantStaticTexts(currentState, evnt.Payload.GroupId);
            groups.ForEach(x => UpdateBreadcrumbs(currentState, x, x.Id));
            questions.ForEach(x => UpdateBreadcrumbs(currentState, x, x.ParentGroupId));
            staticTexts.ForEach(x => UpdateBreadcrumbs(currentState, x, x.ParentGroupId));

            return currentState;
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState, IPublishedEvent<GroupUpdated> evnt)
        {
            if (currentState == null)
            {
                return null;
            }

            var group = currentState.Groups.FirstOrDefault(x => x.Id == evnt.Payload.GroupPublicKey);
            if (group == null)
                return currentState;

            group.Title = evnt.Payload.GroupText;
            group.EnablementCondition = evnt.Payload.ConditionExpression;
            group.VariableName = evnt.Payload.VariableName;

            return currentState;
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState, IPublishedEvent<QuestionnaireDeleted> evnt)
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
                    VariableName = g.VariableName
                })
                .ToList();

            var staticTexts =
                questionnaire.GetEntitiesByType<IStaticText>().Select(staticText => new StaticTextDetailsView()
                {
                    Id = staticText.PublicKey,
                    ParentGroupId = staticText.GetParent().PublicKey,
                    Text = staticText.Text
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
            Guid groupId, string title,string variableName, string description, string enablementCondition, Guid parentGroupId)
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
                Text = staticText.Text
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
                Text = staticText.Text
            };
            UpdateBreadcrumbs(currentState, staticTextDetailsView, staticTextDetailsView.ParentGroupId);
            currentState.StaticTexts.Add(staticTextDetailsView);
            return currentState;
        }
    }
}

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
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    internal class QuestionsAndGroupsCollectionDenormalizer : AbstractFunctionalEventHandler<QuestionsAndGroupsCollectionView>
        , ICreateHandler<QuestionsAndGroupsCollectionView, NewQuestionnaireCreated>
        , ICreateHandler<QuestionsAndGroupsCollectionView, QuestionnaireCloned>
        , ICreateHandler<QuestionsAndGroupsCollectionView, TemplateImported>
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
        private readonly IQuestionFactory questionFactory;

        public QuestionsAndGroupsCollectionDenormalizer(
            IReadSideRepositoryWriter<QuestionsAndGroupsCollectionView> readsideRepositoryWriter,
            IQuestionDetailsViewMapper questionDetailsViewMapper, IQuestionFactory questionFactory)
            : base(readsideRepositoryWriter)
        {
            this.questionDetailsViewMapper = questionDetailsViewMapper;
            this.questionFactory = questionFactory;
        }

        public override Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public override Type[] BuildsViews
        {
            get { return base.BuildsViews.Union(new[] { typeof (QuestionsAndGroupsCollectionView) }).ToArray(); }
        }

        public QuestionsAndGroupsCollectionView Create(IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
            return this.CreateStateWithAllQuestions(new QuestionnaireDocument());
        }

        public QuestionsAndGroupsCollectionView Create(IPublishedEvent<QuestionnaireCloned> evnt)
        {
            return this.CreateStateWithAllQuestions(evnt.Payload.QuestionnaireDocument);
        }

        public QuestionsAndGroupsCollectionView Create(IPublishedEvent<TemplateImported> evnt)
        {
            return this.CreateStateWithAllQuestions(evnt.Payload.Source);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState, IPublishedEvent<NewQuestionAdded> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.NewQuestionAddedToQuestionData(evnt));
            return this.UpdateStateWithAddedQuestion(currentState, evnt.Payload.GroupPublicKey.Value, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState, IPublishedEvent<QuestionCloned> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.QuestionClonedToQuestionData(evnt));
            return this.UpdateStateWithAddedQuestion(currentState, evnt.Payload.GroupPublicKey.Value, question);

        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState, IPublishedEvent<QuestionChanged> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.QuestionChangedToQuestionData(evnt));
            return this.UpdateStateWithUpdatedQuestion(currentState, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<NumericQuestionAdded> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.NumericQuestionAddedToQuestionData(evnt));
            return this.UpdateStateWithAddedQuestion(currentState, evnt.Payload.GroupPublicKey, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<NumericQuestionChanged> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.NumericQuestionChangedToQuestionData(evnt));
            return this.UpdateStateWithUpdatedQuestion(currentState, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<NumericQuestionCloned> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.NumericQuestionClonedToQuestionData(evnt));
            return this.UpdateStateWithAddedQuestion(currentState, evnt.Payload.GroupPublicKey, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<TextListQuestionAdded> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.TextListQuestionAddedToQuestionData(evnt));
            return this.UpdateStateWithAddedQuestion(currentState, evnt.Payload.GroupId, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<TextListQuestionCloned> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.TextListQuestionClonedToQuestionData(evnt));
            return this.UpdateStateWithAddedQuestion(currentState, evnt.Payload.GroupId, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<TextListQuestionChanged> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.TextListQuestionChangedToQuestionData(evnt));
            return this.UpdateStateWithUpdatedQuestion(currentState, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<QRBarcodeQuestionAdded> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.QRBarcodeQuestionAddedToQuestionData(evnt));
            return this.UpdateStateWithAddedQuestion(currentState, evnt.Payload.ParentGroupId, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<QRBarcodeQuestionUpdated> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.QRBarcodeQuestionUpdatedToQuestionData(evnt));
            return this.UpdateStateWithUpdatedQuestion(currentState, question);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState,
            IPublishedEvent<QRBarcodeQuestionCloned> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.QRBarcodeQuestionClonedToQuestionData(evnt));
            return this.UpdateStateWithAddedQuestion(currentState, evnt.Payload.ParentGroupId, question);
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

            if (evnt.Payload.GroupKey == null)
            {
                return currentState;
            }

            var question = currentState.Questions.FirstOrDefault(x => x.Id == evnt.Payload.PublicKey);
            if (question != null)
            {
                question.ParentGroupId = evnt.Payload.GroupKey.Value;
                UpdateBreadcrumbs(currentState, question, question.ParentGroupId);
                return currentState;
            }

            var group = currentState.Groups.FirstOrDefault(x => x.Id == evnt.Payload.PublicKey);
            if (group != null)
            {
                group.ParentGroupId = evnt.Payload.GroupKey ?? Guid.Empty;
                UpdateBreadcrumbs(currentState, group, group.Id);

                var descendantGroups = this.GetAllDescendantGroups(currentState, group.Id);
                descendantGroups.ForEach(x => UpdateBreadcrumbs(currentState, x, x.Id));

                var descendantQuestion = this.GetAllDescendantQuestions(currentState, group.Id);
                descendantQuestion.ForEach(x => UpdateBreadcrumbs(currentState, x, x.ParentGroupId));
            }
            return currentState;
        }

        //public void Delete(QuestionDetailsCollectionView currentState, IPublishedEvent<QuestionnaireDeleted> evnt) { }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState, IPublishedEvent<NewGroupAdded> evnt)
        {
            return UpdateStateWithAddedGroup(currentState, evnt.Payload.PublicKey, evnt.Payload.GroupText, evnt.Payload.Description,
                evnt.Payload.ConditionExpression, evnt.Payload.ParentGroupPublicKey ?? Guid.Empty);
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState, IPublishedEvent<GroupCloned> evnt)
        {
            return UpdateStateWithAddedGroup(currentState, evnt.Payload.PublicKey, evnt.Payload.GroupText, evnt.Payload.Description,
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
            group.RosterFixedTitles = evnt.Payload.RosterFixedTitles;
            group.RosterTitleQuestionId = evnt.Payload.RosterTitleQuestionId;

            var groups = this.GetAllDescendantGroups(currentState, evnt.Payload.GroupId);
            var questions = this.GetAllDescendantQuestions(currentState, evnt.Payload.GroupId);
            groups.ForEach(x => UpdateBreadcrumbs(currentState, x, x.Id));
            questions.ForEach(x => UpdateBreadcrumbs(currentState, x, x.ParentGroupId));

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
            group.RosterFixedTitles = null;
            group.RosterTitleQuestionId = null;

            var groups = this.GetAllDescendantGroups(currentState, evnt.Payload.GroupId);
            var questions = this.GetAllDescendantQuestions(currentState, evnt.Payload.GroupId);
            groups.ForEach(x => UpdateBreadcrumbs(currentState, x, x.Id));
            questions.ForEach(x => UpdateBreadcrumbs(currentState, x, x.ParentGroupId));

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
            group.Description = evnt.Payload.Description;
            group.EnablementCondition = evnt.Payload.ConditionExpression;

            return currentState;
        }

        public QuestionsAndGroupsCollectionView Update(QuestionsAndGroupsCollectionView currentState, IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            return null;
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
            var questions = questionnaire.GetAllQuestions<IQuestion>()
                .Select(question => this.questionDetailsViewMapper.Map(question, question.GetParent().PublicKey))
                .Where(q => q != null)
                .ToList();

            var groups = questionnaire.GetAllGroups()
                .Select(g => new GroupAndRosterDetailsView
                {
                    Id = g.PublicKey,
                    Title = g.Title,
                    Description = g.Description,
                    IsRoster = g.IsRoster,
                    RosterFixedTitles = g.RosterFixedTitles,
                    RosterSizeQuestionId = g.RosterSizeQuestionId,
                    RosterSizeSourceType = g.RosterSizeSource,
                    RosterTitleQuestionId = g.RosterTitleQuestionId,
                    ParentGroupId = g.GetParent().PublicKey == questionnaire.PublicKey ? Guid.Empty : g.GetParent().PublicKey,
                    EnablementCondition = g.ConditionExpression
                })
                .ToList();

            var questionCollection = new QuestionsAndGroupsCollectionView
            {
                Questions = questions,
                Groups = groups
            };

            groups.ForEach(x => UpdateBreadcrumbs(questionCollection, x, x.Id));
            questions.ForEach(x => UpdateBreadcrumbs(questionCollection, x, x.ParentGroupId));

            return questionCollection;
        }

        private static QuestionsAndGroupsCollectionView UpdateStateWithAddedGroup(QuestionsAndGroupsCollectionView currentState,
            Guid groupId, string title, string description, string enablementCondition, Guid parentGroupId)
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
                Description = description,
                EnablementCondition = enablementCondition,
                ParentGroupsIds = new Guid[0],
                RosterScopeIds = new Guid[0]
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

            var questionDetailsView = this.questionDetailsViewMapper.Map(question, parentGroupId);
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

       
    }
}

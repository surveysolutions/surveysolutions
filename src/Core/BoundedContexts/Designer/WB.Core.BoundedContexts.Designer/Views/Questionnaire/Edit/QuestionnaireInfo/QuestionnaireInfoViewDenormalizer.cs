using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    internal class QuestionnaireInfoViewDenormalizer :
        AbstractFunctionalEventHandler<QuestionnaireInfoView, IReadSideKeyValueStorage<QuestionnaireInfoView>>,
        IUpdateHandler<QuestionnaireInfoView, NewQuestionnaireCreated>,
        IUpdateHandler<QuestionnaireInfoView, QuestionnaireCloned>,
        IUpdateHandler<QuestionnaireInfoView, TemplateImported>,
        IUpdateHandler<QuestionnaireInfoView, QuestionnaireUpdated>,
        IUpdateHandler<QuestionnaireInfoView, NewGroupAdded>,
        IUpdateHandler<QuestionnaireInfoView, GroupCloned>,
        IUpdateHandler<QuestionnaireInfoView, GroupUpdated>,
        IUpdateHandler<QuestionnaireInfoView, GroupDeleted>,
        IUpdateHandler<QuestionnaireInfoView, QuestionnaireItemMoved>

    {

        private readonly Dictionary<string, string> groupTitles = new Dictionary<string, string>();

        public QuestionnaireInfoViewDenormalizer(IReadSideKeyValueStorage<QuestionnaireInfoView> writer)
            : base(writer)
        {
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView state, IPublishedEvent<NewQuestionnaireCreated> @event)
        {
            return CreateQuestionnaire(@event.EventSourceId, @event.Payload.Title, @event.Payload.IsPublic);
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView state, IPublishedEvent<QuestionnaireCloned> @event)
        {
            var newState = CreateQuestionnaire(@event.EventSourceId, @event.Payload.QuestionnaireDocument.Title, @event.Payload.QuestionnaireDocument.IsPublic);

            AddQuestionnaireItems(newState, @event.Payload.QuestionnaireDocument);

            return newState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView state, IPublishedEvent<TemplateImported> @event)
        {
            var newState = CreateQuestionnaire(@event.EventSourceId, @event.Payload.Source.Title, @event.Payload.Source.IsPublic);

            AddQuestionnaireItems(newState, @event.Payload.Source);

            return newState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView state, IPublishedEvent<QuestionnaireUpdated> @event)
        {
            state.Title = @event.Payload.Title;
            state.IsPublic = @event.Payload.IsPublic;

            return state;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView state, IPublishedEvent<NewGroupAdded> @event)
        {
            var groupId = @event.Payload.PublicKey.FormatGuid();

            if (!this.groupTitles.ContainsKey(groupId))
            {
                this.groupTitles.Add(groupId, @event.Payload.GroupText);    
            }

            if (!@event.Payload.ParentGroupPublicKey.HasValue ||
                @event.Payload.ParentGroupPublicKey.Value.FormatGuid() == state.QuestionnaireId)
            {
                CreateChapter(currentState: state, chapterId: groupId, chapterTitle: @event.Payload.GroupText);
            }

            return state;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView state, IPublishedEvent<GroupCloned> @event)
        {
            var groupId = @event.Payload.PublicKey.FormatGuid();

            if (!this.groupTitles.ContainsKey(groupId))
            {
                this.groupTitles.Add(groupId, @event.Payload.GroupText);
            }

            if (!@event.Payload.ParentGroupPublicKey.HasValue ||
                @event.Payload.ParentGroupPublicKey.Value.FormatGuid() == state.QuestionnaireId)
            {
                CreateChapter(currentState: state, chapterId: groupId, chapterTitle: @event.Payload.GroupText,
                    orderIndex: @event.Payload.TargetIndex);
            }

            return state;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView state, IPublishedEvent<GroupUpdated> @event)
        {
            var groupId = @event.Payload.GroupPublicKey.FormatGuid();

            if (this.groupTitles.ContainsKey(groupId))
            {
                this.groupTitles[groupId] = @event.Payload.GroupText;
            }

            var chapterView = state.Chapters.Find(chapter => chapter.ItemId == groupId);
            if (chapterView != null)
            {
                chapterView.Title = @event.Payload.GroupText;
            }

            return state;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView state, IPublishedEvent<GroupDeleted> @event)
        {
            var chapterView =
                state.Chapters.Find(chapter => chapter.ItemId == @event.Payload.GroupPublicKey.FormatGuid());
            if (chapterView != null)
            {
                state.Chapters.Remove(chapterView);
            }

            return state;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView state, IPublishedEvent<QuestionnaireItemMoved> @event)
        {
            var groupOrQuestionKey = @event.Payload.PublicKey.FormatGuid();

            var existsChapter = state.Chapters.Find(chapter => chapter.ItemId == groupOrQuestionKey);

            if (existsChapter != null)
            {
                state.Chapters.Remove(existsChapter);
            }

            if (!@event.Payload.GroupKey.HasValue ||
                @event.Payload.GroupKey.Value.FormatGuid() == state.QuestionnaireId)
            {
                string chapterTitle = null;
                if (!this.groupTitles.TryGetValue(groupOrQuestionKey, out chapterTitle))
                {
                    chapterTitle = Monads.Maybe(() => existsChapter.Title);
                }

                CreateChapter(currentState: state, 
                    chapterId: groupOrQuestionKey,
                    chapterTitle: chapterTitle, 
                    orderIndex: @event.Payload.TargetIndex);
            }

            return state;
        }

        private static QuestionnaireInfoView CreateQuestionnaire(Guid questionnaireId, string questionnaireTitle, bool isPublic)
        {
            var questionnaireInfo = new QuestionnaireInfoView()
            {
                QuestionnaireId = questionnaireId.FormatGuid(),
                Title = questionnaireTitle,
                Chapters = new List<ChapterInfoView>(),
                IsPublic = isPublic
            };
            return questionnaireInfo;
        }

        private static void CreateChapter(QuestionnaireInfoView currentState, string chapterId, string chapterTitle,
            int? orderIndex = null)
        {
            var chapterInfoView = new ChapterInfoView()
            {
                ItemId = chapterId,
                Title = chapterTitle,
                GroupsCount = 0,
                RostersCount = 0,
                QuestionsCount = 0
            };

            if (orderIndex.HasValue)
            {
                currentState.Chapters.Insert(orderIndex.Value, chapterInfoView);
            }
            else
            {
                currentState.Chapters.Add(chapterInfoView);
            }

        }

        private static void AddQuestionnaireItems(QuestionnaireInfoView currentState, IGroup sourceQuestionnaireOrGroup)
        {
            foreach (var chapter in sourceQuestionnaireOrGroup.Children.OfType<IGroup>())
            {
                CreateChapter(currentState: currentState, chapterId: chapter.PublicKey.FormatGuid(),
                    chapterTitle: chapter.Title);
            }
        }
    }
}

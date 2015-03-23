using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Utils;
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

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState, IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
            return CreateQuestionnaire(evnt.EventSourceId, evnt.Payload.Title, evnt.Payload.IsPublic);
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState, IPublishedEvent<QuestionnaireCloned> evnt)
        {
            var newState = CreateQuestionnaire(evnt.EventSourceId, evnt.Payload.QuestionnaireDocument.Title, evnt.Payload.QuestionnaireDocument.IsPublic);

            AddQuestionnaireItems(newState, evnt.Payload.QuestionnaireDocument);

            return newState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState, IPublishedEvent<TemplateImported> evnt)
        {
            var newState = CreateQuestionnaire(evnt.EventSourceId, evnt.Payload.Source.Title, evnt.Payload.Source.IsPublic);

            AddQuestionnaireItems(newState, evnt.Payload.Source);

            return newState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState, IPublishedEvent<QuestionnaireUpdated> evnt)
        {
            currentState.Title = evnt.Payload.Title;
            currentState.IsPublic = evnt.Payload.IsPublic;

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState, IPublishedEvent<NewGroupAdded> evnt)
        {
            var groupId = evnt.Payload.PublicKey.FormatGuid();

            if (!this.groupTitles.ContainsKey(groupId))
            {
                this.groupTitles.Add(groupId, evnt.Payload.GroupText);    
            }

            if (!evnt.Payload.ParentGroupPublicKey.HasValue ||
                evnt.Payload.ParentGroupPublicKey.Value.FormatGuid() == currentState.QuestionnaireId)
            {
                CreateChapter(currentState: currentState, chapterId: groupId, chapterTitle: evnt.Payload.GroupText);
            }

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState, IPublishedEvent<GroupCloned> evnt)
        {
            var groupId = evnt.Payload.PublicKey.FormatGuid();

            if (!this.groupTitles.ContainsKey(groupId))
            {
                this.groupTitles.Add(groupId, evnt.Payload.GroupText);
            }

            if (!evnt.Payload.ParentGroupPublicKey.HasValue ||
                evnt.Payload.ParentGroupPublicKey.Value.FormatGuid() == currentState.QuestionnaireId)
            {
                CreateChapter(currentState: currentState, chapterId: groupId, chapterTitle: evnt.Payload.GroupText,
                    orderIndex: evnt.Payload.TargetIndex);
            }

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState, IPublishedEvent<GroupUpdated> evnt)
        {
            var groupId = evnt.Payload.GroupPublicKey.FormatGuid();

            if (this.groupTitles.ContainsKey(groupId))
            {
                this.groupTitles[groupId] = evnt.Payload.GroupText;
            }

            var chapterView = currentState.Chapters.Find(chapter => chapter.ItemId == groupId);
            if (chapterView != null)
            {
                chapterView.Title = evnt.Payload.GroupText;
            }

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState, IPublishedEvent<GroupDeleted> evnt)
        {
            var chapterView =
                currentState.Chapters.Find(chapter => chapter.ItemId == evnt.Payload.GroupPublicKey.FormatGuid());
            if (chapterView != null)
            {
                currentState.Chapters.Remove(chapterView);
            }

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState, IPublishedEvent<QuestionnaireItemMoved> evnt)
        {
            var groupOrQuestionKey = evnt.Payload.PublicKey.FormatGuid();

            var existsChapter = currentState.Chapters.Find(chapter => chapter.ItemId == groupOrQuestionKey);

            if (existsChapter != null)
            {
                currentState.Chapters.Remove(existsChapter);
            }

            if (!evnt.Payload.GroupKey.HasValue ||
                evnt.Payload.GroupKey.Value.FormatGuid() == currentState.QuestionnaireId)
            {
                string chapterTitle = null;
                if (!this.groupTitles.TryGetValue(groupOrQuestionKey, out chapterTitle))
                {
                    chapterTitle = Monads.Maybe(() => existsChapter.Title);
                }

                CreateChapter(currentState: currentState, 
                    chapterId: groupOrQuestionKey,
                    chapterTitle: chapterTitle, 
                    orderIndex: evnt.Payload.TargetIndex);
            }

            return currentState;
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

using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    internal class QuestionnaireInfoViewDenormalizer :
        AbstractFunctionalEventHandler<QuestionnaireInfoView>,
        ICreateHandler<QuestionnaireInfoView, NewQuestionnaireCreated>,
        ICreateHandler<QuestionnaireInfoView, QuestionnaireCloned>,
        ICreateHandler<QuestionnaireInfoView, TemplateImported>,
        IUpdateHandler<QuestionnaireInfoView, QuestionnaireUpdated>,
        IUpdateHandler<QuestionnaireInfoView, NewGroupAdded>,
        IUpdateHandler<QuestionnaireInfoView, GroupCloned>,
        IUpdateHandler<QuestionnaireInfoView, GroupUpdated>,
        IUpdateHandler<QuestionnaireInfoView, GroupDeleted>,
        IUpdateHandler<QuestionnaireInfoView, NewQuestionAdded>,
        IUpdateHandler<QuestionnaireInfoView, QuestionCloned>,
        IUpdateHandler<QuestionnaireInfoView, QuestionDeleted>,
        IUpdateHandler<QuestionnaireInfoView, NumericQuestionAdded>,
        IUpdateHandler<QuestionnaireInfoView, NumericQuestionCloned>,
        IUpdateHandler<QuestionnaireInfoView, TextListQuestionAdded>,
        IUpdateHandler<QuestionnaireInfoView, TextListQuestionCloned>,
        IUpdateHandler<QuestionnaireInfoView, QRBarcodeQuestionAdded>,
        IUpdateHandler<QuestionnaireInfoView, QRBarcodeQuestionCloned>,
        IUpdateHandler<QuestionnaireInfoView, QuestionnaireItemMoved>

    {

        private readonly Dictionary<string, string> groupTitles = new Dictionary<string, string>();

        public QuestionnaireInfoViewDenormalizer(IReadSideRepositoryWriter<QuestionnaireInfoView> writer) : base(writer)
        {
        }

        public override Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public override Type[] BuildsViews
        {
            get { return base.BuildsViews.Union(new[] {typeof (QuestionnaireInfoView)}).ToArray(); }
        }

        public QuestionnaireInfoView Create(IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
            return CreateQuestionnaire(evnt.EventSourceId, evnt.Payload.Title, evnt.Payload.IsPublic);
        }

        public QuestionnaireInfoView Create(IPublishedEvent<QuestionnaireCloned> evnt)
        {
            var currentState = CreateQuestionnaire(evnt.EventSourceId, evnt.Payload.QuestionnaireDocument.Title, evnt.Payload.QuestionnaireDocument.IsPublic);

            AddQuestionnaireItems(currentState, evnt.Payload.QuestionnaireDocument);

            return currentState;
        }

        public QuestionnaireInfoView Create(IPublishedEvent<TemplateImported> evnt)
        {
            var currentState = CreateQuestionnaire(evnt.EventSourceId, evnt.Payload.Source.Title, evnt.Payload.Source.IsPublic);

            AddQuestionnaireItems(currentState, evnt.Payload.Source);

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState, IPublishedEvent<QuestionnaireUpdated> evnt)
        {
            currentState.Title = evnt.Payload.Title;

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

            currentState.GroupsCount += 1;

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

            currentState.GroupsCount += 1;

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

            currentState.GroupsCount -= 1;

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState, IPublishedEvent<NewQuestionAdded> evnt)
        {
            currentState.QuestionsCount += 1;

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState, IPublishedEvent<QuestionCloned> evnt)
        {
            currentState.QuestionsCount += 1;

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState, IPublishedEvent<NumericQuestionAdded> evnt)
        {
            currentState.QuestionsCount += 1;

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState, IPublishedEvent<NumericQuestionCloned> evnt)
        {
            currentState.QuestionsCount += 1;

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState, IPublishedEvent<TextListQuestionAdded> evnt)
        {
            currentState.QuestionsCount += 1;

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState, IPublishedEvent<TextListQuestionCloned> evnt)
        {
            currentState.QuestionsCount += 1;

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState, IPublishedEvent<QRBarcodeQuestionAdded> evnt)
        {
            currentState.QuestionsCount += 1;

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState, IPublishedEvent<QRBarcodeQuestionCloned> evnt)
        {
            currentState.QuestionsCount += 1;

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState, IPublishedEvent<QuestionDeleted> evnt)
        {
            currentState.QuestionsCount -= 1;

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
                CreateChapter(currentState: currentState, chapterId: groupOrQuestionKey,
                    chapterTitle: this.groupTitles[groupOrQuestionKey], orderIndex: evnt.Payload.TargetIndex);
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
                GroupsCount = 0,
                RostersCount = 0,
                QuestionsCount = 0,
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

            currentState.GroupsCount = sourceQuestionnaireOrGroup.Find<IGroup>(group => !group.IsRoster).Count();
            currentState.QuestionsCount = sourceQuestionnaireOrGroup.Find<IQuestion>(question => true).Count();
            currentState.RostersCount = sourceQuestionnaireOrGroup.Find<IGroup>(group => group.IsRoster).Count();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    internal class QuestionnaireInfoViewDenormalizer :
        AbstractFunctionalEventHandler<QuestionnaireInfoView>,
        ICreateHandler<QuestionnaireInfoView, NewQuestionnaireCreated>,
        ICreateHandler<QuestionnaireInfoView, QuestionnaireCloned>,
        IDeleteHandler<QuestionnaireInfoView, QuestionnaireDeleted>,
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
        IUpdateHandler<QuestionnaireInfoView, QRBarcodeQuestionCloned>

    {
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
            var questionnaireInfo = new QuestionnaireInfoView()
            {
                QuestionnaireId = evnt.EventSourceId.FormatGuid(),
                Title = evnt.Payload.Title,
                Chapters = new List<ChapterInfoView>(),
                GroupsCount = 0,
                RostersCount = 0,
                QuestionsCount = 0
            };

            return questionnaireInfo;
        }

        public QuestionnaireInfoView Create(IPublishedEvent<QuestionnaireCloned> evnt)
        {
            var questionnaireInfo = new QuestionnaireInfoView()
            {
                QuestionnaireId = evnt.EventSourceId.FormatGuid(),
                Title = evnt.Payload.QuestionnaireDocument.Title,
                Chapters = new List<ChapterInfoView>(),
                GroupsCount = 0,
                RostersCount = 0,
                QuestionsCount = 0
            };

            return questionnaireInfo;
        }

        public void Delete(QuestionnaireInfoView currentState, IPublishedEvent<QuestionnaireDeleted> evnt)
        {

        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState, IPublishedEvent<QuestionnaireUpdated> evnt)
        {
            currentState.Title = evnt.Payload.Title;

            return currentState;
        }


        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState, IPublishedEvent<NewGroupAdded> evnt)
        {
            if (!evnt.Payload.ParentGroupPublicKey.HasValue ||
                evnt.Payload.ParentGroupPublicKey.Value.FormatGuid() == currentState.QuestionnaireId)
            {
                var chapterInfoView = new ChapterInfoView()
                {
                    ChapterId = evnt.Payload.PublicKey.FormatGuid(),
                    Title = evnt.Payload.GroupText,
                    GroupsCount = 0,
                    RostersCount = 0,
                    QuestionsCount = 0
                };

                currentState.Chapters.Add(chapterInfoView);
            }

            currentState.GroupsCount += 1;

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState,IPublishedEvent<GroupCloned> evnt)
        {
            if (!evnt.Payload.ParentGroupPublicKey.HasValue ||
                evnt.Payload.ParentGroupPublicKey.Value.FormatGuid() == currentState.QuestionnaireId)
            {
                var chapterInfoView = new ChapterInfoView()
                {
                    ChapterId = evnt.Payload.PublicKey.FormatGuid(),
                    Title = evnt.Payload.GroupText,
                    GroupsCount = 0,
                    RostersCount = 0,
                    QuestionsCount = 0
                };
                currentState.Chapters.Add(chapterInfoView); 
            }

            currentState.GroupsCount += 1;

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState,IPublishedEvent<GroupUpdated> evnt)
        {
            var chapterView = currentState.Chapters.Find(chapter => chapter.ChapterId == evnt.Payload.GroupPublicKey.FormatGuid());
            if (chapterView != null)
            {
                chapterView.Title = evnt.Payload.GroupText;
            }

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState,IPublishedEvent<GroupDeleted> evnt)
        {
            var chapterView = currentState.Chapters.Find(chapter => chapter.ChapterId == evnt.Payload.GroupPublicKey.FormatGuid());
            if (chapterView != null)
            {
                currentState.Chapters.Remove(chapterView);
            }

            currentState.GroupsCount -= 1;

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState,IPublishedEvent<NewQuestionAdded> evnt)
        {
            currentState.QuestionsCount += 1;

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState,IPublishedEvent<QuestionCloned> evnt)
        {
            currentState.QuestionsCount += 1;

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState,IPublishedEvent<NumericQuestionAdded> evnt)
        {
            currentState.QuestionsCount += 1;

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState,IPublishedEvent<NumericQuestionCloned> evnt)
        {
            currentState.QuestionsCount += 1;

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState,IPublishedEvent<TextListQuestionAdded> evnt)
        {
            currentState.QuestionsCount += 1;

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState,IPublishedEvent<TextListQuestionCloned> evnt)
        {
            currentState.QuestionsCount += 1;

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState,IPublishedEvent<QRBarcodeQuestionAdded> evnt)
        {
            currentState.QuestionsCount += 1;

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState,IPublishedEvent<QRBarcodeQuestionCloned> evnt)
        {
            currentState.QuestionsCount += 1;

            return currentState;
        }

        public QuestionnaireInfoView Update(QuestionnaireInfoView currentState, IPublishedEvent<QuestionDeleted> evnt)
        {
            currentState.QuestionsCount -= 1;

            return currentState;
        }
    }
}

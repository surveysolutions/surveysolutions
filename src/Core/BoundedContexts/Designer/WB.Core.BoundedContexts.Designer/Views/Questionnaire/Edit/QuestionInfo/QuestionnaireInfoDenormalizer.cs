using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    internal class QuestionnaireInfoDenormalizer : AbstractFunctionalEventHandler<QuestionDetailsCollectionView>,
        ICreateHandler<QuestionDetailsCollectionView, NewQuestionnaireCreated>,
        ICreateHandler<QuestionDetailsCollectionView, QuestionnaireCloned>,
        ICreateHandler<QuestionDetailsCollectionView, TemplateImported>,
        IUpdateHandler<QuestionDetailsCollectionView, NewQuestionAdded>,
        IUpdateHandler<QuestionDetailsCollectionView, QuestionChanged>,
        IUpdateHandler<QuestionDetailsCollectionView, QuestionCloned>,
        IUpdateHandler<QuestionDetailsCollectionView, QuestionDeleted>,
        IUpdateHandler<QuestionDetailsCollectionView, NumericQuestionAdded>,
        IUpdateHandler<QuestionDetailsCollectionView, NumericQuestionChanged>,
        IUpdateHandler<QuestionDetailsCollectionView, NumericQuestionCloned>,
        IUpdateHandler<QuestionDetailsCollectionView, TextListQuestionAdded>,
        IUpdateHandler<QuestionDetailsCollectionView, TextListQuestionChanged>,
        IUpdateHandler<QuestionDetailsCollectionView, TextListQuestionCloned>,
        IUpdateHandler<QuestionDetailsCollectionView, QRBarcodeQuestionAdded>,
        IUpdateHandler<QuestionDetailsCollectionView, QRBarcodeQuestionUpdated>,
        IUpdateHandler<QuestionDetailsCollectionView, QRBarcodeQuestionCloned>,
        IUpdateHandler<QuestionDetailsCollectionView, QuestionnaireItemMoved>
        //,IDeleteHandler<QuestionDetailsCollectionView, QuestionnaireDeleted>
    {
        private readonly IQuestionDetailsFactory questionDetailsFactory;
        private readonly IQuestionFactory questionFactory;

        public QuestionnaireInfoDenormalizer(IReadSideRepositoryWriter<QuestionDetailsCollectionView> readsideRepositoryWriter,
            IQuestionDetailsFactory questionDetailsFactory, IQuestionFactory questionFactory)
            : base(readsideRepositoryWriter)
        {
            this.questionDetailsFactory = questionDetailsFactory;
            this.questionFactory = questionFactory;
        }

        public override Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public override Type[] BuildsViews
        {
            get { return base.BuildsViews.Union(new[] { typeof(QuestionDetailsCollectionView) }).ToArray(); }
        }

        public QuestionDetailsCollectionView Create(IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
            var questionCollection = new QuestionDetailsCollectionView
            {
                Questions = new List<QuestionDetailsView>()
            };
            return questionCollection;
        }

        public QuestionDetailsCollectionView Create(IPublishedEvent<QuestionnaireCloned> evnt)
        {
            var questionCollection = new QuestionDetailsCollectionView
            {
                Questions = this.GetAllQuestions(evnt.Payload.QuestionnaireDocument)
            };
            return questionCollection;
        }

        public QuestionDetailsCollectionView Create(IPublishedEvent<TemplateImported> evnt)
        {
            var questionCollection = new QuestionDetailsCollectionView
            {
                Questions = this.GetAllQuestions(evnt.Payload.Source)
            };
            return questionCollection;
        }

        private List<QuestionDetailsView> GetAllQuestions(QuestionnaireDocument questionnaire)
        {
            questionnaire.ConnectChildrenWithParent();
            return
                questionnaire.GetAllQuestions<IQuestion>()
                    .Select(question => this.questionDetailsFactory.CreateQuestion(question, question.GetParent().PublicKey))
                    .Where(q => q != null)
                    .ToList();
        }

        public QuestionDetailsCollectionView Update(QuestionDetailsCollectionView currentState, IPublishedEvent<NewQuestionAdded> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.NewQuestionAddedToQuestionData(evnt));
            currentState.Questions.Add(questionDetailsFactory.CreateQuestion(question, evnt.Payload.GroupPublicKey.Value));
            return currentState;
        }

        public QuestionDetailsCollectionView Update(QuestionDetailsCollectionView currentState, IPublishedEvent<QuestionCloned> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.QuestionClonedToQuestionData(evnt));
            currentState.Questions.Add(questionDetailsFactory.CreateQuestion(question, evnt.Payload.GroupPublicKey.Value));
            return currentState;
        }

        public QuestionDetailsCollectionView Update(QuestionDetailsCollectionView currentState, IPublishedEvent<QuestionChanged> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.QuestionChangedToQuestionData(evnt));
            var oldQuestion = currentState.Questions.FirstOrDefault(x => x.Id == evnt.Payload.PublicKey);
            if (oldQuestion == null)
            {
                return currentState;
            }
            currentState.Questions.Remove(oldQuestion);
            currentState.Questions.Add(questionDetailsFactory.CreateQuestion(question, oldQuestion.ParentGroupId));
            return currentState;
        }

        public QuestionDetailsCollectionView Update(QuestionDetailsCollectionView currentState, IPublishedEvent<NumericQuestionAdded> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.NumericQuestionAddedToQuestionData(evnt));
            currentState.Questions.Add(this.questionDetailsFactory.CreateQuestion(question, evnt.Payload.GroupPublicKey));
            return currentState;
        }

        public QuestionDetailsCollectionView Update(QuestionDetailsCollectionView currentState, IPublishedEvent<NumericQuestionChanged> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.NumericQuestionChangedToQuestionData(evnt));
            var oldQuestion = currentState.Questions.FirstOrDefault(x => x.Id == evnt.Payload.PublicKey);
            if (oldQuestion == null)
            {
                return currentState;
            }
            currentState.Questions.Remove(oldQuestion);
            currentState.Questions.Add(this.questionDetailsFactory.CreateQuestion(question, oldQuestion.ParentGroupId));
            return currentState;
        }

        public QuestionDetailsCollectionView Update(QuestionDetailsCollectionView currentState, IPublishedEvent<NumericQuestionCloned> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.NumericQuestionClonedToQuestionData(evnt));
            currentState.Questions.Add(this.questionDetailsFactory.CreateQuestion(question, evnt.Payload.GroupPublicKey));
            return currentState;
        }

        public QuestionDetailsCollectionView Update(QuestionDetailsCollectionView currentState, IPublishedEvent<TextListQuestionAdded> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.TextListQuestionAddedToQuestionData(evnt));
            currentState.Questions.Add(this.questionDetailsFactory.CreateQuestion(question, evnt.Payload.GroupId));
            return currentState;
        }

        public QuestionDetailsCollectionView Update(QuestionDetailsCollectionView currentState, IPublishedEvent<TextListQuestionCloned> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.TextListQuestionClonedToQuestionData(evnt));
            currentState.Questions.Add(this.questionDetailsFactory.CreateQuestion(question, evnt.Payload.GroupId));
            return currentState;
        }

        public QuestionDetailsCollectionView Update(QuestionDetailsCollectionView currentState,
            IPublishedEvent<TextListQuestionChanged> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.TextListQuestionChangedToQuestionData(evnt));
            var oldQuestion = currentState.Questions.FirstOrDefault(x => x.Id == evnt.Payload.PublicKey);
            if (oldQuestion == null)
            {
                return currentState;
            }
            currentState.Questions.Remove(oldQuestion);
            currentState.Questions.Add(this.questionDetailsFactory.CreateQuestion(question, oldQuestion.ParentGroupId));
            return currentState;
        }

        public QuestionDetailsCollectionView Update(QuestionDetailsCollectionView currentState, IPublishedEvent<QRBarcodeQuestionAdded> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.QRBarcodeQuestionAddedToQuestionData(evnt));
            currentState.Questions.Add(this.questionDetailsFactory.CreateQuestion(question, evnt.Payload.ParentGroupId));
            return currentState;
        }

        public QuestionDetailsCollectionView Update(QuestionDetailsCollectionView currentState,
            IPublishedEvent<QRBarcodeQuestionUpdated> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.QRBarcodeQuestionUpdatedToQuestionData(evnt));
            var oldQuestion = currentState.Questions.FirstOrDefault(x => x.Id == evnt.Payload.QuestionId);
            if (oldQuestion == null)
            {
                return currentState;
            }
            currentState.Questions.Remove(oldQuestion);
            currentState.Questions.Add(this.questionDetailsFactory.CreateQuestion(question, oldQuestion.ParentGroupId));
            return currentState;
        }

        public QuestionDetailsCollectionView Update(QuestionDetailsCollectionView currentState,
            IPublishedEvent<QRBarcodeQuestionCloned> evnt)
        {
            IQuestion question = this.questionFactory.CreateQuestion(EventConverter.QRBarcodeQuestionClonedToQuestionData(evnt));
            currentState.Questions.Add(this.questionDetailsFactory.CreateQuestion(question, evnt.Payload.ParentGroupId));
            return currentState;
        }

        public QuestionDetailsCollectionView Update(QuestionDetailsCollectionView currentState, IPublishedEvent<QuestionDeleted> evnt)
        {
            var oldQuestion = currentState.Questions.FirstOrDefault(x => x.Id == evnt.Payload.QuestionId);
            currentState.Questions.Remove(oldQuestion);
            return currentState;
        }

        public QuestionDetailsCollectionView Update(QuestionDetailsCollectionView currentState, IPublishedEvent<QuestionnaireItemMoved> evnt)
        {
            var question = currentState.Questions.FirstOrDefault(x => x.Id == evnt.Payload.PublicKey);
            if (question != null && evnt.Payload.GroupKey != null)
                question.ParentGroupId = evnt.Payload.GroupKey.Value;
            return currentState;
        }

        //public void Delete(QuestionDetailsCollectionView currentState, IPublishedEvent<QuestionnaireDeleted> evnt) { }
    }
}

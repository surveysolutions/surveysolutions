using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    internal class QuestionnaireInfoDenormalizer : AbstractFunctionalEventHandler<QuestionDetailsCollectionView>,
        ICreateHandler<QuestionDetailsCollectionView, NewQuestionnaireCreated>,
        ICreateHandler<QuestionDetailsCollectionView, QuestionnaireCloned>,
        ICreateHandler<QuestionDetailsCollectionView, TemplateImported>
    {
        private readonly IQuestionDetailsFactory questionDetailsFactory;
        public QuestionnaireInfoDenormalizer(IReadSideRepositoryWriter<QuestionDetailsCollectionView> readsideRepositoryWriter,
            IQuestionDetailsFactory questionDetailsFactory)
            : base(readsideRepositoryWriter)
        {
            this.questionDetailsFactory = questionDetailsFactory;
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
            var questionCollection = new QuestionDetailsCollectionView();
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
            return questionnaire.GetAllQuestions<IQuestion>().Select(question => this.questionDetailsFactory.CreateQuestion(question)).ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.AbstractFactories;
using Main.Core.Documents;
using Main.Core.Domain;
using Main.Core.Entities.Composite;
using Main.Core.Entities.Extensions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.Events.Questionnaire;
using Main.Core.Utility;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Domain;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using WB.Core.GenericSubdomains.Logging;

namespace WB.Core.SharedKernels.DataCollection.Aggregates
{
    public class Questionnaire : AggregateRootMappedByConvention, ISnapshotable<QuestionnaireDocument>, IQuestionnaire
    {
        private QuestionnaireDocument innerDocument = new QuestionnaireDocument();

        public Questionnaire(){}

        public Questionnaire(Guid createdBy, IQuestionnaireDocument source)
            : base(source.PublicKey)
        {
            ImportQuestionnaire(createdBy, source);
        }

        public QuestionnaireDocument CreateSnapshot()
        {
            return this.innerDocument;
        }

        public void RestoreFromSnapshot(QuestionnaireDocument snapshot)
        {
            this.innerDocument = snapshot.Clone() as QuestionnaireDocument;
        }

        public void ImportQuestionnaire(Guid createdBy, IQuestionnaireDocument source)
        {
           
            var document = source as QuestionnaireDocument;
            if (document == null)
                throw new DomainException(DomainExceptionType.TemplateIsInvalid
                                          , "only QuestionnaireDocuments are supported for now");
            document.CreatedBy = this.innerDocument.CreatedBy;
            ApplyEvent(new TemplateImported() {Source = document});
           
        }

        public void CreateInterviewWithFeaturedQuestions(Guid interviewId, UserLight creator, UserLight responsible, List<QuestionAnswer> featuredAnswers)
        #warning probably a factory should be used here
        {
            // TODO: check is it good to create new AR form another?
            new CompleteQuestionnaireAR(interviewId, this.innerDocument, creator, responsible, featuredAnswers);
        }

        public void CreateCompletedQ(Guid completeQuestionnaireId, UserLight creator)
        #warning probably a factory should be used here
        {
            // TODO: check is it good to create new AR form another?
            // Do we need Saga here?
            new CompleteQuestionnaireAR(completeQuestionnaireId, this.innerDocument, creator);
        }

        protected internal void OnTemplateImported(TemplateImported e)
        {
            this.innerDocument = e.Source;
        }
    }
}
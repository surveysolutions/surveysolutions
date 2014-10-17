using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration
{
    internal static class Create
    {
        private static IPublishedEvent<T> ToPublishedEvent<T>(T @event)
            where T : class
        {
            return ToPublishedEvent<T>(@event, Guid.NewGuid());
        }

        private static IPublishedEvent<T> ToPublishedEvent<T>(T @event, Guid eventSourceId)
            where T : class
        {
            return Mock.Of<IPublishedEvent<T>>(publishedEvent
                => publishedEvent.Payload == @event &&
                   publishedEvent.EventSourceId == eventSourceId);
        }

        public static QuestionnaireDocument QuestionnaireDocument(Guid id, params IComposite[] questions)
        {
            var questionnaireDocument = new QuestionnaireDocument { PublicKey = id };

            questions.ToList().ForEach(q => questionnaireDocument.Add(q, id, null));

            return questionnaireDocument;
        }

        public static NumericQuestion NumericIntegerQuestion(Guid id, string variable, string conditition = null, string validationExpression = null)
        {
            return new NumericQuestion
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = id,
                StataExportCaption = variable,
                IsInteger = true,
                ConditionExpression = conditition,
                ValidationExpression = validationExpression
            };
        }

        public static Group Group(Guid id, string variable = null, string conditition = null)
        {
            return new Group
            {
                PublicKey = id,
                VariableName = variable,
                ConditionExpression = conditition                
            };
        }

        public static Questionnaire Questionnaire(Guid actorId, QuestionnaireDocument questionnaireDocument)
        {
            return new Questionnaire(actorId, questionnaireDocument, false, string.Empty);
        }

        public static Interview Interview(Guid? interviewId = null, Guid? userId = null, Guid? questionnaireId = null,
            Dictionary<Guid, object> answersToFeaturedQuestions = null, DateTime? answersTime = null, Guid? supervisorId = null)
        {
            return new Interview(
                interviewId ?? new Guid("A0A0A0A0B0B0B0B0A0A0A0A0B0B0B0B0"),
                userId ?? new Guid("F111F111F111F111F111F111F111F111"),
                questionnaireId ?? new Guid("B000B000B000B000B000B000B000B000"), 1,
                answersToFeaturedQuestions ?? new Dictionary<Guid, object>(),
                answersTime ?? new DateTime(2012, 12, 20),
                supervisorId ?? new Guid("D222D222D222D222D222D222D222D222"));
        }
    }
}

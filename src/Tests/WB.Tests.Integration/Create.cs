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

        public static QuestionnaireDocument QuestionnaireDocument(Guid? id = null, params IComposite[] children)
        {
            return new QuestionnaireDocument
            {
                PublicKey = id ?? Guid.NewGuid(),
                Children = children != null ? children.ToList() : new List<IComposite>(),
            };
        }

        public static Group Chapter(string title = "Chapter X", IEnumerable<IComposite> children = null)
        {
            return Create.Group(
                title: title,
                children: children);
        }

        public static IQuestion Question(Guid? questionId = null, string variable = null, string enablementCondition = null, string validationExpression = null)
        {
            return new TextQuestion("Question X")
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
            };
        }

        public static NumericQuestion NumericIntegerQuestion(Guid id, string variable, string enablementCondition = null, string validationExpression = null)
        {
            return new NumericQuestion
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = id,
                StataExportCaption = variable,
                IsInteger = true,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression
            };
        }

        public static DateTimeQuestion DateTimeQuestion(Guid id, string variable, string enablementCondition = null, string validationExpression = null)
        {
            return new DateTimeQuestion
            {
                PublicKey = id,
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression
            };
        }

        public static Group Roster(Guid? id = null, string title = "Roster X", string variable = null, string enablementCondition = null,
            IEnumerable<IComposite> children = null)
        {
            Group group = Create.Group(
                id: id,
                title: title,
                variable: variable,
                enablementCondition: enablementCondition,
                children: children);

            group.IsRoster = true;
            group.RosterSizeSource = RosterSizeSourceType.FixedTitles;
            group.RosterFixedTitles = new[] { "Roster X-1", "Roster X-2" };

            return group;
        }

        public static Group Group(Guid? id = null, string title = "Group X", string variable = null,
            string enablementCondition = null, IEnumerable<IComposite> children = null)
        {
            return new Group(title)
            {
                PublicKey = id ?? Guid.NewGuid(),
                VariableName = variable,
                ConditionExpression = enablementCondition,              
                Children = children != null ? children.ToList() : new List<IComposite>(),
            };
        }

        public static Questionnaire Questionnaire(QuestionnaireDocument questionnaireDocument)
        {
            return new Questionnaire(Guid.NewGuid(), questionnaireDocument, false, string.Empty);
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

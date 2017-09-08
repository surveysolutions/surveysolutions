using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

//using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services
{
    [TestFixture]
    [TestOf(typeof(InterviewDetailsViewFactory))]
    public class InterviewDetailsViewFactoryTests
    {
        [Test]
        public void when_requesting_details_having_hidden_and_disabled_questions()
        {
            var interviewGuid =  Guid.Parse("11111111111111111111111111111110");

            var question1id = Guid.Parse("11111111111111111111111111111111");
            var question2id = Guid.Parse("11111111111111111111111111111112");
            var question3id = Guid.Parse("11111111111111111111111111111113");
            var question4id = Guid.Parse("11111111111111111111111111111114");

            var supervisorId = Guid.Parse("91111111111111111111111111111112");
            var user = Guid.Parse("81111111111111111111111111111112");

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: question1id, variable:"num1"),
                Create.Entity.NumericIntegerQuestion(id: question2id, variable:"num2", scope:QuestionScope.Hidden),
                Create.Entity.NumericIntegerQuestion(id: question3id, variable:"num3", enablementCondition:"1!=1"),
                Create.Entity.NumericIntegerQuestion(id: question4id, variable:"num4"),
            });

            var interview = Create.AggregateRoot.StatefulInterview(questionnaireDocument.PublicKey, interviewGuid, questionnaireDocument);

            interview.AnswerNumericIntegerQuestion(user, question1id, RosterVector.Empty,DateTime.Now, 1 );

            interview.ApplyEvent(new QuestionsDisabled(new Identity[]{new Identity(question3id,RosterVector.Empty) }));

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewGuid.FormatGuid()) == interview);

            var interviewStatuses = new TestInMemoryWriter<InterviewSummary>();

            var interviewsInStaus = new List<InterviewCommentedStatus>();

            interviewsInStaus.Add(Create.Entity.InterviewCommentedStatus(interviewerId: user,
                supervisorId: supervisorId,
                timestamp: DateTime.Now.Date.AddHours(-1)));

            interviewStatuses.Store(
                Create.Entity.InterviewSummary(questionnaireId: questionnaireDocument.PublicKey,
                    questionnaireVersion: 1,
                    statuses: interviewsInStaus.ToArray()), interviewGuid.FormatGuid());

            var questionnaireRepository = Mock.Of<IQuestionnaireStorage>(repository
                => repository.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == Create.Entity.PlainQuestionnaire(questionnaireDocument, 1) 
                &&
                   repository.GetQuestionnaireDocument(It.IsAny<QuestionnaireIdentity>()) == questionnaireDocument);

            var factory = Create.Service.InterviewDetailsViewFactory(
                statefulInterviewRepository:interviewRepository, 
                interviewSummaryRepository: interviewStatuses,
                questionnaireStorage: questionnaireRepository);

            //Act
            var details = factory.GetInterviewDetails(interviewGuid, InterviewDetailsFilter.Unanswered, null);

            Assert.That(details.Statistic.UnansweredCount, Is.EqualTo(1));
            Assert.That(details.Statistic.AnsweredCount, Is.EqualTo(1));
            Assert.That(details.Statistic.HiddenCount, Is.EqualTo(1));

            Assert.That(details.FilteredEntities.Count(), Is.EqualTo(2));
        }
    }
}

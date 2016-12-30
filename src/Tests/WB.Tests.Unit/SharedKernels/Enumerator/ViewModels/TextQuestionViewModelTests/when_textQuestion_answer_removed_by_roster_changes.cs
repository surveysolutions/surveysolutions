using System;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Repositories;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.TextQuestionViewModelTests
{
    public class when_textQuestion_answer_removed_by_roster_changes
    {
        StatefulInterview interview;
        
        static readonly Guid rosterId = Guid.Parse("22222222222222222222222222222222");
        static readonly Guid rosterQuestionId = Guid.Parse("55555555555555555555555555555555");
        static readonly Guid sourceOfLinkedQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static readonly Guid interviewerId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

        [Test]
        public void should_set_textQuestionViewModel_Answer_empty()
        {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                Create.Entity.MultipleOptionsQuestion(questionId: rosterQuestionId, 
                    textAnswers: new []{ Create.Entity.Option("1", "Multi 1"), Create.Entity.Option("2", "Multi 2") }),

                Create.Entity.Roster(rosterId: rosterId, rosterSizeQuestionId: rosterQuestionId, children: new IComposite[]
                {
                    Create.Entity.TextQuestion(questionId: sourceOfLinkedQuestionId),
                })
            });

            var plainQuestionnaire = new PlainQuestionnaire(questionnaireDocument, 0);

            this.interview = Create.AggregateRoot.StatefulInterview(questionnaire: plainQuestionnaire);

            var statefulInterviewRepository = Mock.Of<IStatefulInterviewRepository>(x => 
                x.Get(this.interview.Id.ToString()) == this.interview
            );

            this.interview.AnswerMultipleOptionsQuestion(interviewerId, rosterQuestionId, Create.Entity.RosterVector(), DateTime.UtcNow, new[] { 1, 2 });
            this.interview.AnswerTextQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(1), DateTime.UtcNow, "answer 0.1");

            var textQuestionIdentity = Create.Entity.Identity(sourceOfLinkedQuestionId, Create.Entity.RosterVector(1));

            var textQuestionViewModel = Create.ViewModel.TextQuestionViewModel(interviewRepository: statefulInterviewRepository);
            textQuestionViewModel.Init(this.interview.Id.ToString(), textQuestionIdentity, Create.Other.NavigationState());

            //act
            this.interview.AnswerMultipleOptionsQuestion(interviewerId, rosterQuestionId, Create.Entity.RosterVector(), DateTime.UtcNow, new int[0]);
            textQuestionViewModel.Handle(new AnswersRemoved(new [] { textQuestionIdentity}));

            //assert
            Assert.That(textQuestionViewModel.Answer, Is.Empty);
        }
    }
}
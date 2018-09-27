using System;
using System.Linq;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    [TestFixture]
    internal class when_answering_question_with_validation_refering_nested_rosters_from_the_same_level : InterviewTestsContext
    {
        [OneTimeSetUp]
        public void Setup()
        {
            Integration.SetUp.MockedServiceLocator();

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var priceId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var unitsId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var userId = Guid.NewGuid();
            var roster1Id = Guid.Parse("11111111111111111111111111111111");
            var roster2Id = Guid.Parse("22222222222222222222222222222222");

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                Create.Entity.FixedRoster(rosterId: roster1Id, fixedTitles: new[] { Create.Entity.FixedTitle(1), Create.Entity.FixedTitle(2) }, variable: "food", children: new IComposite[]
                {
                    Create.Entity.MultipleOptionsQuestion(unitsId, variable: "units", answers: new[] {10, 11, 12}),
                    Create.Entity.MultiRoster(roster2Id, variable: "units_r", rosterSizeQuestionId: unitsId, children: new IComposite[]
                    {
                        Create.Entity.NumericIntegerQuestion(priceId, variable: "price", validationExpression: "units_r.Where(x=>x.@rowcode<@rowcode).All(y=>y.price<price)")
                    })
                })
            );

            var interview = SetupInterview(questionnaireDocument);

            interview.AnswerMultipleOptionsQuestion(userId, unitsId, Create.RosterVector(1), DateTime.Now, new[] { 10, 11, 12 });
            interview.AnswerNumericIntegerQuestion(userId, priceId, Create.RosterVector(1, 10), DateTime.Now, 10);
            interview.AnswerNumericIntegerQuestion(userId, priceId, Create.RosterVector(1, 11), DateTime.Now, 20);

            using (var eventContext = new EventContext())
            {
                interview.AnswerNumericIntegerQuestion(userId, priceId, Create.RosterVector(1, 12), DateTime.Now, 5);

                var invalidQuestions = eventContext.GetSingleEvent<AnswersDeclaredInvalid>().Questions;

                results.SmallestPriceIsInvalid = invalidQuestions.Contains(Create.Identity(priceId, Create.RosterVector(1, 10)));
                results.MiddlePriceIsInvalid = invalidQuestions.Contains(Create.Identity(priceId, Create.RosterVector(1, 11)));
                results.HighestPriceIsInvalid = invalidQuestions.Contains(Create.Identity(priceId, Create.RosterVector(1, 12)));
            }
        }

        [Test]
        public void should_mark_smallest_price_as_valid()
        {
            Assert.That(results.SmallestPriceIsInvalid, Is.False);
        }

        [Test]
        public void should_mark_middle_price_as_valid()
        {
            Assert.That(results.MiddlePriceIsInvalid, Is.False);
        }

        [Test]
        public void should_mark_highest_price_as_invalid()
        {
            Assert.That(results.HighestPriceIsInvalid, Is.True);
        }

        private static InvokeResults results = new InvokeResults();

        [Serializable]
        internal class InvokeResults
        {
            public bool SmallestPriceIsInvalid { get; set; }
            public bool MiddlePriceIsInvalid { get; set; }
            public bool HighestPriceIsInvalid { get; set; }
        }
    }
}
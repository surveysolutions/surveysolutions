using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.API.WebInterview.Services;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Filtering
{
    public class StatefullInterviewSearchTests
    {
        StatefulInterview interview;

        [OneTimeSetUp]
        public void Setup()
        {
            var document = Create.Entity.QuestionnaireDocument(Guid.NewGuid(),
                Create.Entity.Group(Id.IdentityA.Id, "Group A", children: new IComposite[]
                {
                    Create.Entity.Variable(), // should not count
                    Create.Entity.TextQuestion(Id.Identity1.Id, text: "Text 1 Flagged", variable: "text1"),
                    Create.Entity.TextQuestion(Id.Identity2.Id, text: "Text Super A", variable: "text2", scope: Main.Core.Entities.SubEntities.QuestionScope.Supervisor),
                    Create.Entity.TextQuestion(Id.Identity3.Id, text: "Text 3 With Comment", variable: "text3"),
                    Create.Entity.FixedRoster(Id.IdentityD.Id, title: "roster", children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(Id.Identity6.Id, text: "Text Roster Flagged", variable: "textInRoster"),
                    }, fixedTitles: new [] { Create.Entity.FixedTitle(1.0m, "Test") }),
                    Create.Entity.StaticText(Id.Identity4.Id, text: "StaticText 4 Invalid"),
                    Create.Entity.TextQuestion(Id.Identity5.Id, text: "Text 5 With Comment Flagged Answered", variable: "text5")
                }),
                Create.Entity.Group(Id.gB, "Group B", children: new IComposite[]
                {
                    Create.Entity.TextQuestion(Id.Identity7.Id, text: "Text 7 Answered Invalid", variable: "text6"),
                    Create.Entity.TextQuestion(Id.Identity8.Id, text: "Text Super B", variable: "text7", scope: Main.Core.Entities.SubEntities.QuestionScope.Supervisor),
                    Create.Entity.StaticText(Id.Identity9.Id, text: "StaticText 9"),
                    Create.Entity.TextQuestion(Id.Identity10.Id, text: "Text 10", variable: "text9")
                }));

            interview = Create.AggregateRoot.StatefulInterview(Guid.NewGuid(), questionnaire: document);

            interview.Apply(Create.Event.AnswersDeclaredInvalid(Id.Identity7));
            interview.Apply(Create.Event.StaticTextsDeclaredInvalid(Id.Identity4));
            interview.Apply(Create.Event.TextQuestionAnswered(Id.Identity5.Id));
            interview.Apply(Create.Event.TextQuestionAnswered(Id.Identity7.Id));
            interview.Apply(Create.Event.AnswerCommented(Id.Identity3.Id));
            interview.Apply(Create.Event.AnswerCommented(Id.Identity5.Id));

            var interviewFactory = Mock.Of<IInterviewFactory>(f => f.GetFlaggedQuestionIds(interview.Id) == new[]
            {
                Id.Identity1,
                Id.Identity5,
                Id.Identity6
            });

            Subject = new StatefullInterviewSearcher(interviewFactory);
        }

        public StatefullInterviewSearcher Subject { get; set; }

        [TestCaseSource(nameof(SingleFilteringCaseData))]
        public void ShouldFilterBySingleOption(FilterTestCase @case)
        {
            var search = Subject.Search(interview, @case.Options.ToArray(), 0, 100);

            AssertSearchReturnFollowingIds(search, @case.Results.Select(id => id).ToArray());
        }

        public static FilterTestCase[] SingleFilteringCaseData =
        {
            Case(FilterOption.Flagged).Result(Id.Identity1, Id.Identity5),
            Case(FilterOption.NotFlagged).ResultExcept(Id.Identity1, Id.Identity5, Id.Identity4, Id.Identity9),

            Case(FilterOption.Answered).Result(Id.Identity5, Id.Identity7),
            Case(FilterOption.NotAnswered).ResultExcept(Id.Identity5, Id.Identity7, Id.Identity4, Id.Identity9),

            Case(FilterOption.Invalid).Result(Id.Identity4, Id.Identity7),
            Case(FilterOption.Valid).ResultExcept(Id.Identity4, Id.Identity7),

            Case(FilterOption.ForSupervisor).Result(Id.Identity2, Id.Identity8),
            Case(FilterOption.ForInterviewer).ResultExcept(Id.Identity2, Id.Identity8, Id.Identity4, Id.Identity9),

            Case(FilterOption.WithComments).Result(Id.Identity3, Id.Identity5)
        };

        [TestCaseSource(nameof(MultipleFilteringCaseData))]
        public void ShouldFilterByMultipleOption(FilterTestCase @case)
        {
            var search = Subject.Search(interview, @case.Options.ToArray(), 0, 100);
            AssertSearchReturnFollowingIds(search, @case.Results.Select(id => id).ToArray());
        }

        public static FilterTestCase[] MultipleFilteringCaseData =
        {
            Case(FilterOption.Flagged, FilterOption.WithComments).Result(Id.Identity5),
            Case(FilterOption.Flagged, FilterOption.NotAnswered).Result(Id.Identity1),

            Case(FilterOption.Flagged, FilterOption.Valid).Result(Id.Identity1, Id.Identity5),

            Case(FilterOption.NotFlagged, FilterOption.Invalid).Result(Id.Identity7),
            Case(FilterOption.Valid, FilterOption.Invalid).ResultExcept(), // all results
            Case(FilterOption.Flagged, FilterOption.NotFlagged).ResultExcept(Id.Identity4, Id.Identity9), // all results
            Case(FilterOption.Answered, FilterOption.NotAnswered).ResultExcept(Id.Identity4, Id.Identity9), // all results
            Case(FilterOption.ForSupervisor, FilterOption.ForInterviewer).ResultExcept(Id.Identity4, Id.Identity9), // all results
        };

        [Test]
        public void ShouldApplyProperSkipPatterns()
        {
            var search = Subject.Search(interview, new[] { FilterOption.NotFlagged }, 2, 3);

            Assert.That(search.TotalCount, Is.EqualTo(6)); //4,7,8
            Assert.That(search.Results, Has.Count.EqualTo(2));

            AssertSearchReturnFollowingIds(search, Create.Identity(Id.g6, new []{1}), Id.Identity7, Id.Identity8);
        }

        [TestCase(1, 2)]
        [TestCase(2, 8)]
        [TestCase(3, 5)]
        [TestCase(4, 5)]
        [TestCase(5, 5)]
        [TestCase(7, 5)]
        public void ShouldReturnConsistentSearchResultIds(long skip, long take)
        {
            var search = Subject.Search(interview, Array.Empty<FilterOption>(), skip, take);
            var map = new Dictionary<int, string>
            {
                {0, Id.IdentityA.ToString()},
                {1, Create.Identity(Id.gD, new []{1}).ToString()},
                {2, Id.IdentityA.ToString()},
                {3, Id.IdentityB.ToString()}
            };

            foreach (var result in search.Results)
            {
                Assert.That(map[result.Id], Is.EqualTo(result.SectionId));
            }
        }

        private static FilterTestCase Case(params FilterOption[] options)
        {
            var allQuestions
                = //Enumerable.Range(1, 10).Select(id => Create.Entity.Identity(id, RosterVector.Empty)).ToArray();
                new[]
                {
                    Id.Identity1, Id.Identity2,
                    Id.Identity3, Create.Entity.Identity(Id.g6, new []{ 1 }), Id.Identity4,
                    Id.Identity5, 
                    Id.Identity7, Id.Identity8,
                    Id.Identity9, Id.Identity10,
                };
            return new FilterTestCase(allQuestions, options);
        }

        public class FilterTestCase
        {
            private readonly Identity[] allAnswers;

            public FilterTestCase(Identity[] allAnswers, FilterOption[] options)
            {
                this.allAnswers = allAnswers;
                Options.AddRange(options);
            }

            public List<FilterOption> Options { get; set; } = new List<FilterOption>();
            public List<Identity> Results { get; set; } = new List<Identity>();
            public long Skip { get; set; }
            public long Take { get; set; }

            public override string ToString()
            {
                return string.Join(", ", Options);
            }

            public FilterTestCase WithOptions(params FilterOption[] options)
            {
                this.Options.AddRange(options);
                return this;
            }

            public FilterTestCase Result(params Identity[] ids)
            {
                this.Results.AddRange(ids);
                return this;
            }

            public FilterTestCase ResultExcept(params Identity[] ids)
            {
                this.Results.AddRange(allAnswers.Except(ids));
                return this;
            }
        }

        private void AssertSearchReturnFollowingIds(SearchResults search, params Identity[] ids)
        {
            var result = search.Results.SelectMany(r => r.Questions).Select(q => q.Target).ToArray();
            var expect = ids.Select(id => id.ToString()).ToArray();
            Console.WriteLine("Result: " + string.Join(", ", result));
            Console.WriteLine("Expect: " + string.Join(", ", expect));
            Assert.That(result.ToArray(), Is.EqualTo(expect));
        }
    }
}

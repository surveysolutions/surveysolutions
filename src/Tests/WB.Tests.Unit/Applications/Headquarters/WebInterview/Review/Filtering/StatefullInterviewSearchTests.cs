using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.API.WebInterview.Services;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Filtering
{
    public class StatefullInterviewSearchTests
    {
        StatefulInterview interview;

        [NUnit.Framework.OneTimeSetUp]
        public void Setup()
        {
            var document = Create.Entity.QuestionnaireDocument(Guid.NewGuid(),
                Create.Entity.Group(Id.gA, "Group A", children: new IComposite[]
                {
                    Create.Entity.Variable(), // should not count
                    Create.Entity.TextQuestion(Id.g1, text: "Text 1 Flagged", variable: "text1"),
                    Create.Entity.TextQuestion(Id.g2, text: "Text Super A", variable: "text2", scope: Main.Core.Entities.SubEntities.QuestionScope.Supervisor),
                    Create.Entity.TextQuestion(Id.g3, text: "Text 3 With Comment", variable: "text3"),
                    Create.Entity.TextQuestion(Id.g4, text: "Text 4 Invalid", variable: "text4"),
                    Create.Entity.TextQuestion(Id.g5, text: "Text 5 With Comment Flagged Answered", variable: "text5")
                }),
                Create.Entity.Group(Id.gB, "Group B", children: new IComposite[]
                {
                    Create.Entity.TextQuestion(Id.g7, text: "Text 7 Answered Invalid", variable: "text6"),
                    Create.Entity.TextQuestion(Id.g8, text: "Text Super B", variable: "text7", scope: Main.Core.Entities.SubEntities.QuestionScope.Supervisor),
                    Create.Entity.TextQuestion(Id.g9, text: "Text 9", variable: "text8"),
                    Create.Entity.TextQuestion(Id.g10, text: "Text 10", variable: "text9")
                }));

            interview = Create.AggregateRoot.StatefulInterview(Guid.NewGuid(), questionnaire: document);

            interview.Apply(Create.Event.AnswersDeclaredInvalid(Create.Identity(Id.g4), Create.Identity(Id.g7)));
            interview.Apply(Create.Event.TextQuestionAnswered(Id.g5));
            interview.Apply(Create.Event.TextQuestionAnswered(Id.g7));
            interview.Apply(Create.Event.AnswerCommented(Id.g3));
            interview.Apply(Create.Event.AnswerCommented(Id.g5));

            var interviewFactory = Mock.Of<IInterviewFactory>(f => f.GetFlaggedQuestionIds(interview.Id) == new[]
            {
                Create.Identity(Id.g1),
                Create.Identity(Id.g5)
            });

            Subject = new StatefullInterviewSearcher(interviewFactory);
        }

        public StatefullInterviewSearcher Subject { get; set; }

        [TestCaseSource(nameof(SingleFilteringCaseData))]
        public void ShouldFilterBySingleOption(FilterTestCase @case)
        {
            var search = Subject.Search(interview, @case.Options.ToArray(), 0, 100);

            AssertSearchReturnFollowingIds(search, @case.Results.Select(id => Create.Entity.Identity(id)).ToArray());
        }
        
        public static FilterTestCase[] SingleFilteringCaseData =
        {
            Case(FilterOption.Flagged).Result(Id.g1, Id.g5),
            Case(FilterOption.NotFlagged).ResultExcept(Id.g1, Id.g5),

            Case(FilterOption.Answered).Result(Id.g5, Id.g7),
            Case(FilterOption.NotAnswered).ResultExcept(Id.g5, Id.g7),

            Case(FilterOption.Invalid).Result(Id.g4, Id.g7),
            Case(FilterOption.Valid).ResultExcept(Id.g4, Id.g7),

            Case(FilterOption.ForSupervisor).Result(Id.g2, Id.g8),
            Case(FilterOption.ForInterviewer).ResultExcept(Id.g2, Id.g8),

            Case(FilterOption.WithComments).Result(Id.g3, Id.g5)
        };

        [TestCaseSource(nameof(MultipleFilteringCaseData))]
        public void ShouldFilterByMultipleOption(FilterTestCase @case)
        {
            var search = Subject.Search(interview, @case.Options.ToArray(), 0, 100);
            AssertSearchReturnFollowingIds(search, @case.Results.Select(id => Create.Entity.Identity(id)).ToArray());
        }

        public static FilterTestCase[] MultipleFilteringCaseData =
        {
            Case(FilterOption.Flagged, FilterOption.WithComments).Result(Id.g5),
            Case(FilterOption.Flagged, FilterOption.NotAnswered).Result(Id.g1),

            Case(FilterOption.Flagged, FilterOption.Valid).Result(Id.g1, Id.g5),

            Case(FilterOption.NotFlagged, FilterOption.Invalid).Result(Id.g4, Id.g7),

            Case(FilterOption.Flagged, FilterOption.NotFlagged).ResultExcept(), // all results
            Case(FilterOption.Answered, FilterOption.NotAnswered).ResultExcept(), // all results
            Case(FilterOption.ForSupervisor, FilterOption.ForInterviewer).ResultExcept(), // all results
        };

        [Test]
        public void ShouldApplyProperSkipPatterns()
        {
            var search = Subject.Search(interview, new [] {FilterOption.NotFlagged}, 2, 3);

            Assert.That(search.TotalCount, Is.EqualTo(7)); //4,7,8
            Assert.That(search.Results, Has.Count.EqualTo(2));

            AssertSearchReturnFollowingIds(search, Id.g4, Id.g7, Id.g8);
        }

        private static FilterTestCase Case(params FilterOption[] options)
        {
            Guid[] allQuestions = { Id.g1, Id.g2, Id.g3, Id.g4, Id.g5, Id.g7, Id.g8, Id.g9, Id.g10 };
            return new FilterTestCase(allQuestions, options);
        }

        public class FilterTestCase
        {
            private readonly Guid[] allAnswers;

            public FilterTestCase(Guid[] allAnswers, FilterOption[] options)
            {
                this.allAnswers = allAnswers;
                Options.AddRange(options);
            }

            public List<FilterOption> Options { get; set; } = new List<FilterOption>();
            public List<Guid> Results { get; set; } = new List<Guid>();
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

            public FilterTestCase Result(params Guid[] ids)
            {
                this.Results.AddRange(ids);
                return this;
            }

            public FilterTestCase ResultExcept(params Guid[] ids)
            {
                this.Results.AddRange(allAnswers.Except(ids));
                return this;
            }
        }
        
        private void AssertSearchReturnFollowingIds(SearchResults search, params Guid[] ids)
        {
            AssertSearchReturnFollowingIds(search, ids.Select(id => Create.Entity.Identity(id)).ToArray());
        }

        private void AssertSearchReturnFollowingIds(SearchResults search, params Identity[] ids)
        {
            var result = search.Results.SelectMany(r => r.Questions).Select(q => q.Target);
            var expect = ids.Select(id => id.ToString()).ToArray();
            Assert.That(result.ToArray(), Is.EqualTo(expect));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.API.WebInterview.Services;
using WB.UI.Headquarters.Services.Impl;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Filtering
{
    public partial class StatefullInterviewSearchTests
    {
        StatefulInterview interview;
        IQuestionnaire questionnaire;

        private static readonly Identity TextFlagged = Id.Identity1;
        private static readonly Identity TextSupervisor = Id.Identity2;
        private static readonly Identity TextComment = Id.Identity3;
        private static readonly Identity TextInRosterFlagged = Create.Identity(Id.g6, 1);
        private static readonly Identity StaticTextInvalid = Id.Identity4;
        private static readonly Identity TextCommentFlaggedAnswered = Id.Identity5;
        private static readonly Identity TextAnsweredInvalid = Id.Identity7;
        private static readonly Identity TextSuper = Id.Identity8;
        private static readonly Identity StaticText = Id.Identity9;
        private static readonly Identity Text = Id.Identity10;


        [OneTimeSetUp]
        public void Setup()
        {
            var document = Create.Entity.QuestionnaireDocument(Guid.NewGuid(), null,
                Create.Entity.Group(Id.IdentityA.Id, "Group A", children: new IComposite[]
                {
                    Create.Entity.Variable(), // should not count
                    Create.Entity.TextQuestion(TextFlagged.Id, text: "Text 1 Flagged", variable: "text1"),
                    Create.Entity.TextQuestion(TextSupervisor.Id, text: "Text Super A", variable: "text2", scope: Main.Core.Entities.SubEntities.QuestionScope.Supervisor),
                    Create.Entity.TextQuestion(TextComment.Id, text: "Text 3 With Comment", variable: "text3"),
                    Create.Entity.FixedRoster(Id.IdentityD.Id, title: "roster", children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(TextInRosterFlagged.Id, text: "Text Roster Flagged", variable: "textInRoster"),
                    }, fixedTitles: new [] { Create.Entity.FixedTitle(1, "Test") }),
                    Create.Entity.StaticText(StaticTextInvalid.Id, "StaticText 4 Invalid"),
                    Create.Entity.TextQuestion(TextCommentFlaggedAnswered.Id, text: "Text 5 With Comment Flagged Answered", variable: "text5"),
                }),
                Create.Entity.Group(Id.gB, "Group B", children: new IComposite[]
                {
                    Create.Entity.TextQuestion(TextAnsweredInvalid.Id, text: "Text 7 Answered Invalid", variable: "text6"),
                    Create.Entity.TextQuestion(TextSuper.Id, text: "Text Super B", variable: "text7", scope: Main.Core.Entities.SubEntities.QuestionScope.Supervisor),
                    Create.Entity.StaticText(StaticText.Id, "StaticText 9"),
                    Create.Entity.TextQuestion(Text.Id, text: "Text 10", variable: "text9")
                }));
            questionnaire = Create.Entity.PlainQuestionnaire(document);
            interview = Create.AggregateRoot.StatefulInterview(Guid.NewGuid(), questionnaire: document);

            interview.Apply(Create.Event.AnswersDeclaredInvalid(TextAnsweredInvalid));
            interview.Apply(Create.Event.StaticTextsDeclaredInvalid(StaticTextInvalid));
            interview.Apply(Create.Event.TextQuestionAnswered(TextCommentFlaggedAnswered.Id));
            interview.Apply(Create.Event.TextQuestionAnswered(TextAnsweredInvalid.Id));
            interview.Apply(Create.Event.AnswerCommented(TextComment.Id));
            interview.Apply(Create.Event.AnswerCommented(TextCommentFlaggedAnswered.Id));

            var interviewFactory = Mock.Of<IInterviewFactory>(f => f.GetFlaggedQuestionIds(interview.Id) == new[]
            {
                TextFlagged,
                TextCommentFlaggedAnswered,
                TextInRosterFlagged
            });

            Subject = new StatefulInterviewSearcher(interviewFactory);
        }

        private static readonly Identity[] AllQuestions = {
            TextFlagged,
            TextSupervisor,
            TextComment,
            TextInRosterFlagged,
            StaticTextInvalid,
            TextCommentFlaggedAnswered,
            TextAnsweredInvalid, TextSuper,
            StaticText, Text,
        };

        private static Identity[] AllQuestionsBut(params Identity[] ids) => AllQuestions.Except(ids).ToArray();

        public StatefulInterviewSearcher Subject { get; set; }

        [TestCaseSource(nameof(TestCaseData))]
        public void ShouldFilterResults(FilterTestCase @case)
        {
            var search = Subject.Search(interview, questionnaire, @case.Options.ToArray(), 0, 100);
            AssertSearchReturnFollowingIds(search, @case.Results.Select(id => id).ToArray());

            if (@case.StatsCounter != null)
            {
                var resultedStats = AllFilterOptions.Select(option => search.Stats[option]).ToList();

                Assert.That(resultedStats, Is.EqualTo(@case.StatsCounter), 
                    "\r\nExpected: " + string.Join(", ", @case.StatsCounter) + "\r\n" +
                    "Actual  : " + string.Join(", ", resultedStats));
            }
        }

        public static FilterTestCase[] TestCaseData =
        {
            new FilterTestCase(FilterOption.Flagged)
                .ExpectedQuestions(TextFlagged, TextInRosterFlagged, TextCommentFlaggedAnswered),
            new FilterTestCase(FilterOption.NotFlagged)
                .ExpectedQuestions(AllQuestionsBut(TextFlagged, TextInRosterFlagged, TextCommentFlaggedAnswered, StaticTextInvalid, StaticText)),
            new FilterTestCase(FilterOption.Answered)
                .ExpectedQuestions(TextCommentFlaggedAnswered, TextAnsweredInvalid),
            new FilterTestCase(FilterOption.NotAnswered)
                .ExpectedQuestions(AllQuestionsBut(TextCommentFlaggedAnswered, TextAnsweredInvalid, StaticTextInvalid, StaticText)),
            new FilterTestCase(FilterOption.Invalid)
                .ExpectedQuestions(StaticTextInvalid, TextAnsweredInvalid),
            new FilterTestCase(FilterOption.Valid)
                .ExpectedQuestions(AllQuestionsBut(StaticTextInvalid,TextAnsweredInvalid)),
            new FilterTestCase(FilterOption.ForSupervisor).ExpectedQuestions(TextSupervisor, TextSuper),
            new FilterTestCase(FilterOption.ForInterviewer)
                .ExpectedQuestions(AllQuestionsBut(TextSupervisor, TextSuper, StaticTextInvalid, StaticText)),
            new FilterTestCase(FilterOption.WithComments).ExpectedQuestions(TextComment, TextCommentFlaggedAnswered),
            new FilterTestCase(FilterOption.Flagged, FilterOption.WithComments)
                .ExpectedQuestions(TextCommentFlaggedAnswered)
                .ExpectedStats(1, 0, 1, 0, 1, 1, 0, 0, 1),
            new FilterTestCase(FilterOption.Flagged, FilterOption.NotAnswered)
                .ExpectedQuestions(TextFlagged, TextInRosterFlagged)
                .ExpectedStats(2, 0, 0, 0, 2, 0, 2, 0, 2),
            new FilterTestCase(FilterOption.Flagged, FilterOption.Valid)
                .ExpectedQuestions(TextFlagged, TextInRosterFlagged, TextCommentFlaggedAnswered)
                .ExpectedStats(3, 0, 1, 0, 3, 1, 2, 0, 3),
            new FilterTestCase(FilterOption.NotFlagged, FilterOption.Invalid)
                .ExpectedQuestions(TextAnsweredInvalid)
                .ExpectedStats(0, 1, 0, 1, 0, 1, 0, 0, 1),
            new FilterTestCase(FilterOption.Valid, FilterOption.Invalid)
                .ExpectedQuestions(AllQuestions)
                .ExpectedStats(3, 5, 2, 2, 8, 2, 6, 2, 6),
            new FilterTestCase(FilterOption.Flagged, FilterOption.NotFlagged)
                .ExpectedQuestions(AllQuestionsBut(StaticTextInvalid, StaticText))
                .ExpectedStats(3, 5, 2, 1, 7, 2, 6, 2, 6),
            new FilterTestCase(FilterOption.Answered, FilterOption.NotAnswered)
                .ExpectedQuestions(AllQuestionsBut(StaticTextInvalid, StaticText))
                .ExpectedStats(3, 5, 2, 1, 7, 2, 6, 2, 6),
            new FilterTestCase(FilterOption.ForSupervisor, FilterOption.ForInterviewer)
                .ExpectedQuestions(AllQuestionsBut(StaticTextInvalid, StaticText))
                .ExpectedStats(3, 5, 2, 1, 7, 2, 6, 2, 6)
        };

        [TestCase(1, 2)]
        [TestCase(2, 8)]
        [TestCase(3, 5)]
        [TestCase(4, 5)]
        [TestCase(5, 5)]
        [TestCase(7, 5)]
        public void ShouldReturnConsistentSearchResultIds(int skip, int take)
        {
            var search = Subject.Search(interview, questionnaire, Array.Empty<FilterOption>(), skip, take);

            // no matter of what skip/take values provided result id should always point to same sectionId
            var map = new Dictionary<int, string>
            {
                {0, Id.IdentityA.ToString()},
                {1, Create.Identity(Id.gD, 1).ToString()},
                {2, Id.IdentityA.ToString()},
                {3, Id.IdentityB.ToString()}
            };

            foreach (var result in search.Results)
            {
                Assert.That(map[result.Id], Is.EqualTo(result.SectionId));
            }
        }

        private void AssertSearchReturnFollowingIds(SearchResults search, params Identity[] ids)
        {
            var result = search.Results.SelectMany(r => r.Questions).Select(q => q.Target).ToArray();
            var expect = ids.Select(id => id.ToString()).ToArray();
            var message = "Result: " + string.Join(", ", result) + "\r\n" + "Expect: " + string.Join(", ", expect);
            Assert.That(result.ToArray(), Is.EqualTo(expect), message);
        }

        private static readonly FilterOption[] AllFilterOptions =
            Enum.GetValues(typeof(FilterOption)).Cast<FilterOption>().ToArray();
    }
}

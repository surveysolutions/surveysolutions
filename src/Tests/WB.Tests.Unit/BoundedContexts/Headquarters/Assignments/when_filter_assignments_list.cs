using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.DenormalizerStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    [TestOf(typeof(AssignmentViewFactory))]
    public class when_filter_assignments_list
    {
        [Test]
        public void should_by_default_return_first_page_of_size_20()
        {
            var fixture = NewFixture();

            var sut = fixture.Create<AssignmentViewFactory>();

            var result = sut.Load(new AssignmentsInputModel()
            {
                Offset = 0,
                Limit = 20,
            });

            Assert.That(result.Page, Is.EqualTo(0));
            Assert.That(result.PageSize, Is.EqualTo(20));
        }

        [Test]
        public void should_filter_by_questionnaire()
        {
            var fixture = NewFixture();

            fixture.Register<IQueryableReadSideRepositoryReader<Assignment, Guid>>(() => new InMemoryReadSideRepositoryAccessor<Assignment, Guid>(
                new Dictionary<Guid, Assignment>
                {
                    { Id.g1, Create.Entity.Assignment(1, Create.Entity.QuestionnaireIdentity(Id.gA, 1))},
                    { Id.g2, Create.Entity.Assignment(2, Create.Entity.QuestionnaireIdentity(Id.gB, 1))},
                    { Id.g3, Create.Entity.Assignment(3, Create.Entity.QuestionnaireIdentity(Id.gA, 2), 
                            updatedAt: DateTime.UtcNow.AddDays(1)) }
                }
            ));

            var sut = fixture.Create<AssignmentViewFactory>();

            var result = sut.Load(new AssignmentsInputModel
            {
                Offset = 0,
                Limit = 20,

                QuestionnaireId = Id.gA
            });

            Assert.That(result.TotalCount, Is.EqualTo(2));
            Assert.That(result.Items.First().Id, Is.EqualTo(3), "Sorted by default by UpdatedDate desc");
        }

        [Test]
        public void should_be_able_to_sort_by_questionnaire_title()
        {
            var fixture = NewFixture();

            fixture.Register<IQueryableReadSideRepositoryReader<Assignment, Guid>>(() => new InMemoryReadSideRepositoryAccessor<Assignment, Guid>(
                new Dictionary<Guid, Assignment>
                {
                    { Id.g1, Create.Entity.Assignment(1, Create.Entity.QuestionnaireIdentity(Id.gA, 1), questionnaireTitle: "Aaaaa")},
                    { Id.g2, Create.Entity.Assignment(2, Create.Entity.QuestionnaireIdentity(Id.gB, 1), questionnaireTitle: "CCCCCC")},
                    { Id.g3, Create.Entity.Assignment(3, Create.Entity.QuestionnaireIdentity(Id.gA, 2), questionnaireTitle: "Asterix")}
                }
            ));

            var sut = fixture.Create<AssignmentViewFactory>();

            var result = sut.Load(new AssignmentsInputModel
            {
                Offset = 0,
                Limit = 20,

                Orders = new []
                {
                    new OrderRequestItem { Direction = OrderDirection.Desc, Field = "QuestionnaireTitle" } 
                }
            });

            Assert.That(result.Items.First(), Has.Property(nameof(AssignmentRow.Id)).EqualTo(2));
        }

        [Test]
        public void should_be_able_to_search_by_received_by_tablet_flag()
        {
            var fixture = NewFixture();

            var assignment = Create.Entity.Assignment(1);
            assignment.ReceivedByTabletAtUtc = DateTime.UtcNow;

            fixture.Register<IQueryableReadSideRepositoryReader<Assignment, Guid>>(() => 
                new InMemoryReadSideRepositoryAccessor<Assignment, Guid>(
                    new Dictionary<Guid, Assignment>
                    {
                        {Id.g1, assignment},
                        {Id.g2, Create.Entity.Assignment(2)}
                    }
            ));

            var sut = fixture.Create<AssignmentViewFactory>();

            var result = sut.Load(new AssignmentsInputModel
            {
                ReceivedByTablet = AssignmentReceivedState.Received,
                Offset = 0,
                Limit = 20,
            });

            Assert.That(result.Items.First(), Has.Property(nameof(AssignmentRow.Id)).EqualTo(1));
        }

        [Test]
        public void should_search_identifying_questions_using_starts_with()
        {
            var fixture = NewFixture();

            var assignment1 = Create.Entity.Assignment(1, identifyingAnswers: new List<IdentifyingAnswer>
            {
                Create.Entity.IdentifyingAnswer(variable: "name", answerAsString: "John Smith")
            });
            var assignment2 = Create.Entity.Assignment(2, identifyingAnswers: new List<IdentifyingAnswer>
            {
                Create.Entity.IdentifyingAnswer(variable: "name", answerAsString: "Smith John")
            });

            fixture.Register<IQueryableReadSideRepositoryReader<Assignment, Guid>>(() =>
                new InMemoryReadSideRepositoryAccessor<Assignment, Guid>(
                    new Dictionary<Guid, Assignment>
                    {
                        { Id.g1, assignment1 },
                        { Id.g2, assignment2 },
                    }
                ));

            var sut = fixture.Create<AssignmentViewFactory>();

            var result = sut.Load(new AssignmentsInputModel
            {
                SearchBy = "John",
                SearchByFields = AssignmentsInputModel.SearchTypes.IdentifyingQuestions,
                Offset = 0,
                Limit = 20,
            });

            // StartsWith("john") should only match "John Smith", not "Smith John"
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items.First().Id, Is.EqualTo(1));
        }

        [Test]
        public void should_filter_by_condition_starts_with()
        {
            var fixture = NewFixture();

            var assignment1 = Create.Entity.Assignment(1, identifyingAnswers: new List<IdentifyingAnswer>
            {
                Create.Entity.IdentifyingAnswer(variable: "region", answerAsString: "North East")
            });
            var assignment2 = Create.Entity.Assignment(2, identifyingAnswers: new List<IdentifyingAnswer>
            {
                Create.Entity.IdentifyingAnswer(variable: "region", answerAsString: "South West")
            });

            fixture.Register<IQueryableReadSideRepositoryReader<Assignment, Guid>>(() =>
                new InMemoryReadSideRepositoryAccessor<Assignment, Guid>(
                    new Dictionary<Guid, Assignment>
                    {
                        { Id.g1, assignment1 },
                        { Id.g2, assignment2 },
                    }
                ));

            var sut = fixture.Create<AssignmentViewFactory>();

            var result = sut.Load(new AssignmentsInputModel
            {
                Conditions = new[]
                {
                    new AssignmentFilterCondition { Variable = "region", Field = "valueLowerCase|startsWith", Value = "north" }
                },
                Offset = 0,
                Limit = 20,
            });

            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items.First().Id, Is.EqualTo(1));
        }

        [Test]
        public void should_filter_by_condition_equals()
        {
            var fixture = NewFixture();

            var assignment1 = Create.Entity.Assignment(1, identifyingAnswers: new List<IdentifyingAnswer>
            {
                Create.Entity.IdentifyingAnswer(variable: "city", answerAsString: "Paris")
            });
            var assignment2 = Create.Entity.Assignment(2, identifyingAnswers: new List<IdentifyingAnswer>
            {
                Create.Entity.IdentifyingAnswer(variable: "city", answerAsString: "London")
            });

            fixture.Register<IQueryableReadSideRepositoryReader<Assignment, Guid>>(() =>
                new InMemoryReadSideRepositoryAccessor<Assignment, Guid>(
                    new Dictionary<Guid, Assignment>
                    {
                        { Id.g1, assignment1 },
                        { Id.g2, assignment2 },
                    }
                ));

            var sut = fixture.Create<AssignmentViewFactory>();

            var result = sut.Load(new AssignmentsInputModel
            {
                Conditions = new[]
                {
                    new AssignmentFilterCondition { Variable = "city", Field = "valueLowerCase|eq", Value = "paris" }
                },
                Offset = 0,
                Limit = 20,
            });

            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items.First().Id, Is.EqualTo(1));
        }

        [Test]
        public void should_filter_by_answer_code_not_equals()
        {
            var fixture = NewFixture();

            var assignment1 = Create.Entity.Assignment(1, identifyingAnswers: new List<IdentifyingAnswer>
            {
                Create.Entity.IdentifyingAnswer(variable: "gender", answer: "1", answerAsString: "Male")
            });
            var assignment2 = Create.Entity.Assignment(2, identifyingAnswers: new List<IdentifyingAnswer>
            {
                Create.Entity.IdentifyingAnswer(variable: "gender", answer: "2", answerAsString: "Female")
            });

            fixture.Register<IQueryableReadSideRepositoryReader<Assignment, Guid>>(() =>
                new InMemoryReadSideRepositoryAccessor<Assignment, Guid>(
                    new Dictionary<Guid, Assignment>
                    {
                        { Id.g1, assignment1 },
                        { Id.g2, assignment2 },
                    }
                ));

            var sut = fixture.Create<AssignmentViewFactory>();

            var result = sut.Load(new AssignmentsInputModel
            {
                Conditions = new[]
                {
                    new AssignmentFilterCondition { Variable = "gender", Field = "answerCode|neq", Value = "1" }
                },
                Offset = 0,
                Limit = 20,
            });

            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items.First().Id, Is.EqualTo(2));
        }

        [Test]
        public void should_filter_by_multiple_conditions()
        {
            var fixture = NewFixture();

            var assignment1 = Create.Entity.Assignment(1, identifyingAnswers: new List<IdentifyingAnswer>
            {
                Create.Entity.IdentifyingAnswer(variable: "region", answerAsString: "North"),
                Create.Entity.IdentifyingAnswer(variable: "city", answerAsString: "Oslo")
            });
            var assignment2 = Create.Entity.Assignment(2, identifyingAnswers: new List<IdentifyingAnswer>
            {
                Create.Entity.IdentifyingAnswer(variable: "region", answerAsString: "North"),
                Create.Entity.IdentifyingAnswer(variable: "city", answerAsString: "Bergen")
            });
            var assignment3 = Create.Entity.Assignment(3, identifyingAnswers: new List<IdentifyingAnswer>
            {
                Create.Entity.IdentifyingAnswer(variable: "region", answerAsString: "South"),
                Create.Entity.IdentifyingAnswer(variable: "city", answerAsString: "Oslo")
            });

            fixture.Register<IQueryableReadSideRepositoryReader<Assignment, Guid>>(() =>
                new InMemoryReadSideRepositoryAccessor<Assignment, Guid>(
                    new Dictionary<Guid, Assignment>
                    {
                        { Id.g1, assignment1 },
                        { Id.g2, assignment2 },
                        { Id.g3, assignment3 },
                    }
                ));

            var sut = fixture.Create<AssignmentViewFactory>();

            var result = sut.Load(new AssignmentsInputModel
            {
                Conditions = new[]
                {
                    new AssignmentFilterCondition { Variable = "region", Field = "valueLowerCase|eq", Value = "north" },
                    new AssignmentFilterCondition { Variable = "city", Field = "valueLowerCase|eq", Value = "oslo" }
                },
                Offset = 0,
                Limit = 20,
            });

            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items.First().Id, Is.EqualTo(1));
        }

        IFixture NewFixture()
        {
            var autoFixture = Create.Other.AutoFixture();
            var webInterviewConfigProvider = new Mock<IWebInterviewConfigProvider>();
            webInterviewConfigProvider.Setup(x => x.Get(It.IsAny<QuestionnaireIdentity>()))
                .Returns((QuestionnaireIdentity arg) => new WebInterviewConfig
                {
                    QuestionnaireId = arg
                });
            
            autoFixture.Register(() => webInterviewConfigProvider.Object);

            // Return null questionnaire so GetIdentifyingColumnText returns early
            // (test assertions focus on filtering, not on column text rendering)
            var questionnaireStorage = new Mock<IQuestionnaireStorage>();
            questionnaireStorage.Setup(x => x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()))
                .Returns((IQuestionnaire)null);
            autoFixture.Register(() => questionnaireStorage.Object);

            return autoFixture;
        }
    }
}

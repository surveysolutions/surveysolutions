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
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
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
            return autoFixture;
        }
    }
}

using System;
using FluentAssertions.Common;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Tests.Abc;

using WB.UI.Headquarters.API.WebInterview.Services;
using WB.UI.Headquarters.Services.Impl;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Api
{
    internal class when_getting_entity_details
    {
        private Mock<IAuthorizedUser> user;
        private HqWebInterviewInterviewEntityFactory service;
        private QuestionnaireDocument document;
        private StatefulInterview interview;
        private readonly Guid supervisorId = Id.gA;
        private readonly Guid interviewerId = Id.gB;
        private readonly Guid headquartersId = Id.gC;
        private readonly Guid commentedQuestionId = Id.g1;

        [SetUp]
        public void Setup()
        {
            user = new Mock<IAuthorizedUser>();
            service = Web.Create.Service.HqWebInterviewInterviewEntityFactory(user.Object);
            document = GetDocument();
            interview = Create.AggregateRoot.StatefulInterview(questionnaire: document, supervisorId: supervisorId, userId: interviewerId);
            interview.CommentAnswer(interviewerId, commentedQuestionId, RosterVector.Empty, DateTimeOffset.UtcNow, "test");
        }

        [Test]
        public void should_allow_resolving_comments_for_HQ()
        {
            user.Setup(x => x.IsHeadquarter).Returns(true);
           
            // Act
            var entity = service.GetEntityDetails(commentedQuestionId.FormatGuid(), interview, Create.Entity.PlainQuestionnaire(document), true);

            // Assert
            Assert.That(entity, Has.Property(nameof(InterviewTextQuestion.AllowResolveComments)).True);
        }

        [Test]
        public void should_allow_resolving_comments_for_Admin()
        {
            user.Setup(x => x.IsAdministrator).Returns(true);
           
            // Act
            var entity = service.GetEntityDetails(commentedQuestionId.FormatGuid(), interview, Create.Entity.PlainQuestionnaire(document), true);

            // Assert
            Assert.That(entity, Has.Property(nameof(InterviewTextQuestion.AllowResolveComments)).True);
        }

        [Test]
        public void should_allow_resolving_comments_for_Supervisor_when_no_HQ_comments_made()
        {
            user.Setup(x => x.IsSupervisor).Returns(true);
           
            // Act
            var entity = service.GetEntityDetails(commentedQuestionId.FormatGuid(), interview, Create.Entity.PlainQuestionnaire(document), true);

            // Assert
            Assert.That(entity, Has.Property(nameof(InterviewTextQuestion.AllowResolveComments)).True);
        }

        [Test]
        public void should_not_allow_resolving_comments_for_supervisor_when_HQ_comments_made()
        {
            user.Setup(x => x.IsSupervisor).Returns(true);
            interview.CommentAnswer(headquartersId, commentedQuestionId, RosterVector.Empty, DateTimeOffset.UtcNow,  "test1");
           
            // Act
            var entity = service.GetEntityDetails(commentedQuestionId.FormatGuid(), interview, Create.Entity.PlainQuestionnaire(document), true);

            // Assert
            Assert.That(entity, Has.Property(nameof(InterviewTextQuestion.AllowResolveComments)).False);
        }

        protected QuestionnaireDocument GetDocument()
        {
            return Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(commentedQuestionId)
            );
        }
    }
}

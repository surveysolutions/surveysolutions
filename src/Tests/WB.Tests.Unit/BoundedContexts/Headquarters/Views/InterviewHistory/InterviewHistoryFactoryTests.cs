using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Tests.Abc;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Views.InterviewHistory
{
    [TestOf(typeof(InterviewHistoryFactory))]
    public class InterviewHistoryFactoryTests
    {
        [Test]
        public void when_loading_interviews_history()
        {
            var interviewId1 = Id.g1;
            var interviewId2 = Id.g2;

            var userId = Id.g10;
            var superId = Id.g9;
            var questionnaireId = Id.g6;
            
            var date = DateTime.Now;
            
            var eventStore = new Mock<IEventStore>();
            eventStore.Setup(x => x.Read(interviewId1, 0)).Returns(new []
            {
                new CommittedEvent(Guid.NewGuid(), "", Guid.NewGuid(), interviewId1, 1, date, 0, 
                    new InterviewCreated(userId, questionnaireId, 1,1,false,date)
                    ),
                new CommittedEvent(Guid.NewGuid(), "", Guid.NewGuid(), interviewId1, 2, date, 0, 
                    new SupervisorAssigned(userId, superId, date)
                ),
                new CommittedEvent(Guid.NewGuid(), "", Guid.NewGuid(), interviewId1, 3, date, 0, 
                    new InterviewerAssigned(userId, userId, date)
                ),
                new CommittedEvent(Guid.NewGuid(), "", Guid.NewGuid(), interviewId1, 4, date, 0, 
                    new InterviewCompleted(userId, date,"test")
                ),
                
            });
            eventStore.Setup(x => x.Read(interviewId2, 0)).Returns(
                new []
                {
                    new CommittedEvent(Guid.NewGuid(), "", Guid.NewGuid(), interviewId2, 1, DateTime.Now, 0, 
                        new InterviewCreated(userId, questionnaireId, 1,1,false,DateTime.Now)
                    ),
                    new CommittedEvent(Guid.NewGuid(), "", Guid.NewGuid(), interviewId2, 2, date, 0, 
                        new SupervisorAssigned(userId, superId, date)
                    ),
                    new CommittedEvent(Guid.NewGuid(), "", Guid.NewGuid(), interviewId2, 3, date, 0, 
                        new InterviewerAssigned(userId, userId, date)
                    ),
                });

            var interviewSummaries = new Mock<IReadSideRepositoryWriter<InterviewSummary>>();
            interviewSummaries.Setup(x => x.GetById(It.IsAny<string>())).Returns(new InterviewSummary()
            {
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = 1
            });
            
            
            var users = new Mock<IUserViewFactory>();
            users.Setup(x => x.GetUser(userId)).Returns(new UserViewLite()
            {
                UserName = "Api",
                Roles = {UserRoles.ApiUser}
            });
            users.Setup(x => x.GetUser(superId)).Returns(new UserViewLite()
            {
                UserName = "Super",
                Roles = {UserRoles.Supervisor}
            });

            var questionnaireExportStructureStorage = new Mock<IQuestionnaireExportStructureStorage>();
            questionnaireExportStructureStorage
                .Setup(x => x.GetQuestionnaireExportStructure(It.IsAny<QuestionnaireIdentity>()))
                .Returns(new QuestionnaireExportStructure());
            
            var interviewHistoryFactory = Create.Service.InterviewHistoryFactory(
                eventStore.Object,
                interviewSummaries.Object,
                users.Object,
                questionnaireExportStructureStorage.Object);

            var historyView = interviewHistoryFactory.Load(new[]{interviewId1, interviewId2}, false);
            
            Assert.That(historyView.Length, Is.EqualTo(2));
            Assert.That(historyView[0].Records.Count, Is.EqualTo(4));
            Assert.That(historyView[0].Records[2].Index, Is.EqualTo(2));
            
            Assert.That(historyView[0].Records[2].OriginatorRole, Is.EqualTo("ApiUser"));
            
            Assert.That(historyView[1].Records.Count, Is.EqualTo(3));
            Assert.That(historyView[1].Records[1].Index, Is.EqualTo(1));
        }
        
        [Test]
        public void when_need_return_interviews_reduced_history()
        {
            var interviewId = Id.g1;

            var userId = Id.g10;
            var superId = Id.g9;
            var questionnaireId = Id.g6;
            
            var date = DateTime.Now;
            
            var eventStore = new Mock<IEventStore>();
            eventStore.Setup(x => x.Read(interviewId, 0)).Returns(new []
            {
                new CommittedEvent(Guid.NewGuid(), "", Guid.NewGuid(), interviewId, 1, date, 0, 
                    new InterviewCreated(userId, questionnaireId, 1,1,false,date)
                    ),
                new CommittedEvent(Guid.NewGuid(), "", Guid.NewGuid(), interviewId, 2, date, 0, 
                    new SupervisorAssigned(userId, superId, date)
                ),
                new CommittedEvent(Guid.NewGuid(), "", Guid.NewGuid(), interviewId, 3, date, 0, 
                    new InterviewerAssigned(userId, userId, date)
                ),
                new CommittedEvent(Guid.NewGuid(), "", Guid.NewGuid(), interviewId, 4, date, 0, 
                    new GroupsDisabled([Identity.Create(Id.g1, RosterVector.Empty)], DateTimeOffset.Now)
                ),
                new CommittedEvent(Guid.NewGuid(), "", Guid.NewGuid(), interviewId, 5, date, 0, 
                    new QuestionsEnabled([Identity.Create(Id.g1, RosterVector.Empty)], DateTimeOffset.Now)
                ),
                new CommittedEvent(Guid.NewGuid(), "", Guid.NewGuid(), interviewId, 6, date, 0, 
                    new StaticTextsDisabled([Identity.Create(Id.g1, RosterVector.Empty)], DateTimeOffset.Now)
                ),
                new CommittedEvent(Guid.NewGuid(), "", Guid.NewGuid(), interviewId, 7, date, 0, 
                    new AnswersDeclaredInvalid(new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>()
                    {
                        {
                            Abc.Create.Identity(Id.g1),
                            new List<FailedValidationCondition>() {new FailedValidationCondition(0)}
                        }
                    }, DateTimeOffset.Now)
                ),
                new CommittedEvent(Guid.NewGuid(), "", Guid.NewGuid(), interviewId, 8, date, 0, 
                    new VariablesDisabled([Identity.Create(Id.g1, RosterVector.Empty)], DateTimeOffset.Now)
                ),
                new CommittedEvent(Guid.NewGuid(), "", Guid.NewGuid(), interviewId, 9, date, 0, 
                    new InterviewCompleted(userId, date,"test")
                ),
            });

            var interviewSummaries = new Mock<IReadSideRepositoryWriter<InterviewSummary>>();
            interviewSummaries.Setup(x => x.GetById(It.IsAny<string>())).Returns(new InterviewSummary()
            {
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = 1
            });
            
            
            var users = new Mock<IUserViewFactory>();
            users.Setup(x => x.GetUser(userId)).Returns(new UserViewLite()
            {
                UserName = "Api",
                Roles = {UserRoles.ApiUser}
            });

            var questionnaireExportStructureStorage = new Mock<IQuestionnaireExportStructureStorage>();
            questionnaireExportStructureStorage
                .Setup(x => x.GetQuestionnaireExportStructure(It.IsAny<QuestionnaireIdentity>()))
                .Returns(new QuestionnaireExportStructure());
            
            var interviewHistoryFactory = Create.Service.InterviewHistoryFactory(
                eventStore.Object,
                interviewSummaries.Object,
                users.Object,
                questionnaireExportStructureStorage.Object);

            var historyView = interviewHistoryFactory.Load([interviewId], true);
            
            Assert.That(historyView.Length, Is.EqualTo(1));
            Assert.That(historyView[0].Records.Count, Is.EqualTo(4));
            Assert.That(historyView[0].Records[2].Index, Is.EqualTo(2));
            Assert.That(historyView[0].Records[2].OriginatorRole, Is.EqualTo("ApiUser"));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Supervisor.Denormalizer;
using Core.Supervisor.DenormalizerStorageItem;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire.Completed;
using Main.Core.Utility;
using Main.Core.View.CompleteQuestionnaire;
using Main.DenormalizerStorage;
using Moq;
using NUnit.Framework;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Core.Supervisor.Tests
{
    [TestFixture]
    public class SummaryDenormalizerTests
    {
        [Test]
        public void Handle_When_New_assigment_event_is_arrived_summary_Store_is_empty_Then_created_record_with_Responsible_supervisor()
        {
            //arrange
            var questionnarieId = Guid.NewGuid();
            var supervisorId = Guid.NewGuid();
            var summaryStore = new InMemoryReadSideRepositoryAccessor<SummaryItem>();

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, questionnarieId, supervisorId);
         
            

            var newAssigmentEvent =
                CreatePublishedEvent(new QuestionnaireAssignmentChanged()
                    {
                        Responsible = new UserLight(supervisorId, "supervisor")
                    }, questionnarieId);
            //act

            target.Handle(newAssigmentEvent);

            //assert
            var queryForSupervisorItem = summaryStore.Query(_ => _.First(i => i.ResponsibleId == supervisorId));
            Assert.Null(queryForSupervisorItem.ResponsibleSupervisorId);

            Assert.That(queryForSupervisorItem.UnassignedCount, Is.EqualTo(1));
        }

        [Test]
        public void Handle_When_New_assigment_event_is_arrived_summary_Store_is_not_empty_Then_update_record_with_Responsible_interviewer()
        {
            //arrange
            var questionnarieId = Guid.NewGuid();
            var templateId = Guid.NewGuid();
            var supervisorId = Guid.NewGuid();
            var interviewerId = Guid.NewGuid();
            var itemId = supervisorId.Combine(templateId);
            var summaryStore = new InMemoryReadSideRepositoryAccessor<SummaryItem>();

            summaryStore.Store(new SummaryItem() {UnassignedCount = 1, ResponsibleId = supervisorId, QuestionnaireStatus = SurveyStatus.Unassign.PublicId}, itemId);

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, questionnarieId, supervisorId,
                                                                   interviewerId, templateId);



            var newAssigmentEvent =
                CreatePublishedEvent(new QuestionnaireAssignmentChanged()
                {
                    Responsible = new UserLight(interviewerId, "interviewer"), PreviousResponsible = new UserLight(supervisorId,"supervisor")
                }, questionnarieId);
            //act

            target.Handle(newAssigmentEvent);

            //assert


            var queryForInterviewerItem = summaryStore.Query(_ => _.First(i => i.ResponsibleId == interviewerId));

            Assert.That(queryForInterviewerItem.ResponsibleSupervisorId, Is.EqualTo(supervisorId));

            Assert.That(queryForInterviewerItem.UnassignedCount, Is.EqualTo(1));

            var queryForSupervisorItem = summaryStore.Query(_ => _.First(i => i.ResponsibleId == supervisorId));

            Assert.That(queryForSupervisorItem.UnassignedCount, Is.EqualTo(1));
            Assert.Null(queryForSupervisorItem.ResponsibleSupervisorId);
        }

        [Test]
        public void
            Handle_When_Staus_change_happend_Then_Counters_is_updated_for_interviewer_record_and_supervisor_record()
        {

            //arrange
            var questionnarieId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var templateId = Guid.Parse("21111111-1111-1111-1111-111111111111");
            var supervisorId = Guid.Parse("31111111-1111-1111-1111-111111111111");
            var interviewerId = Guid.Parse("41111111-1111-1111-1111-111111111111");
            var supervisorItemId = supervisorId.Combine(templateId);
            var interviewerItemId = interviewerId.Combine(templateId);

            var summaryStore = new InMemoryReadSideRepositoryAccessor<SummaryItem>();

            summaryStore.Store(
                new SummaryItem()
                    {
                        UnassignedCount = 1,
                        ResponsibleId = supervisorId,
                        QuestionnaireStatus = SurveyStatus.Unassign.PublicId
                    }, supervisorItemId);
            summaryStore.Store(
                new SummaryItem()
                {
                    UnassignedCount = 1,
                    ResponsibleId = interviewerId,
                    ResponsibleSupervisorId = supervisorId,
                    QuestionnaireStatus = SurveyStatus.Unassign.PublicId
                }, interviewerItemId);

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, questionnarieId, supervisorId,
                                                                   interviewerId, templateId,null, false);

            var statusChangeEvent =
                CreatePublishedEvent(new QuestionnaireStatusChanged()
                    {
                        Status = SurveyStatus.Initial,
                        PreviousStatus = SurveyStatus.Unassign
                    }, questionnarieId);
            //act

            target.Handle(statusChangeEvent);

            //assert
            var queryForInterviewerItem = summaryStore.Query(_ => _.First(i => i.ResponsibleId == interviewerId));

            Assert.That(queryForInterviewerItem.UnassignedCount, Is.EqualTo(0));
            Assert.That(queryForInterviewerItem.InitialCount, Is.EqualTo(1));

            var queryForSupervisorItem = summaryStore.Query(_ => _.First(i => i.ResponsibleId == supervisorId));

            Assert.That(queryForSupervisorItem.UnassignedCount, Is.EqualTo(0));
            Assert.That(queryForSupervisorItem.InitialCount, Is.EqualTo(1));
        }

        private SummaryDenormalizer CreateSummaryDenormalizer(IReadSideRepositoryWriter<SummaryItem> summaryStore,
                                                              Guid questionnarieId, Guid supervisorId, Guid? interviewerId=null, Guid? tempalteId=null, SurveyStatus? status=null, bool isSupervisorOwner=true)
        {
            var questionnarieStore = new InMemoryReadSideRepositoryAccessor<CompleteQuestionnaireBrowseItem>();
            questionnarieStore.Store(
                new CompleteQuestionnaireBrowseItem(new CompleteQuestionnaireDocument())
                    {
                        TemplateId = tempalteId ?? Guid.NewGuid(),
                        Responsible =
                            new UserLight(isSupervisorOwner ? supervisorId : interviewerId ?? Guid.NewGuid(), "test"),
                        Status = status??SurveyStatus.Unassign
                    },
                questionnarieId);

            var userStore = new InMemoryReadSideRepositoryAccessor<UserDocument>();
            if (interviewerId.HasValue)
                userStore.Store(new UserDocument() { PublicKey = interviewerId.Value,Supervisor = new UserLight(supervisorId,"t")}, interviewerId.Value);
            userStore.Store(new UserDocument() { PublicKey = supervisorId }, supervisorId);
            return new SummaryDenormalizer(summaryStore, userStore,
                                           questionnarieStore);
        }


        private IPublishedEvent<T> CreatePublishedEvent<T>(T evt, Guid eventSourceId)
        {
            var eventMock = new Mock<IPublishedEvent<T>>();
            eventMock.Setup(x => x.Payload).Returns(evt);
            eventMock.Setup(x => x.EventSourceId).Returns(eventSourceId);
           return eventMock.Object;
        }
    }
}

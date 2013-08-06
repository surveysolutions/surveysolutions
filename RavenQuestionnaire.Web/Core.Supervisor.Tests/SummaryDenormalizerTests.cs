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
        private readonly Guid headquarterId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        private readonly Guid supervisorId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private readonly Guid interviewerId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private readonly Guid interviewer1Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
        private readonly Guid templateId = Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");
        private readonly Guid template1Id = Guid.Parse("EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE");
        private readonly Guid interviewId = Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-111111111111");

        [TestCase("AA6C0DC1-23C4-4B03-A3ED-B24EF0055555", "Approved")]
        [TestCase("776C0DC1-23C4-4B03-A3ED-B24EF005559B", "Completed")]
        [TestCase("D65CF1F6-8A75-43FA-9158-B745EB4D6A1F", "Completed with error")]
        [TestCase("8927D124-3CFB-4374-AD36-2FD99B62CE13", "Initial")]
        [TestCase("2bb6f94d-5beb-4374-8749-fac7cee1e020", "Redo")]
        [TestCase("4da8dddb-b31d-4508-bde6-178160705ba1", "Unassigned")]
        public void HandleInterviewDeleted_When_only_one_interview_in_given_status_deleted_and_total_1_Then_all_counters_should_be_0(string statusId, string statusName)
        {
            // Arrange
            var status = new SurveyStatus {PublicId = new Guid(statusId), Name = statusName};

            var summaryStore = CreateInmemorySummaryStore();
            var itemId = GetStoreItemId(this.supervisorId, templateId);
            summaryStore.Store(this.CreateSummaryItem(supervisorId, supervisorId, status.PublicId, templateId, increaseStatus: status.PublicId), itemId);

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, this.interviewId, templateId, status: status);

            var deleteInteviewEvent = this.CreatePublishedInterviewDeletedEvent(this.interviewId, headquarterId);
            // Act
            target.Handle(deleteInteviewEvent);

            // Assert
            var item = summaryStore.GetById(itemId);
            Assert.That(item.UnassignedCount, Is.EqualTo(0));
            Assert.That(item.InitialCount, Is.EqualTo(0));
            Assert.That(item.RedoCount, Is.EqualTo(0));
            Assert.That(item.CompletedCount, Is.EqualTo(0));
            Assert.That(item.CompletedWithErrorsCount, Is.EqualTo(0));
            Assert.That(item.ApprovedCount, Is.EqualTo(0));
            Assert.That(item.TotalCount, Is.EqualTo(0));
            Assert.True(item.DeletedInterviews.Contains(this.interviewId));
        }

        [TestCase("AA6C0DC1-23C4-4B03-A3ED-B24EF0055555", "Approved")]
        [TestCase("776C0DC1-23C4-4B03-A3ED-B24EF005559B", "Completed")]
        [TestCase("D65CF1F6-8A75-43FA-9158-B745EB4D6A1F", "Completed with error")]
        [TestCase("8927D124-3CFB-4374-AD36-2FD99B62CE13", "Initial")]
        [TestCase("2bb6f94d-5beb-4374-8749-fac7cee1e020", "Redo")]
        [TestCase("4da8dddb-b31d-4508-bde6-178160705ba1", "Unassigned")]
        public void HandleInterviewDeleted_When_user_has_surveys_in_all_statuses_deletes_interview_in_given_status_Then_all_counters_should_be_1(string statusId, string statusName)
        {
            // Arrange
            var status = new SurveyStatus { PublicId = new Guid(statusId), Name = statusName };

            var summaryStore = CreateInmemorySummaryStore();
            var itemId = GetStoreItemId(this.supervisorId, templateId);
            summaryStore.Store(this.CreateSummaryItem(supervisorId, supervisorId, status.PublicId, templateId, increaseStatus: status.PublicId, approved:1, completed:1, error: 1, initial: 1, redo: 1, unassigned: 1, total:6), itemId);

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, this.interviewId, templateId, status: status);

            var deleteInteviewEvent = this.CreatePublishedInterviewDeletedEvent(this.interviewId, headquarterId);
            // Act
            target.Handle(deleteInteviewEvent);

            // Assert
            var item = summaryStore.GetById(itemId);
            Assert.That(item.UnassignedCount, Is.EqualTo(1));
            Assert.That(item.InitialCount, Is.EqualTo(1));
            Assert.That(item.RedoCount, Is.EqualTo(1));
            Assert.That(item.CompletedCount, Is.EqualTo(1));
            Assert.That(item.CompletedWithErrorsCount, Is.EqualTo(1));
            Assert.That(item.ApprovedCount, Is.EqualTo(1));
            Assert.That(item.TotalCount, Is.EqualTo(6));
            Assert.True(item.DeletedInterviews.Contains(this.interviewId));
        }

        ///////////////////
        //// Assignments 
        ///////////////////

        [Test]
        public void HandleQuestionnaireAssignmentChanged_When_interview_assingnment_changed_event_has_come_but_with_same_responsible_Then_nothing_should_happen()
        {
            // Arrange
            var summaryStore = CreateInmemorySummaryStore();
            var itemId = GetStoreItemId(this.supervisorId, templateId);
            summaryStore.Store(this.CreateSummaryItem(supervisorId, supervisorId, SurveyStatus.Unassign.PublicId, templateId, total: 1, unassigned: 1), itemId);

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, this.interviewId, templateId);

            var assignmentChangeEvent = this.CreatePublishedAssignmentChangedEvent(this.interviewId, new UserLight(supervisorId, "supervisor"), new UserLight(supervisorId, "supervisor"));
            // Act
            target.Handle(assignmentChangeEvent);

            // Assert
            var item = summaryStore.GetById(itemId);
            Assert.That(item.UnassignedCount, Is.EqualTo(1));
            Assert.That(item.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public void HandleQuestionnaireAssignmentChanged_When_store_is_empty_and_event_come_Then_one_record_should_be_created()
        {
            // Arrange
            var summaryStore = CreateInmemorySummaryStore();

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, this.interviewId, templateId);

            var assignmentChangeEvent = this.CreatePublishedAssignmentChangedEvent(this.interviewId, null, new UserLight(supervisorId, "supervisor"));
            // Act
            target.Handle(assignmentChangeEvent);

            // Assert
            Assert.That(summaryStore.Count(), Is.EqualTo(1));
        }

        [Test]
        public void HandleQuestionnaireAssignmentChanged_When_store_is_not_empty_but_event_has_new_template_Then_one_record_should_be_created()
        {
            // Arrange
            var summaryStore = CreateInmemorySummaryStore();
            var itemId = GetStoreItemId(this.supervisorId, template1Id);
            summaryStore.Store(this.CreateSummaryItem(supervisorId, supervisorId, SurveyStatus.Unassign.PublicId, template1Id, total: 1, unassigned: 1), itemId);

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, this.interviewId, templateId);

            var assignmentChangeEvent = this.CreatePublishedAssignmentChangedEvent(this.interviewId, null, new UserLight(supervisorId, "supervisor"));

            // Act
            target.Handle(assignmentChangeEvent);

            // Assert
            Assert.That(summaryStore.Count(), Is.EqualTo(2));
        }

        [Test]
        public void HandleQuestionnaireAssignmentChanged_When_SummaryItem_is_absent_and_assignment_has_come_Then_SummaryItem_should_have_1_in_unassigned_counter_and_total_is_1()
        {
            // Arrange
            var summaryStore = CreateInmemorySummaryStore();

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, this.interviewId, templateId);

            var assignmentChangeEvent = this.CreatePublishedAssignmentChangedEvent(this.interviewId, null, new UserLight(supervisorId, "supervisor"));

            // Act
            target.Handle(assignmentChangeEvent);

            // Assert
            var item = summaryStore.GetById(GetStoreItemId(this.supervisorId, this.templateId));
            Assert.That(item.UnassignedCount, Is.EqualTo(1));
            Assert.That(item.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public void HandleQuestionnaireAssignmentChanged_When_SummaryItem_is_absent_and_assignment_to_supervisor_has_come_Then_supervisors_id_should_be_in_Responsible_and_ResponsibleSupervisor_fields()
        {
            // Arrange
            var summaryStore = CreateInmemorySummaryStore();

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, this.interviewId, templateId);

            var assignmentChangeEvent = this.CreatePublishedAssignmentChangedEvent(this.interviewId, null, new UserLight(supervisorId, "supervisor"));

            // Act
            target.Handle(assignmentChangeEvent);

            // Assert
            var item = summaryStore.GetById(GetStoreItemId(this.supervisorId, this.templateId));
            Assert.That(item.ResponsibleId, Is.EqualTo(supervisorId));
            Assert.That(item.ResponsibleSupervisorId, Is.EqualTo(supervisorId));
        }

        [Test]
        public void HandleQuestionnaireAssignmentChanged_When_supervisor_assigns_interview_on_interviewer_Then_one_more_record_should_be_created()
        {
            // Arrange
            var summaryStore = CreateInmemorySummaryStore();
            var itemId = GetStoreItemId(this.supervisorId, templateId);
            summaryStore.Store(this.CreateSummaryItem(supervisorId, supervisorId, SurveyStatus.Unassign.PublicId, templateId, total: 1, unassigned: 1), itemId);

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, this.interviewId, templateId);

            var assignmentChangeEvent = this.CreatePublishedAssignmentChangedEvent(this.interviewId, new UserLight(supervisorId, "supervisor"), new UserLight(interviewerId, "interviewer"));

            // Act
            target.Handle(assignmentChangeEvent);

            // Assert
            Assert.That(summaryStore.Count(), Is.EqualTo(2));
        }

        [Test]
        public void HandleQuestionnaireAssignmentChanged_When_supervisor_assigns_interview_on_interviewer_Then_new_summary_item_should_contains_1_inunassigned_and_1_in_total()
        {
            // Arrange
            var summaryStore = CreateInmemorySummaryStore();
            summaryStore.Store(this.CreateSummaryItem(this.supervisorId, this.supervisorId, SurveyStatus.Unassign.PublicId, this.templateId, total: 1, unassigned: 1), GetStoreItemId(this.supervisorId, this.templateId));

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, this.interviewId, templateId);

            var assignmentChangeEvent = this.CreatePublishedAssignmentChangedEvent(this.interviewId, new UserLight(supervisorId, "supervisor"), new UserLight(interviewerId, "interviewer"));

            // Act
            target.Handle(assignmentChangeEvent);

            // Assert
            var item = summaryStore.GetById(GetStoreItemId(this.interviewerId, this.templateId));
            Assert.That(item.UnassignedCount, Is.EqualTo(1));
            Assert.That(item.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public void HandleQuestionnaireAssignmentChanged_When_supervisor_assigns_interview_on_interviewer_Then_new_summary_item_should_contains_interviewer_as_responsible_and_supervisor_as_responsibleSupervisor()
        {
            // Arrange
            var summaryStore = CreateInmemorySummaryStore();
            summaryStore.Store(this.CreateSummaryItem(this.supervisorId, this.supervisorId, SurveyStatus.Unassign.PublicId, this.templateId, total: 1, unassigned: 1), GetStoreItemId(this.supervisorId, this.templateId));

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, this.interviewId, templateId);

            var assignmentChangeEvent = this.CreatePublishedAssignmentChangedEvent(this.interviewId, new UserLight(supervisorId, "supervisor"), new UserLight(interviewerId, "interviewer"));
            // Act
            target.Handle(assignmentChangeEvent);

            // Assert
            var item = summaryStore.GetById(GetStoreItemId(this.interviewerId, this.templateId));
            Assert.That(item.ResponsibleId, Is.EqualTo(interviewerId));
            Assert.That(item.ResponsibleSupervisorId, Is.EqualTo(supervisorId));
        }

        [Test]
        public void HandleQuestionnaireAssignmentChanged_When_supervisor_assigns_interview_on_interviewer_Then_old_summary_item_should_contains_0_in_unassigned_and_0_in_total()
        {
            // Arrange
            var summaryStore = CreateInmemorySummaryStore();
            summaryStore.Store(this.CreateSummaryItem(this.supervisorId, this.supervisorId, SurveyStatus.Unassign.PublicId, this.templateId, total: 1, unassigned: 1), GetStoreItemId(this.supervisorId, this.templateId));

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, this.interviewId, templateId);

            var assignmentChangeEvent = this.CreatePublishedAssignmentChangedEvent(this.interviewId, new UserLight(supervisorId, "supervisor"), new UserLight(interviewerId, "interviewer"));
            // Act
            target.Handle(assignmentChangeEvent);

            // Assert
            var item = summaryStore.GetById(GetStoreItemId(this.supervisorId, this.templateId));
            Assert.That(item.TotalCount, Is.EqualTo(0));
            Assert.That(item.UnassignedCount, Is.EqualTo(0));
        }

        [Test]
        public void HandleQuestionnaireAssignmentChanged_When_supervisor_assigns_one_more_interview_on_interviewer_Then_no_more_records_should_be_created()
        {
            // Arrange
            var summaryStore = CreateInmemorySummaryStore();
            summaryStore.Store(this.CreateSummaryItem(this.supervisorId, this.supervisorId, SurveyStatus.Unassign.PublicId, this.templateId, total: 1, unassigned: 1), GetStoreItemId(this.supervisorId, this.templateId));
            summaryStore.Store(this.CreateSummaryItem(this.interviewerId, this.supervisorId, SurveyStatus.Initial.PublicId, this.templateId, total: 1, initial: 1), GetStoreItemId(this.interviewerId, this.templateId));


            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, this.interviewId, templateId);

            var assignmentChangeEvent = this.CreatePublishedAssignmentChangedEvent(this.interviewId, new UserLight(supervisorId, "supervisor"), new UserLight(interviewerId, "interviewer"));
            // Act
            target.Handle(assignmentChangeEvent);

            // Assert
            Assert.That(summaryStore.Count(), Is.EqualTo(2));
        }

        [Test]
        public void HandleQuestionnaireAssignmentChanged_When_supervisor_assigns_one_more_interview_on_interviewer_Then_unassigned_should_be_1_initias_1_total_2()
        {
            // Arrange
            var summaryStore = CreateInmemorySummaryStore();
            summaryStore.Store(this.CreateSummaryItem(this.supervisorId, this.supervisorId, SurveyStatus.Unassign.PublicId, this.templateId, total: 1, unassigned: 1), GetStoreItemId(this.supervisorId, this.templateId));
            summaryStore.Store(this.CreateSummaryItem(this.interviewerId, this.supervisorId, SurveyStatus.Initial.PublicId, this.templateId, total: 1, initial: 1), GetStoreItemId(this.interviewerId, this.templateId));


            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, this.interviewId, templateId);

            var assignmentChangeEvent = this.CreatePublishedAssignmentChangedEvent(this.interviewId, new UserLight(supervisorId, "supervisor"), new UserLight(interviewerId, "interviewer"));
            // Act
            target.Handle(assignmentChangeEvent);

            // Assert
            var item = summaryStore.GetById(GetStoreItemId(this.interviewerId, this.templateId));
            Assert.That(item.TotalCount, Is.EqualTo(2));
            Assert.That(item.InitialCount, Is.EqualTo(1));
            Assert.That(item.UnassignedCount, Is.EqualTo(1));
        }

        ///////////////////
        //// Statuses 
        ///////////////////

        [Test]
        public void HandleQuestionnaireStatusChanged_When_interview_status_changed_event_has_come_but_with_same_status_Then_nothing_should_happen()
        {
            // Arrange
            var summaryStore = CreateInmemorySummaryStore();
            var itemId = GetStoreItemId(this.supervisorId, templateId);
            summaryStore.Store(this.CreateSummaryItem(supervisorId, supervisorId, SurveyStatus.Initial.PublicId, templateId, total: 1, initial: 1), itemId);

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, this.interviewId, templateId);

            var statusChangeEvent = this.CreatePublishedStatusChangedEvent(this.interviewId, SurveyStatus.Initial, SurveyStatus.Initial);
            // Act
            target.Handle(statusChangeEvent);

            // Assert
            var item = summaryStore.GetById(itemId);
            Assert.That(item.InitialCount, Is.EqualTo(1));
            Assert.That(item.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public void HandleQuestionnaireStatusChanged_When_interview_status_changes_but_summary_item_is_absent_Then_nothing_should_happen()
        {
            // Arrange
            var summaryStore = CreateInmemorySummaryStore();

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, this.interviewId, templateId);

            var statusChangeEvent = this.CreatePublishedStatusChangedEvent(this.interviewId, SurveyStatus.Unknown, SurveyStatus.Unassign);
            // Act
            target.Handle(statusChangeEvent);

            // Assert
            Assert.That(summaryStore.Count(), Is.EqualTo(0));
        }

        [Test]
        public void HandleQuestionnaireStatusChanged_When_interview_status_changes_from_Unknown_to_Unassign_Then_unassigned_count_should_not_be_changed_and_total_equals_1()
        {
            // Arrange
            var summaryStore = CreateInmemorySummaryStore();
            var itemId = GetStoreItemId(this.supervisorId, templateId);
            summaryStore.Store(this.CreateSummaryItem(supervisorId, supervisorId, SurveyStatus.Unassign.PublicId, templateId, total: 1, unassigned: 1), itemId);

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, this.interviewId, templateId);

            var statusChangeEvent = this.CreatePublishedStatusChangedEvent(this.interviewId, SurveyStatus.Unknown, SurveyStatus.Unassign);
            // Act
            target.Handle(statusChangeEvent);

            // Assert
            var item = summaryStore.GetById(itemId);
            Assert.That(item.UnassignedCount, Is.EqualTo(1));
            Assert.That(item.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public void HandleQuestionnaireStatusChanged_When_interview_status_change_from_Unassign_to_Initial_Then_unassigned_count_decrement_and_initial_count_increment_and_total_equals_1()
        {
            // Arrange
            var summaryStore = CreateInmemorySummaryStore();
            var itemId = GetStoreItemId(this.supervisorId, templateId);
            summaryStore.Store(this.CreateSummaryItem(supervisorId, supervisorId, SurveyStatus.Unassign.PublicId, templateId, total: 1, unassigned: 1), itemId);

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, this.interviewId, templateId);

            var statusChangeEvent = this.CreatePublishedStatusChangedEvent(this.interviewId, SurveyStatus.Unassign, SurveyStatus.Initial);
            // Act
            target.Handle(statusChangeEvent);

            // Assert
            var item = summaryStore.GetById(itemId);
            Assert.That(item.UnassignedCount, Is.EqualTo(0));
            Assert.That(item.InitialCount, Is.EqualTo(1));
            Assert.That(item.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public void HandleQuestionnaireStatusChanged_When_interview_status_change_from_Initial_to_Complete_Then_initial_count_decrement_and_complete_count_increment_and_total_equals_1()
        {
            // Arrange
            var summaryStore = CreateInmemorySummaryStore();
            var itemId = GetStoreItemId(this.supervisorId, templateId);
            summaryStore.Store(this.CreateSummaryItem(supervisorId, supervisorId, SurveyStatus.Initial.PublicId, templateId, total: 1, initial: 1), itemId);

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, this.interviewId, templateId);

            var statusChangeEvent = this.CreatePublishedStatusChangedEvent(this.interviewId, SurveyStatus.Initial, SurveyStatus.Complete);
            // Act
            target.Handle(statusChangeEvent);

            // Assert
            var item = summaryStore.GetById(itemId);
            Assert.That(item.InitialCount, Is.EqualTo(0));
            Assert.That(item.CompletedCount, Is.EqualTo(1));
            Assert.That(item.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public void HandleQuestionnaireStatusChanged_When_deleted_interview_status_change_from_Initial_to_Complete_Then_initial_count_is_the_same_and_complete_count_increment_and_total_equals_1()
        {
            // Arrange
            var summaryStore = CreateInmemorySummaryStore();
            var itemId = GetStoreItemId(this.supervisorId, templateId);
            var summaryItemWithOneDeletedInterview = this.CreateSummaryItem(supervisorId, supervisorId,
                                                                            SurveyStatus.Unassign.PublicId, templateId);
            summaryItemWithOneDeletedInterview.DeletedInterviews.Add(this.interviewId);
            summaryStore.Store(summaryItemWithOneDeletedInterview, itemId);

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, this.interviewId, templateId);

            var statusChangeEvent = this.CreatePublishedStatusChangedEvent(this.interviewId, SurveyStatus.Initial, SurveyStatus.Complete);
            // Act
            target.Handle(statusChangeEvent);

            // Assert
            var item = summaryStore.GetById(itemId);
            Assert.That(item.InitialCount, Is.EqualTo(0));
            Assert.That(item.CompletedCount, Is.EqualTo(1));
            Assert.That(item.TotalCount, Is.EqualTo(1));
        }



        [Test]
        public void HandleQuestionnaireStatusChanged_When_interview_status_change_from_Initial_to_CompleteWithError_Then_initial_count_decrement_and_error_count_increment_and_total_equals_1()
        {
            // Arrange
            var summaryStore = CreateInmemorySummaryStore();
            var itemId = GetStoreItemId(this.supervisorId, templateId);
            summaryStore.Store(this.CreateSummaryItem(supervisorId, supervisorId, SurveyStatus.Initial.PublicId, templateId, total: 1, initial: 1), itemId);

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, this.interviewId, templateId);

            var statusChangeEvent = this.CreatePublishedStatusChangedEvent(this.interviewId, SurveyStatus.Initial, SurveyStatus.Error);
            // Act
            target.Handle(statusChangeEvent);

            // Assert
            var item = summaryStore.GetById(itemId);
            Assert.That(item.InitialCount, Is.EqualTo(0));
            Assert.That(item.CompletedWithErrorsCount, Is.EqualTo(1));
            Assert.That(item.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public void HandleQuestionnaireStatusChanged_When_interview_status_change_from_Complete_to_Redo_Then_complete_count_decrement_and_redo_count_increment_and_total_equals_1()
        {
            // Arrange
            var summaryStore = CreateInmemorySummaryStore();
            var itemId = GetStoreItemId(this.supervisorId, templateId);
            summaryStore.Store(this.CreateSummaryItem(supervisorId, supervisorId, SurveyStatus.Complete.PublicId, templateId, total: 1, completed: 1), itemId);

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, this.interviewId, templateId);

            var statusChangeEvent = this.CreatePublishedStatusChangedEvent(this.interviewId, SurveyStatus.Complete, SurveyStatus.Redo);
            // Act
            target.Handle(statusChangeEvent);

            // Assert
            var item = summaryStore.GetById(itemId);
            Assert.That(item.CompletedCount, Is.EqualTo(0));
            Assert.That(item.RedoCount, Is.EqualTo(1));
            Assert.That(item.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public void HandleQuestionnaireStatusChanged_When_interview_status_change_from_Complete_to_Approve_Then_complete_count_decrement_and_approve_count_increment_and_total_equals_1()
        {
            // Arrange
            var summaryStore = CreateInmemorySummaryStore();
            var itemId = GetStoreItemId(this.supervisorId, templateId);
            summaryStore.Store(this.CreateSummaryItem(supervisorId, supervisorId, SurveyStatus.Complete.PublicId, templateId, total: 1, completed: 1), itemId);

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, this.interviewId, templateId);

            var statusChangeEvent = this.CreatePublishedStatusChangedEvent(this.interviewId, SurveyStatus.Complete, SurveyStatus.Approve);
            // Act
            target.Handle(statusChangeEvent);

            // Assert
            var item = summaryStore.GetById(itemId);
            Assert.That(item.CompletedCount, Is.EqualTo(0));
            Assert.That(item.ApprovedCount, Is.EqualTo(1));
            Assert.That(item.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public void HandleQuestionnaireStatusChanged_When_interview_status_change_from_Redo_to_Complete_Then_redo_count_decrement_and_complete_count_increment_and_total_equals_1()
        {
            // Arrange
            var summaryStore = CreateInmemorySummaryStore();
            var itemId = GetStoreItemId(this.supervisorId, templateId);
            summaryStore.Store(this.CreateSummaryItem(supervisorId, supervisorId, SurveyStatus.Redo.PublicId, templateId, total: 1, redo: 1), itemId);

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, this.interviewId, templateId);

            var statusChangeEvent = this.CreatePublishedStatusChangedEvent(this.interviewId, SurveyStatus.Redo, SurveyStatus.Complete);
            // Act
            target.Handle(statusChangeEvent);

            // Assert
            var item = summaryStore.GetById(itemId);
            Assert.That(item.RedoCount, Is.EqualTo(0));
            Assert.That(item.CompletedCount, Is.EqualTo(1));
            Assert.That(item.TotalCount, Is.EqualTo(1));
        }

        ///////////////////
        //// Misc 
        ///////////////////

        [Test]
        public void Handle_When_New_assigment_event_is_arrived_summary_Store_is_not_empty_Then_update_record_with_Responsible_interviewer()
        {
            //arrange
            var questionnarieId = Guid.NewGuid();
            var templateId = Guid.NewGuid();
            var itemId = supervisorId.Combine(templateId);
            var summaryStore = new InMemoryReadSideRepositoryAccessor<SummaryItem>();

            summaryStore.Store(new SummaryItem() { UnassignedCount = 1, ResponsibleId = supervisorId}, itemId);

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, questionnarieId, templateId);

            var newAssigmentEvent =
                CreatePublishedEvent(new QuestionnaireAssignmentChanged()
                {
                    Responsible = new UserLight(interviewerId, "interviewer"),
                    PreviousResponsible = new UserLight(supervisorId, "supervisor")
                }, questionnarieId);
            //act

            target.Handle(newAssigmentEvent);

            //assert


            var queryForInterviewerItem = summaryStore.Query(_ => _.First(i => i.ResponsibleId == interviewerId));

            Assert.That(queryForInterviewerItem.ResponsibleSupervisorId, Is.EqualTo(supervisorId));

            Assert.That(queryForInterviewerItem.UnassignedCount, Is.EqualTo(1));

            var queryForSupervisorItem = summaryStore.Query(_ => _.First(i => i.ResponsibleId == supervisorId));

            Assert.That(queryForSupervisorItem.UnassignedCount, Is.EqualTo(0));
            Assert.Null(queryForSupervisorItem.ResponsibleSupervisorId);
        }

        [Test]
        public void Handle_When_Staus_change_happend_Then_Counters_is_updated_for_interviewer_record_and_supervisor_record()
        {
            //arrange
            var questionnarieId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var templateId = Guid.Parse("21111111-1111-1111-1111-111111111111");
            var supervisorItemId = supervisorId.Combine(templateId);
            var interviewerItemId = interviewerId.Combine(templateId);

            var summaryStore = new InMemoryReadSideRepositoryAccessor<SummaryItem>();

            summaryStore.Store(
                new SummaryItem()
                {
                    UnassignedCount = 1,
                    ResponsibleId = interviewerId,
                    ResponsibleSupervisorId = supervisorId,
                    CurrentStatusId = SurveyStatus.Unassign.PublicId
                }, interviewerItemId);

            SummaryDenormalizer target = CreateSummaryDenormalizer(summaryStore, questionnarieId, templateId, null, false);

            var statusChangeEvent =
                CreatePublishedEvent(new QuestionnaireStatusChanged()
                    {
                        Status = SurveyStatus.Initial
                    }, questionnarieId);
            //act

            target.Handle(statusChangeEvent);

            //assert
            var queryForInterviewerItem = summaryStore.Query(_ => _.First(i => i.ResponsibleId == interviewerId));

            Assert.That(queryForInterviewerItem.UnassignedCount, Is.EqualTo(0));
            Assert.That(queryForInterviewerItem.InitialCount, Is.EqualTo(1));
        }

        private SummaryItem CreateSummaryItem(Guid responsibleId, Guid responsibleSupervisorId, Guid status, Guid templateId, Guid? increaseStatus = null, int total = 0, int unassigned = 0, int initial = 0, int completed = 0, int error = 0, int redo = 0, int approved = 0)
        {
            if (increaseStatus.HasValue)
            {
                unassigned += increaseStatus.Value != SurveyStatus.Unassign.PublicId ? 0 : 1;
                initial += increaseStatus.Value != SurveyStatus.Initial.PublicId ? 0 : 1;
                completed += increaseStatus.Value != SurveyStatus.Complete.PublicId ? 0 : 1;
                error += increaseStatus.Value != SurveyStatus.Error.PublicId ? 0 : 1;
                redo += increaseStatus.Value != SurveyStatus.Redo.PublicId ? 0 : 1;
                approved += increaseStatus.Value != SurveyStatus.Approve.PublicId ? 0 : 1;
                total += 1;

            }

            var summaryItem = new SummaryItem
                {
                    UnassignedCount = unassigned, 
                    InitialCount = initial, 
                    CompletedCount = completed, 
                    CompletedWithErrorsCount = error, 
                    RedoCount = redo, 
                    ApprovedCount = approved, 
                    TotalCount = total, 
                    ResponsibleId = responsibleId, 
                    ResponsibleSupervisorId = responsibleSupervisorId, 
                    TemplateId = templateId,
                    CurrentStatusId = status
                };

            return summaryItem;
        }

        private static Guid GetStoreItemId(Guid supervisorId, Guid templateId)
        {
            return supervisorId.Combine(templateId);
        }

        private SummaryDenormalizer CreateSummaryDenormalizer(IReadSideRepositoryWriter<SummaryItem> summaryStore,
                                                              Guid questionnarieId, Guid? tempalteId = null, SurveyStatus? status = null, bool isSupervisorOwner = true)
        {
            var questionnarieStore = CreateInmemoryQuestionnarieStore();
            var userStore = this.CreateInmemoryUserStoreWithAllUsers();

            questionnarieStore.Store(
                new CompleteQuestionnaireBrowseItem(new CompleteQuestionnaireDocument())
                    {
                        TemplateId = tempalteId ?? Guid.NewGuid(),
                        Responsible = new UserLight(isSupervisorOwner ? supervisorId : interviewerId, "test"),
                        Status = status ?? SurveyStatus.Unassign
                    },
                questionnarieId);

            return new SummaryDenormalizer(summaryStore, userStore, questionnarieStore);
        }

        private InMemoryReadSideRepositoryAccessor<UserDocument> CreateInmemoryUserStoreWithAllUsers()
        {
            var store = new InMemoryReadSideRepositoryAccessor<UserDocument>();

            store.Store(new UserDocument { PublicKey = headquarterId, UserName = "headquarter", Roles = new List<UserRoles>() { UserRoles.Headquarter } }, headquarterId);
            store.Store(new UserDocument { PublicKey = supervisorId, UserName = "supervisor", Roles = new List<UserRoles>() { UserRoles.Supervisor } }, supervisorId);
            store.Store(new UserDocument { PublicKey = interviewerId, Supervisor = new UserLight(supervisorId, "supervisor") }, interviewerId);
            store.Store(new UserDocument { PublicKey = interviewer1Id, Supervisor = new UserLight(supervisorId, "supervisor") }, interviewer1Id);

            return store;
        }

        private static InMemoryReadSideRepositoryAccessor<SummaryItem> CreateInmemorySummaryStore()
        {
            return new InMemoryReadSideRepositoryAccessor<SummaryItem>();
        }

        private static InMemoryReadSideRepositoryAccessor<CompleteQuestionnaireBrowseItem> CreateInmemoryQuestionnarieStore()
        {
            return new InMemoryReadSideRepositoryAccessor<CompleteQuestionnaireBrowseItem>();
        }


        private IPublishedEvent<T> CreatePublishedEvent<T>(T evt, Guid eventSourceId)
        {
            var eventMock = new Mock<IPublishedEvent<T>>();
            eventMock.Setup(x => x.Payload).Returns(evt);
            eventMock.Setup(x => x.EventSourceId).Returns(eventSourceId);
            return eventMock.Object;
        }


        private IPublishedEvent<InterviewDeleted> CreatePublishedInterviewDeletedEvent(Guid questionnarieId, Guid deletedBy)
        {
            return this.CreatePublishedEvent(new InterviewDeleted()
            {
                DeletedBy = deletedBy
            }, questionnarieId);
        }

        private IPublishedEvent<QuestionnaireAssignmentChanged> CreatePublishedAssignmentChangedEvent(Guid questionnarieId, UserLight previousResponsible, UserLight responsible)
        {
            return this.CreatePublishedEvent(new QuestionnaireAssignmentChanged()
            {
                PreviousResponsible = previousResponsible,
                Responsible = responsible
            }, questionnarieId);
        }

        private IPublishedEvent<QuestionnaireStatusChanged> CreatePublishedStatusChangedEvent(Guid questionnarieId, SurveyStatus previousStatus, SurveyStatus status)
        {
            return this.CreatePublishedEvent(new QuestionnaireStatusChanged { Status = status, PreviousStatus = previousStatus }, questionnarieId);
        }
    }
}

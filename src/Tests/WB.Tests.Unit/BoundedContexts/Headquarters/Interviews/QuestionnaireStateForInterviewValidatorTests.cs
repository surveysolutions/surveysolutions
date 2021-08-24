using System;
using Moq;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments.Validators;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;
using WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.LinkedQuestions;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Interviews
{
    [TestOf(typeof(QuestionnaireStateForAssignmentValidator))]
    public class QuestionnaireStateForInterviewValidatorTests 
    {
        [Test]
        public void should_not_allow_interview_creation_if_questionnaire_is_deleted()
        {
            QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 1); 
            var questionnaireBrowseItemRepository =
                SetUp.QuestionnaireBrowseItemRepository(new QuestionnaireBrowseItem[]{new QuestionnaireBrowseItem()
                {
                    QuestionnaireId = questionnaireIdentity.QuestionnaireId,
                    Version = questionnaireIdentity.Version,
                    IsDeleted = true
                }});

            var validator = Create.Service.QuestionnaireStateForInterviewValidator(questionnaireBrowseItemRepository);

            TestDelegate act = () =>
                validator.Validate(null, Create.Command.CreateInterview(questionnaireIdentity:questionnaireIdentity, userId: Id.gA));

            Assert.That(act, Throws.Exception.InstanceOf<InterviewException>()
                .With.Message.EqualTo(CommandValidatorsMessages.QuestionnaireWasDeleted)
                .And.Property(nameof(InterviewException.ExceptionType))
                .EqualTo(InterviewDomainExceptionType.QuestionnaireDeleted));
        }
        
        [Test]
        public void should_not_allow_interview_creation_if_questionnaire_is_disabled()
        {
            QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 13); 
            var questionnaireBrowseItemRepository =
                SetUp.QuestionnaireBrowseItemRepository(new QuestionnaireBrowseItem[]{new QuestionnaireBrowseItem()
                {
                    QuestionnaireId = questionnaireIdentity.QuestionnaireId,
                    Version = questionnaireIdentity.Version,
                    Disabled = true
                }});

            var validator = Create.Service.QuestionnaireStateForInterviewValidator(questionnaireBrowseItemRepository);

            TestDelegate act = () =>
                validator.Validate(null, Create.Command.CreateInterview(questionnaireIdentity:questionnaireIdentity, userId: Id.gA));

            Assert.That(act, Throws.Exception.InstanceOf<InterviewException>()
                .With.Message.EqualTo(CommandValidatorsMessages.QuestionnaireWasDeleted)
                .And.Property(nameof(InterviewException.ExceptionType))
                .EqualTo(InterviewDomainExceptionType.QuestionnaireDeleted));
        }
        
        
        [Test]
        public void should_not_allow_interview_update_if_questionnaire_is_disabled()
        {
            QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 13); 
            var questionnaireBrowseItemRepository =
                SetUp.QuestionnaireBrowseItemRepository(new QuestionnaireBrowseItem[]{new QuestionnaireBrowseItem()
                {
                    QuestionnaireId = questionnaireIdentity.QuestionnaireId,
                    Version = questionnaireIdentity.Version,
                    Disabled = true
                }});

            var validator = Create.Service.QuestionnaireStateForInterviewValidator(questionnaireBrowseItemRepository);

            var interviewId = Id.g1;
            
            var statefulInterview = new StatefulInterview(
                Create.Service.SubstitutionTextFactory(),
                Create.Service.InterviewTreeBuilder(),
                Create.Storage.QuestionnaireQuestionOptionsRepository());

            statefulInterview.QuestionnaireIdentity = questionnaireIdentity;
            
            TestDelegate act = () =>
                validator.Validate(statefulInterview, Create.Command.ResumeInterview(Id.g1));

            Assert.That(act, Throws.Exception.InstanceOf<InterviewException>()
                .With.Message.EqualTo(CommandValidatorsMessages.QuestionnaireWasDeleted)
                .And.Property(nameof(InterviewException.ExceptionType))
                .EqualTo(InterviewDomainExceptionType.QuestionnaireDeleted));
        }
    }
}

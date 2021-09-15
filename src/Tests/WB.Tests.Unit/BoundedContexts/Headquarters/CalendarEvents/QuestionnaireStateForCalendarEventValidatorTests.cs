using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments.Validators;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.CalendarEvents
{
    [TestOf(typeof(QuestionnaireStateForAssignmentValidator))]
    public class QuestionnaireStateForCalendarEventValidatorTests 
    {
        [Test]
        public void should_not_allow_calendar_event_creation_if_questionnaire_is_deleted()
        {
            QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 1); 
            var questionnaireBrowseItemRepository =
                SetUp.QuestionnaireBrowseItemRepository(new QuestionnaireBrowseItem[]{new QuestionnaireBrowseItem()
                {
                    QuestionnaireId = questionnaireIdentity.QuestionnaireId,
                    Version = questionnaireIdentity.Version,
                    IsDeleted = true
                }});

            var validator = Create.Service.QuestionnaireStateForCalendarEventValidator(questionnaireBrowseItemRepository);

            TestDelegate act = () =>
                validator.Validate(null, Create.Command.CreateCalendarEventCommand(questionnaireIdentity:questionnaireIdentity, userId: Id.gA));

            Assert.That(act, Throws.Exception.InstanceOf<CalendarEventException>()
                .With.Message.EqualTo(CommandValidatorsMessages.QuestionnaireWasDeleted)
                .And.Property(nameof(CalendarEventException.ExceptionType))
                .EqualTo(CalendarEventDomainExceptionType.QuestionnaireDeleted));
        }
        
        [Test]
        public void should_not_allow_calendar_event_creation_if_questionnaire_is_disabled()
        {
            QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 13); 
            var questionnaireBrowseItemRepository =
                SetUp.QuestionnaireBrowseItemRepository(new QuestionnaireBrowseItem[]{new QuestionnaireBrowseItem()
                {
                    QuestionnaireId = questionnaireIdentity.QuestionnaireId,
                    Version = questionnaireIdentity.Version,
                    Disabled = true
                }});

            var validator = Create.Service.QuestionnaireStateForCalendarEventValidator(questionnaireBrowseItemRepository);

            TestDelegate act = () =>
                validator.Validate(null, Create.Command.CreateCalendarEventCommand(questionnaireIdentity:questionnaireIdentity, userId: Id.gA));

            Assert.That(act, Throws.Exception.InstanceOf<CalendarEventException>()
                .With.Message.EqualTo(CommandValidatorsMessages.QuestionnaireWasDeleted)
                .And.Property(nameof(CalendarEventException.ExceptionType))
                .EqualTo(CalendarEventDomainExceptionType.QuestionnaireDeleted));
        }
        
        
        [Test]
        public void should_not_allow_calendar_event_update_if_questionnaire_is_disabled()
        {
            QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 13); 
            var questionnaireBrowseItemRepository =
                SetUp.QuestionnaireBrowseItemRepository(new QuestionnaireBrowseItem[]{new QuestionnaireBrowseItem()
                {
                    QuestionnaireId = questionnaireIdentity.QuestionnaireId,
                    Version = questionnaireIdentity.Version,
                    Disabled = true
                }});

            var validator = Create.Service.QuestionnaireStateForCalendarEventValidator(questionnaireBrowseItemRepository);

            TestDelegate act = () =>
                validator.Validate(null, Create.Command.UpdateCalendarEventCommand(questionnaireIdentity:questionnaireIdentity, userId: Id.gA));

            Assert.That(act, Throws.Exception.InstanceOf<CalendarEventException>()
                .With.Message.EqualTo(CommandValidatorsMessages.QuestionnaireWasDeleted)
                .And.Property(nameof(CalendarEventException.ExceptionType))
                .EqualTo(CalendarEventDomainExceptionType.QuestionnaireDeleted));
        }
    }
}

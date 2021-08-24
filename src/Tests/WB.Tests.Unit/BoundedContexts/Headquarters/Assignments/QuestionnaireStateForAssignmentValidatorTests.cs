using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments.Validators;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    [TestOf(typeof(QuestionnaireStateForAssignmentValidator))]
    public class QuestionnaireStateForAssignmentValidatorTests 
    {
        [Test]
        public void should_not_allow_creating_assignment_if_questionnaire_is_deleted()
        {
            QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 1); 
            var questionnaireBrowseItemRepository =
                SetUp.QuestionnaireBrowseItemRepository(new QuestionnaireBrowseItem[]{new QuestionnaireBrowseItem()
                {
                    QuestionnaireId = questionnaireIdentity.QuestionnaireId,
                    Version = questionnaireIdentity.Version,
                    IsDeleted = true
                }});

            var validator = Create.Service.QuestionnaireStateForAssignmentValidator(questionnaireBrowseItemRepository);

            TestDelegate act = () =>
                validator.Validate(null, Create.Command.CreateAssignment(questionnaireId:questionnaireIdentity,responsibleId: Id.gA, webMode: true));

            Assert.That(act, Throws.Exception.InstanceOf<AssignmentException>()
                .With.Message.EqualTo(CommandValidatorsMessages.QuestionnaireWasDeleted)
                .And.Property(nameof(AssignmentException.ExceptionType))
                .EqualTo(AssignmentDomainExceptionType.QuestionnaireDeleted));
        }
        
        [Test]
        public void should_not_allow_creating_assignment_if_questionnaire_is_disabled()
        {
            QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 13); 
            var questionnaireBrowseItemRepository =
                SetUp.QuestionnaireBrowseItemRepository(new QuestionnaireBrowseItem[]{new QuestionnaireBrowseItem()
                {
                    QuestionnaireId = questionnaireIdentity.QuestionnaireId,
                    Version = questionnaireIdentity.Version,
                    Disabled = true
                }});

            var validator = Create.Service.QuestionnaireStateForAssignmentValidator(questionnaireBrowseItemRepository);

            TestDelegate act = () =>
                validator.Validate(null, Create.Command.CreateAssignment(questionnaireId:questionnaireIdentity, responsibleId: Id.gA, webMode: true));

            Assert.That(act, Throws.Exception.InstanceOf<AssignmentException>().With.Message.EqualTo(CommandValidatorsMessages.QuestionnaireWasDeleted)
                .And.Property(nameof(AssignmentException.ExceptionType)).EqualTo(AssignmentDomainExceptionType.QuestionnaireDeleted));
        }
    }
}

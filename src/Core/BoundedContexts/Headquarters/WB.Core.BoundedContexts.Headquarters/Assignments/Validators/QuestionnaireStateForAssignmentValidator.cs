#nullable enable
using System;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.Assignments.Validators
{
    public class QuestionnaireStateForAssignmentValidator : ICommandValidator<AssignmentAggregateRoot, AssignmentCommand>
    {
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage;

        public QuestionnaireStateForAssignmentValidator(IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage)
        {
            this.questionnaireBrowseItemStorage = questionnaireBrowseItemStorage ?? throw new ArgumentNullException(nameof(questionnaireBrowseItemStorage));
        }

        public void Validate(AssignmentAggregateRoot? aggregate, AssignmentCommand command)
        {
            var questionnaire = this.questionnaireBrowseItemStorage.GetById(command.QuestionnaireId.ToString());

            if (questionnaire == null || questionnaire.Disabled || questionnaire.IsDeleted)
            {
                throw new AssignmentException(CommandValidatorsMessages.QuestionnaireWasDeleted, AssignmentDomainExceptionType.QuestionnaireDeleted);
            }
        }
    }
}

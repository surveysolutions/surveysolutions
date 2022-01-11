#nullable enable
using System;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.Headquarters.CalendarEvents.Validators
{
    public class QuestionnaireStateForCalendarEventValidator : ICommandValidator<
        WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.CalendarEvent, CalendarEventCommand>
    {
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage;

        public QuestionnaireStateForCalendarEventValidator(IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage)
        {
            this.questionnaireBrowseItemStorage = questionnaireBrowseItemStorage ?? throw new ArgumentNullException(nameof(questionnaireBrowseItemStorage));
        }

        public void Validate(WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.CalendarEvent? aggregate, CalendarEventCommand command)
        {
            var questionnaire = this.questionnaireBrowseItemStorage.GetById(command.QuestionnaireId.ToString());

            if (questionnaire == null || questionnaire.Disabled || questionnaire.IsDeleted)
            {
                throw new CalendarEventException(CommandValidatorsMessages.QuestionnaireWasDeleted, CalendarEventDomainExceptionType.QuestionnaireDeleted);
            }
        }
    }
}

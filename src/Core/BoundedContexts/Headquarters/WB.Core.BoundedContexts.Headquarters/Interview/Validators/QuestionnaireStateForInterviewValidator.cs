#nullable enable
using System;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Interview.Validators
{
    public class QuestionnaireStateForInterviewValidator : 
        ICommandValidator<StatefulInterview, CreateInterview>,
        ICommandValidator<StatefulInterview, CreateInterviewFromSynchronizationMetadata>,
        ICommandValidator<StatefulInterview, SynchronizeInterviewEventsCommand>,
        ICommandValidator<StatefulInterview, CreateTemporaryInterviewCommand>,
        
        ICommandValidator<StatefulInterview, InterviewCommand>
    {
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage;

        public QuestionnaireStateForInterviewValidator(IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage)
        {
            this.questionnaireBrowseItemStorage = questionnaireBrowseItemStorage ?? throw new ArgumentNullException(nameof(questionnaireBrowseItemStorage));
        }

        public void Validate(StatefulInterview aggregate, InterviewCommand command)
        {
            if (aggregate == null)
            {
                throw new InvalidOperationException("Interview is invalid");
            }
            
            CheckQuestionnaireAndThrow(aggregate.QuestionnaireIdentity);
        }

        private void CheckQuestionnaireAndThrow(QuestionnaireIdentity questionnaireIdentity)
        {
            QuestionnaireBrowseItem questionnaire = this.questionnaireBrowseItemStorage.GetById(questionnaireIdentity.ToString());

            if (questionnaire == null || questionnaire.Disabled || questionnaire.IsDeleted)
            {
                throw new InterviewException(CommandValidatorsMessages.QuestionnaireWasDeleted, InterviewDomainExceptionType.QuestionnaireDeleted);
            }
        }

        public void Validate(StatefulInterview? aggregate, CreateInterview command)
        {
            CheckQuestionnaireAndThrow(command.QuestionnaireId);
        }

        public void Validate(StatefulInterview? aggregate, CreateInterviewFromSynchronizationMetadata command)
        {
            CheckQuestionnaireAndThrow(new QuestionnaireIdentity(command.QuestionnaireId, command.QuestionnaireVersion));
        }

        public void Validate(StatefulInterview? aggregate, SynchronizeInterviewEventsCommand command)
        {
            CheckQuestionnaireAndThrow(new QuestionnaireIdentity(command.QuestionnaireId, command.QuestionnaireVersion));
        }

        public void Validate(StatefulInterview? aggregate, CreateTemporaryInterviewCommand command)
        {
            CheckQuestionnaireAndThrow(command.QuestionnaireId);
        }
    }
}

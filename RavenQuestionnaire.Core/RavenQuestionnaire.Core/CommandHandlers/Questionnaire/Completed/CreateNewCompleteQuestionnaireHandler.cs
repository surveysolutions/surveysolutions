using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Services;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Completed
{
    public class CreateNewCompleteQuestionnaireHandler : ICommandHandler<CreateNewCompleteQuestionnaireCommand>
    {
        private IQuestionnaireRepository _questionnaireRepository;
        private ICompleteQuestionnaireUploaderService _completeQuestionnaireUploader;
        
        
        public CreateNewCompleteQuestionnaireHandler(IQuestionnaireRepository questionnaireRepository, 
            ICompleteQuestionnaireUploaderService completeQuestionnaireUploader)
        {
            this._questionnaireRepository = questionnaireRepository;
            this._completeQuestionnaireUploader = completeQuestionnaireUploader;
        }

        public void Handle(CreateNewCompleteQuestionnaireCommand command)
        {
            var questionnaire = this._questionnaireRepository.Load(command.QuestionnaireId);
            var result =this._completeQuestionnaireUploader.CreateCompleteQuestionnaire(questionnaire, 
                command.Creator, command.Status);
            
            
            if (result != null)
                command.CompleteQuestionnaireId = IdUtil.ParseId(result.CompleteQuestinnaireId);

            var questions = result.GetInnerDocument().GetAllQuestions();
            var executor = new CompleteQuestionnaireConditionExecutor(result.GetInnerDocument());
            foreach (ICompleteQuestion completeQuestion in questions)
            {
                completeQuestion.Enabled = executor.Execute(completeQuestion);
                if (!completeQuestion.Enabled)
                    result.Remove(completeQuestion);
            }
        }
    }
}

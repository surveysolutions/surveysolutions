using RavenQuestionnaire.Core.Commands.Questionnaire;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Services;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire
{
    /// <summary>
    /// DeleteImageCommandHandler 
    /// </summary>
    public class DeleteImageCommandHandler : ICommandHandler<DeleteImageCommand>
    {
        private readonly IQuestionnaireRepository _questionnaireRepository;
        private IFileStorageService _fileStorageService;

        public DeleteImageCommandHandler(IFileStorageService fileStorageService, IQuestionnaireRepository questionnaireRepository)
        {
            _questionnaireRepository = questionnaireRepository;
            _fileStorageService = fileStorageService;
        }

        public void Handle(DeleteImageCommand command)
        {
            var questionnaire = _questionnaireRepository.Load(IdUtil.CreateQuestionnaireId(command.QuestionnaireId));

            var question = questionnaire.Find<AbstractQuestion>(command.QuestionKey);

            var card = question.RemoveCard(command.ImageKey);

            _fileStorageService.DeleteFile(card.OriginalBase64);
            _fileStorageService.DeleteFile(card.ThumbnailBase);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Services;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.CommandHandlers
{
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

            var question = questionnaire.Find<Question>(command.QuestionKey);

            var card = question.RemoveCard(command.ImageKey);

            _fileStorageService.DeleteFile(card.OriginalBase64);
            _fileStorageService.DeleteFile(card.ThumbnailBase);
        }
    }
}

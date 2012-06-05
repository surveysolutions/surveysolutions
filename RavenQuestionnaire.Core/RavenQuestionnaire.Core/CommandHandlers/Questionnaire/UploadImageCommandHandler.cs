using System;
using RavenQuestionnaire.Core.Commands.Questionnaire;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Services;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire
{
    public class UploadImageCommandHandler : ICommandHandler<UploadImageCommand>
    {
        private readonly IQuestionnaireRepository _questionnaireRepository;
        private IFileStorageService _fileStorageService;
        public UploadImageCommandHandler(IFileStorageService fileStorageService, IQuestionnaireRepository questionnaireRepository)
        {
            _questionnaireRepository = questionnaireRepository;
            _fileStorageService = fileStorageService;
        }
        public void Handle(UploadImageCommand command)
        {
            var id = Guid.NewGuid();
            string filename = String.Format("images/{0}.png", id);
            string thumbname = String.Format("images/{0}_thumb.png", id);

            _fileStorageService.StoreFile(filename, command.OriginalImage);
            _fileStorageService.StoreFile(thumbname, command.ThumbnailImage);

            var newImage = new Image
            {
                PublicKey = Guid.NewGuid(),
                Title = command.Title,
                Description = command.Description,
                OriginalBase64 = filename/*fileBase64*/,
                Width = command.OriginalWidth,
                Height = command.OriginalHeight,
                ThumbnailBase = thumbname/*thumbBase64*/,
                ThumbnailHeight = command.ThumbHeight,
                ThumbnailWidth = command.ThumbWidth,
                CreationDate = DateTime.Now
            };

            var questionnaire = _questionnaireRepository.Load(IdUtil.CreateQuestionnaireId(command.QuestionnaireId));

            var question = questionnaire.Find<AbstractQuestion>(command.PublicKey);

            question.AddCard(newImage);
        }
    }
}

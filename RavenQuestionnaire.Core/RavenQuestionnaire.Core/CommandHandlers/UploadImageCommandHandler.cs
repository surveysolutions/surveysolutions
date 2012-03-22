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
    public class UploadImageCommandHandler : ICommandHandler<UploadImageCommand>
    {
        private readonly IQuestionnaireRepository _questionnaireRepository;
        public UploadImageCommandHandler(IQuestionnaireRepository questionnaireRepository)
        {
            _questionnaireRepository = questionnaireRepository;
        }
        public void Handle(UploadImageCommand command)
        {
            string fileBase64 = Convert.ToBase64String(command.OriginalImage);
            string thumbBase64 = Convert.ToBase64String(command.ThumbnailImage);

            var newImage = new Image
                               {
                                   Title = command.Title, 
                                   Description = command.Description,
                                   OriginalBase64 = fileBase64,
                                   Width = command.OriginalWidth,
                                   Height = command.OriginalHeight,
                                   ThumbnailBase64 = thumbBase64,
                                   ThumbnailHeight = command.ThumbHeight,
                                   ThumbnailWidth = command.ThumbWidth,
                                   CreationDate = DateTime.Now
                               };

            var questionnaire =  _questionnaireRepository.Load(IdUtil.CreateQuestionnaireId(command.QuestionnaireId));

            var question = questionnaire.Find<Question>(command.PublicKey);

            question.AddCard(newImage);
        }
    }
}

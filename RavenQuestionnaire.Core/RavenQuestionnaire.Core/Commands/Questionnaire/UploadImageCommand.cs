using System;
using System.IO;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Questionnaire
{
    public class UploadImageCommand : ICommand
    {
        public UploadImageCommand(Guid publicKey, string questionnaireId, string title, string description, Stream thumbData, int thumbWidth, int thumbHeight, Stream originalImage, int originalWidth, int originalHeight, UserLight executor)
        {
            PublicKey = publicKey;
            QuestionnaireId = questionnaireId;
            Executor = executor;
            Description = description;
            Title = title;
            OriginalImage = originalImage;
            ThumbnailImage = thumbData;
            OriginalWidth = originalWidth;
            OriginalHeight = originalHeight;
            ThumbHeight = thumbHeight;
            ThumbWidth = thumbWidth;
        }

        #region Properties

        public Guid PublicKey { get; set; }

        public string QuestionnaireId { get; set; }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public Stream OriginalImage { get; private set; }

        public int OriginalWidth { get; private set; }

        public int OriginalHeight { get; private set; }

        public int ThumbWidth { get; private set; }

        public int ThumbHeight { get; private set; }

        public Stream ThumbnailImage { get; private set; }

        public UserLight Executor { get; set; }

        #endregion
    }
}
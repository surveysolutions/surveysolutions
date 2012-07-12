using System;
using System.IO;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Questionnaire
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "UploadImage")]
    public class UploadImageCommand : CommandBase
    {
        protected UploadImageCommand(Guid publicKey, Guid questionnaireId, string title, string description, int thumbWidth, int thumbHeight, int originalWidth, int originalHeight)
        {
            PublicKey = publicKey;
            QuestionnaireId = questionnaireId;
            Description = description;
            Title = title;
            OriginalWidth = originalWidth;
            OriginalHeight = originalHeight;
            ThumbHeight = thumbHeight;
            ThumbWidth = thumbWidth;
        }
        public UploadImageCommand(Guid publicKey, Guid questionnaireId, string title, string description, Stream thumbData, int thumbWidth, int thumbHeight,
            Stream originalImage, int originalWidth, int originalHeight)
            : this(publicKey, questionnaireId, title, description, thumbWidth, thumbHeight, originalWidth, originalHeight)
        {
            OriginalImage = ToBase64(originalImage);
            ThumbnailImage = ToBase64(thumbData);
        }
        public UploadImageCommand(Guid publicKey, Guid questionnaireId, string title, string description, string thumbData, int thumbWidth, int thumbHeight,
           string originalImage, int originalWidth, int originalHeight)
            : this(publicKey, questionnaireId, title, description, thumbWidth, thumbHeight, originalWidth, originalHeight)
        {
            OriginalImage = originalImage;
            ThumbnailImage = thumbData;
        }

        protected string ToBase64(Stream stream)
        {
         /*   Byte[] bytes = new Byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            return Convert.ToBase64String(bytes);*/
            string base64;
            // first I need the data as a byte[]; I'll use
            // MemoryStream, as a convenience; if you already
            // have the byte[] you can skip this
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] buffer = new byte[1024];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, bytesRead);
                }
                base64 = Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
            }
            return base64;
        }

        #region Properties

        public Guid PublicKey { get; set; }
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public string OriginalImage { get; private set; }

        public int OriginalWidth { get; private set; }

        public int OriginalHeight { get; private set; }

        public int ThumbWidth { get; private set; }

        public int ThumbHeight { get; private set; }

        public string ThumbnailImage { get; private set; }

        #endregion
    }
}
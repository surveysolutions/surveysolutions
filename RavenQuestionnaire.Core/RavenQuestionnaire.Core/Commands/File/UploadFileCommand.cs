using System;
using System.IO;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.File
{
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(FileAR))]
    public class UploadFileCommand : CommandBase
    {
        protected UploadFileCommand(Guid publicKey, string title, string desc, int thumbWidth, int thumbHeight, int origWidth, int origHeight)
        {
            PublicKey = publicKey;
            Description = desc;
            Title = title;
            OriginalWidth = origWidth;
            OriginalHeight = origHeight;
            ThumbHeight = thumbHeight;
            ThumbWidth = thumbWidth;
        }
        public UploadFileCommand(Guid publicKey, string title, string desc, Stream thumbData, int thumbWidth, int thumbHeight, Stream origData, int origWidth, int origHeight) :
            this(publicKey,title, desc, thumbWidth, thumbHeight, origWidth, origHeight)
        {
            OriginalFile = ToBase64(origData);
            ThumbFile = ToBase64(thumbData);
        }
        public UploadFileCommand(Guid publicKey, string title, string desc, string thumbData, int thumbWidth, int thumbHeight, string origData, int origWidth, int origHeight) :
            this(publicKey,title, desc, thumbWidth, thumbHeight, origWidth, origHeight)
        {
            OriginalFile = origData;
            ThumbFile = thumbData;
        }
        protected string ToBase64(Stream stream)
        {
            string base64;
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

        public Guid PublicKey { get; set; }
        public string Title { get;  set; }

        public string Description { get;  set; }

        public int OriginalWidth { get;  set; }

        public int OriginalHeight { get;  set; }

        public int ThumbWidth { get;  set; }

        public int ThumbHeight { get;  set; }

        public string OriginalFile { get; set; }

        public string ThumbFile { get; set; }

    }
}
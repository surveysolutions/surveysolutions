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
        protected UploadFileCommand(Guid publicKey, string title, string desc)
        {
            PublicKey = publicKey;
            Description = desc;
            Title = title;
        }
        public UploadFileCommand(Guid publicKey, string title, string desc,  Stream origData) :
            this(publicKey,title, desc)
        {
            OriginalFile = ToBase64(origData);
        }
        public UploadFileCommand(Guid publicKey, string title, string desc,string origData) :
            this(publicKey,title, desc)
        {
            OriginalFile = origData;
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

        public string OriginalFile { get; set; }

    }
}
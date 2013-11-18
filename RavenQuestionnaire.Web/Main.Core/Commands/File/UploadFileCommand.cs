namespace Main.Core.Commands.File
{
    using System;
    using System.IO;

    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The upload file command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(FileAR))]
    public class UploadFileCommand : CommandBase
    {
        public UploadFileCommand(Guid publicKey, string title, string desc, Stream origData)
            : this(publicKey, title, desc)
        {
            this.OriginalFile = this.ToBase64(origData);
        }

        public UploadFileCommand(Guid publicKey, string title, string desc, string origData)
            : this(publicKey, title, desc)
        {
            this.OriginalFile = origData;
        }

        protected UploadFileCommand(Guid publicKey, string title, string desc)
        {
            this.PublicKey = publicKey;
            this.Description = desc;
            this.Title = title;
        }

        public string Description { get; set; }

        public string OriginalFile { get; set; }

        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        protected string ToBase64(Stream stream)
        {
            string base64;
            using (var ms = new MemoryStream())
            {
                var buffer = new byte[1024];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, bytesRead);
                }

                base64 = Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
            }

            return base64;
        }
    }
}
#region

using System;
using System.IO;

#endregion

namespace RavenQuestionnaire.Core.Documents
{
    public class FileDescription
    {
        public string PublicKey { get; set; }
       // public Guid ThumbPublicKey { get; set; }
        // public DateTime CreationDate { get; set; }
        public string Title { get; set; }

        public string Description { get; set; }

        
        public Stream Content { get; set; }

        public FileDescription()
        {
          //  CreationDate = DateTime.UtcNow;
        }
    }
}
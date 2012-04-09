#region

using System;

#endregion

namespace RavenQuestionnaire.Core.Documents
{
    public class FileDocument
    {
        public string Id { get; set; }

        public string UserId { get; set; }

        public DateTime CreationDate { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Filename { get; set; }

        public string Thumbnail { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public int ThumbnailWidth { get; set; }

        public int ThumbnailHeight { get; set; }


        public FileDocument()
        {
            CreationDate = DateTime.UtcNow;
        }
    }
}
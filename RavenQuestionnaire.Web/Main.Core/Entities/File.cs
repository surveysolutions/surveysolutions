using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Entities
{
    public class File : IEntity<FileDocument>
    {
        private FileDocument innerDocument;

        public string FileId { get { return innerDocument.Id; } }

        public File(FileDocument innerDocument)
        {
            this.innerDocument = innerDocument;
        }

        public File(string title, string description, string filename, int originalWidth, int originalHeight, string thumbnail, int thumbHeight, int thumbWidth, string userId)
        {
            innerDocument = new FileDocument
                                {
                                    UserId = userId,
                                    Title = title,
                                    Description = description,
                                    Filename = filename,
                                    Width = originalWidth,
                                    Height = originalHeight,
                                    Thumbnail = thumbnail,
                                    ThumbnailHeight = thumbHeight,
                                    ThumbnailWidth = thumbWidth
                                };
        }

        public FileDocument GetInnerDocument()
        {
            return this.innerDocument;
        }

        public void UpdateMeta(string title, string description)
        {
            innerDocument.Description = description;
            innerDocument.Title = title;
        }
    }
}

using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.File
{
    public class UploadFileCommand : ICommand
    {
        public UploadFileCommand(string title, string desc, byte[] thumbData, int thumbWidth, int thumbHeight, byte[] origData, int origWidth, int origHeight, UserLight executor)
        {
            Executor = executor;
            Description = desc;
            Title = title;
            OriginalImage = origData;
            ThumbnailImage = thumbData;
            OriginalWidth = origWidth;
            OriginalHeight = origHeight;
            ThumbHeight = thumbHeight;
            ThumbWidth = thumbWidth;
        }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public byte[] OriginalImage { get; private set; }

        public int OriginalWidth { get; private set; }

        public int OriginalHeight { get; private set; }

        public int ThumbWidth { get; private set; }

        public int ThumbHeight { get; private set; }

        public string OriginalFile { get; set; }

        public string ThumbFile { get; set; }

        public byte[] ThumbnailImage { get; private set; }

        #region ICommand Members

        public UserLight Executor { get; set; }

        #endregion
    }
}
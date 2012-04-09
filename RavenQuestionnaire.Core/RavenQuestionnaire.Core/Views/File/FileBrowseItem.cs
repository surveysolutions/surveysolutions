using System;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.File
{
    public class FileBrowseItem
    {
        public string Id
        {
            get { return IdUtil.ParseId(_id); }
            set { _id = value; }
        }

        private string _id;
        public string Title {  get; set; }
        public string Description { get; set; }
        public string Original { get; set; }
        private string _thumbnail;
        public string Thumbnail
        {
            get { return IdUtil.ParseId(_thumbnail); }
            set { _thumbnail = value; }
        }

        public int Width { get; set; }
        public int Height { get; set; }
        public int ThumbWidth { get; set; }
        public int ThumbHeight { get; set; }

        public DateTime CreationDate { get; private set; }

        public FileBrowseItem()
        {
        }

        public FileBrowseItem(string id, string title, string description, DateTime createDate, string original,
            int width, int height, string thumbnail, int thumbWidth, int thumbHeight)
        {
            Id = id;
            Title = title;
            Description = description;
            CreationDate = createDate;
            Original = IdUtil.ParseId(original);
            Width = width;
            Height = height;
            Thumbnail = thumbnail;
            ThumbHeight = thumbHeight;
            ThumbWidth = thumbWidth;
        }

        public static FileBrowseItem New()
        {
            return new FileBrowseItem();
        }
    }
}

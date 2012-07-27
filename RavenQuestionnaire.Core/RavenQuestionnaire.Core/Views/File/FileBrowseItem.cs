using System;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.File
{
    public class FileBrowseItem
    {
        public Guid Id { get; set; }

        public string Title {  get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
       
        public int Width { get; set; }
        public int Height { get; set; }


        public FileBrowseItem()
        {
        }

        public FileBrowseItem(Guid id, string title, string description, string fileName,
            int width, int height)
        {
            Id = id;
            Title = title;
            Description = description;
            FileName = IdUtil.ParseId(fileName);
            Width = width;
            Height = height;
        }

        public static FileBrowseItem New()
        {
            return new FileBrowseItem();
        }
    }
}

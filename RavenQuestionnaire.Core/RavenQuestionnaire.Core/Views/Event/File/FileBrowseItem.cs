using System;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.File
{
    public class FileBrowseItem
    {
    //    public Guid Id { get; set; }

        public string Title {  get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
       


        public FileBrowseItem()
        {
        }

        public FileBrowseItem(/*Guid id,*/ string title, string description, string fileName)
        {
         //   Id = id;
            Title = title;
            Description = description;
            FileName = fileName;
        }

        public static FileBrowseItem New()
        {
            return new FileBrowseItem();
        }
    }
}

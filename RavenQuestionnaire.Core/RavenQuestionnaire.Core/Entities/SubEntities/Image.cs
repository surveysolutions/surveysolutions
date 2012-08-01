#region

using System;

#endregion

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public class Image
    {
        public Guid PublicKey { get; set; }
        public DateTime CreationDate { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

     /*   public string OriginalBase64 { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public string ThumbnailBase { get; set; }
        public int ThumbnailWidth { get; set; }
        public int ThumbnailHeight { get; set; }*/
    }
}
using System;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Views.Card
{
    public class CardView
    {
        public CardView()
        {
        }

        public CardView(Guid questionPublicKey, Image doc)
        {
            Title = doc.Title;
            Description = doc.Description;
            Original = doc.OriginalBase64;
            Width = doc.Width;
            Height = doc.Height;

            Thumb = doc.ThumbnailBase64;
            ThumbHeight = doc.ThumbnailHeight;
            ThumbWidth = doc.ThumbnailWidth;

            QuestionId = questionPublicKey;
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Original { get; set; }
        public string Thumb { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int ThumbWidth { get; set; }
        public int ThumbHeight { get; set; }
        public Guid QuestionId { get; set; }
    }
}
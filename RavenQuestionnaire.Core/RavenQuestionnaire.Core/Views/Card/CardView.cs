using System;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Card
{
    public class CardView
    {
        public CardView()
        {
        }

        public CardView(Guid questionPublicKey, Image doc)
        {
            PublicKey = doc.PublicKey;
            Title = doc.Title;
            Description = doc.Description;
            ThumbPublicKey = doc.ThumbPublicKey;
          /*  Original = String.Format("{0}.png", doc.PublicKey);
            Thumb = String.Format("{0}_thumb.png", doc.PublicKey);*/
           // Original = IdUtil.ParseId(doc.OriginalBase64);
         /*   Width = doc.Width;
            Height = doc.Height;*/

          //  Thumb = IdUtil.ParseId(doc.ThumbnailBase);
           /* ThumbHeight = doc.ThumbnailHeight;
            ThumbWidth = doc.ThumbnailWidth;*/

            QuestionId = questionPublicKey;
        }

        public Guid PublicKey { get; set; }
        public Guid ThumbPublicKey { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
       /* public string Original { get; set; }
        public string Thumb { get; set; }*/
     /*   public int Width { get; set; }
        public int Height { get; set; }
        public int ThumbWidth { get; set; }
        public int ThumbHeight { get; set; }*/
        public Guid QuestionId { get; set; }
    }
}
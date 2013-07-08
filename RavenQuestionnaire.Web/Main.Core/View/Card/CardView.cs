using System;
using Main.Core.Entities.SubEntities;

namespace Main.Core.View.Card
{
    /// <summary>
    /// The card view.
    /// </summary>
    public class CardView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CardView"/> class.
        /// </summary>
        public CardView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CardView"/> class.
        /// </summary>
        /// <param name="questionPublicKey">
        /// The question public key.
        /// </param>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public CardView(Guid questionPublicKey, Image doc)
        {
            this.PublicKey = doc.PublicKey;
            this.Title = doc.Title;
            this.Description = doc.Description;

            /*  Original = String.Format("{0}.png", doc.PublicKey);
            Thumb = String.Format("{0}_thumb.png", doc.PublicKey);*/
            // Original = IdUtil.ParseId(doc.OriginalBase64);
            /*   Width = doc.Width;
            Height = doc.Height;*/

            // Thumb = IdUtil.ParseId(doc.ThumbnailBase);
            /* ThumbHeight = doc.ThumbnailHeight;
            ThumbWidth = doc.ThumbnailWidth;*/
            this.QuestionId = questionPublicKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /* public string Original { get; set; }
        public string Thumb { get; set; }*/
        /*   public int Width { get; set; }
        public int Height { get; set; }
        public int ThumbWidth { get; set; }
        public int ThumbHeight { get; set; }*/

        /// <summary>
        /// Gets or sets the question id.
        /// </summary>
        public Guid QuestionId { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        #endregion
    }
}
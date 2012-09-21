// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnswerView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The answer view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace Main.Core.View.Answer
{
    /// <summary>
    /// The answer view.
    /// </summary>
    public class AnswerView : ICompositeView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AnswerView"/> class.
        /// </summary>
        public AnswerView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnswerView"/> class.
        /// </summary>
        /// <param name="questionPublicKey">
        /// The question public key.
        /// </param>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public AnswerView(Guid questionPublicKey, IAnswer doc)
        {
            this.PublicKey = doc.PublicKey;
            this.Title = doc.AnswerText;
            this.AnswerValue = this.GetAnswerValue(doc.AnswerValue);
            this.AnswerImage = doc.AnswerImage;
            this.Mandatory = doc.Mandatory;
            this.AnswerType = doc.AnswerType;
            this.Parent = questionPublicKey;
            this.NameCollection = doc.NameCollection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnswerView"/> class.
        /// </summary>
        /// <param name="questionPublicKey">
        /// The question public key.
        /// </param>
        public AnswerView(Guid questionPublicKey)
        {
            this.Parent = questionPublicKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the answer image.
        /// </summary>
        public string AnswerImage { get; set; }

        /// <summary>
        /// Gets or sets the answer type.
        /// </summary>
        public AnswerType AnswerType { get; set; }

        /// <summary>
        /// Gets or sets the answer value.
        /// </summary>
        public string AnswerValue { get; set; }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public List<ICompositeView> Children { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether mandatory.
        /// </summary>
        public bool Mandatory { get; set; }

        /// <summary>
        /// Gets or sets the name collection.
        /// </summary>
        public string NameCollection { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        public Guid? Parent { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// The get answer value.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        protected string GetAnswerValue(object value)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return string.Empty;
            }

            /*   try
            {
                return DateTime.Parse(value.ToString()).ToString(@"MM/dd/yyyy", DateTimeFormatInfo.InvariantInfo);
            }
            catch (FormatException)
            {*/
            return value.ToString();

            // }
        }

        #endregion
    }
}
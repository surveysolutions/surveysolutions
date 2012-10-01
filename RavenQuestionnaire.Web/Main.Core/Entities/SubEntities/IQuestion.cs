// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IQuestion.cs" company="">
//   
// </copyright>
// <summary>
//   The Question interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Entities.SubEntities
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.Composite;
    using Main.Core.Entities.Observers;

    /// <summary>
    /// The Question interface.
    /// </summary>
    public interface IQuestion : IComposite
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the answer order.
        /// </summary>
        Order AnswerOrder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether capital.
        /// </summary>
        bool Capital { get; set; }

        /// <summary>
        /// Gets or sets the cards.
        /// </summary>
        List<Image> Cards { get; set; }

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        string Comments { get; set; }

        /// <summary>
        /// Gets or sets the condition expression.
        /// </summary>
        string ConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether featured.
        /// </summary>
        bool Featured { get; set; }

        /// <summary>
        /// Gets or sets the instructions.
        /// </summary>
        string Instructions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether mandatory.
        /// </summary>
        bool Mandatory { get; set; }

        /// <summary>
        /// Gets or sets the question text.
        /// </summary>
        string QuestionText { get; set; }

        /// <summary>
        /// Gets or sets the question type.
        /// </summary>
        QuestionType QuestionType { get; set; }

        /// <summary>
        /// Gets or sets the stata export caption.
        /// </summary>
        string StataExportCaption { get; set; }

        /// <summary>
        /// Gets or sets the validation expression.
        /// </summary>
        string ValidationExpression { get; set; }

        /// <summary>
        /// Gets or sets the validation message.
        /// </summary>
        string ValidationMessage { get; set; }

        /// <summary>
        /// Gets or sets Triggers.
        /// </summary>
        List<Guid> Triggers { get; set; }

        #endregion
    }
}
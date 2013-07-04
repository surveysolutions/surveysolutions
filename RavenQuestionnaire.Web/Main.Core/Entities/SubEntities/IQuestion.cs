namespace Main.Core.Entities.SubEntities
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.Composite;
    

    /// <summary>
    /// The Question interface.
    /// </summary>
    public interface IQuestion : IComposite, IConditional
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        List<IAnswer> Answers { get; set; }

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
        /// Gets or sets a value indicating whether featured.
        /// Is used for visual item distinguish.
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
        QuestionType QuestionType { get; set; } ////must be deleted

        /// <summary>
        /// Gets or sets the question scope.
        /// </summary>
        QuestionScope QuestionScope { get; set; } 

        /// <summary>
        /// Gets or sets the Stata export caption.
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
        /// Gets or sets the conditional dependent questions.
        /// </summary>
        List<Guid> ConditionalDependentQuestions { get; set; }

        /// <summary>
        /// Gets or sets the dependent items.
        /// </summary>
        List<Guid> ConditionalDependentGroups { get; set; }

        #endregion

        /// <summary>
        /// The add answer.
        /// </summary>
        /// <param name="answer">
        /// The answer.
        /// </param>
        void AddAnswer(IAnswer answer);

        /*/// <summary>
        /// The remove answer.
        /// </summary>
        /// <param name="answerKey">
        /// The answer key.
        /// </param>
        void RemoveAnswer(Guid answerKey);*/
    }
}
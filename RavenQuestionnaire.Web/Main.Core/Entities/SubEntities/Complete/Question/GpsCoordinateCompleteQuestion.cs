namespace Main.Core.Entities.SubEntities.Complete.Question
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Entities.Composite;

    /// <summary>
    /// The gps coordinate complete question.
    /// </summary>
    public sealed class GpsCoordinateCompleteQuestion : AbstractCompleteQuestion, IGpsCoordinatesQuestion, ICompelteValueQuestion<GeoPosition>
    {

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GpsCoordinateCompleteQuestion"/> class.
        /// </summary>
        public GpsCoordinateCompleteQuestion()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GpsCoordinateCompleteQuestion"/> class.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        public GpsCoordinateCompleteQuestion(string text)
            : base(text)
        {
        }

        #endregion

        #region Public Methods and Operators
        
        public override void AddAnswer(IAnswer answer)
        {
            throw new NotImplementedException();
        }

        
        /// <summary>
        /// The get answer object.
        /// </summary>
        /// <returns>
        /// The System.Object.
        /// </returns>
        public override object GetAnswerObject()
        {
            return this.Answer;
        }

        /// <summary>
        /// The is answered.
        /// </summary>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public override bool IsAnswered()
        {
            return this.Answer != null;
        }

        /// <summary>
        /// The get answer string.
        /// </summary>
        /// <returns>
        /// The System.String.
        /// </returns>
        public override string GetAnswerString()
        {
            return this.Answer == null ? string.Empty : this.Answer.ToString();
        }

        /// <summary>
        /// The set answer.
        /// </summary>
        /// <param name="answer">
        /// The answer.
        /// </param>
        /// <param name="answerValue">
        /// The answer value.
        /// </param>
        public override void SetAnswer(List<Guid> answer, string answerValue)
        {
            //this.Answer = answerValue;
        }

        public override void ThrowDomainExceptionIfAnswerInvalid(List<Guid> answerKeys, string answerValue)
        {
            
        }

        #endregion

        #region Implementation of ICompelteValueQuestion<string>

        public GeoPosition Answer { get; set; }

        #endregion

    }
}
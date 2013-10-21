using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities.Complete;

namespace Main.Core.Entities.SubEntities.Question
{
    public class GpsCoordinateQuestion : AbstractQuestion, IGpsCoordinatesQuestion
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GpsCoordinateQuestion"/> class.
        /// </summary>
        public GpsCoordinateQuestion()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GpsCoordinateQuestion"/> class.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        public GpsCoordinateQuestion(string text)
            : base(text)
        {
        }

        #endregion
        

        #region Public Methods and Operators

        /// <summary>
        /// The add answer.
        /// </summary>
        /// <param name="answer">
        /// The answer.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public override void AddAnswer(IAnswer answer)
        {
            throw new NotImplementedException();
        }
        
        public override T Find<T>(Guid publicKey)
        {
            return null;
        }

        
        public override IEnumerable<T> Find<T>(Func<T, bool> condition)
        {
            return Enumerable.Empty<T>();
        }

        /// <summary>
        /// The first or default.
        /// </summary>
        /// <param name="condition">
        /// The condition.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The T.
        /// </returns>
        public override T FirstOrDefault<T>(Func<T, bool> condition)
        {
            return null;
        }
        
        #endregion

        //public GeoPosition Position { get; set; }
    }
}
using System;

namespace Main.Core.View.Group
{
    /// <summary>
    /// The counter.
    /// </summary>
    public class Counter
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Counter"/> class.
        /// </summary>
        public Counter()
        {
            this.Total = 0;
            this.Enablad = 0;
            this.Answered = 0;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the answered.
        /// </summary>
        public int Answered { get; set; }

        /// <summary>
        /// Gets or sets the enablad.
        /// </summary>
        public int Enablad { get; set; }

        /// <summary>
        /// Gets the progress.
        /// </summary>
        public int Progress
        {
            get
            {
                if (this.Enablad < 1)
                {
                    return 0;
                }

                return (int)Math.Round(100 * ((double)this.Answered / this.Enablad));
            }
        }

        /// <summary>
        /// Gets or sets the total.
        /// </summary>
        public int Total { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The +.
        /// </summary>
        /// <param name="a">
        /// The a.
        /// </param>
        /// <param name="b">
        /// The b.
        /// </param>
        /// <returns>
        /// </returns>
        public static Counter operator +(Counter a, Counter b)
        {
            return new Counter
                {
                   Total = a.Total + b.Total, Enablad = a.Enablad + b.Enablad, Answered = a.Answered + b.Answered 
                };
        }

        #endregion
    }
}
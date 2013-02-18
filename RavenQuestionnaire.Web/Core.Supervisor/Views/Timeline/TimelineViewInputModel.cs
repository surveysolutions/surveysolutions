// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimelineViewInputModel.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire statistic view input model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Core.Supervisor.Views.Timeline
{
    using System;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The complete questionnaire statistic view input model.
    /// </summary>
    public class TimelineViewInputModel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TimelineViewInputModel"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        public TimelineViewInputModel(Guid id)
        {
            this.Id = id;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the id.
        /// </summary>
        public Guid Id { get; private set; }

        #endregion
    }
}
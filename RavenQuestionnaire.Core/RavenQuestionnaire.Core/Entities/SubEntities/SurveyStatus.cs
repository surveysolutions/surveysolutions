// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SurveyStatus.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The survey status.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The survey status.
    /// </summary>
    public class SurveyStatus
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyStatus"/> class.
        /// </summary>
        public SurveyStatus()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyStatus"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        public SurveyStatus(Guid id)
        {
            this.PublicId = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyStatus"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public SurveyStatus(Guid id, string name)
        {
            this.PublicId = id;
            this.Name = name;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the approve.
        /// </summary>
        public static SurveyStatus Approve
        {
            get
            {
                var identifier = new Guid("AA6C0DC1-23C4-4B03-A3ED-B24EF0055555");
                string name = "Approve";
                return new SurveyStatus(identifier, name);
            }
        }

        /// <summary>
        /// Gets the complete.
        /// </summary>
        public static SurveyStatus Complete
        {
            get
            {
                var identifier = new Guid("776C0DC1-23C4-4B03-A3ED-B24EF005559B");
                string name = "Completed";
                return new SurveyStatus(identifier, name);
            }
        }

        /// <summary>
        /// Gets the error.
        /// </summary>
        public static SurveyStatus Error
        {
            get
            {
                var identifier = new Guid("D65CF1F6-8A75-43FA-9158-B745EB4D6A1F");
                string name = "Completed with Error";
                return new SurveyStatus(identifier, name);
            }
        }

        /// <summary>
        /// Gets the initial.
        /// </summary>
        public static SurveyStatus Initial
        {
            get
            {
                var identifier = new Guid("8927D124-3CFB-4374-AD36-2FD99B62CE13");
                string name = "Initial";
                return new SurveyStatus(identifier, name);
            }
        }

        /// <summary>
        /// Gets or sets the change comment.
        /// </summary>
        public string ChangeComment { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the public id.
        /// </summary>
        public Guid PublicId { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get all statuses.
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; RavenQuestionnaire.Core.Entities.SubEntities.SurveyStatus].
        /// </returns>
        public static IEnumerable<SurveyStatus> GetAllStatuses()
        {
            return new[] { Initial, Error, Complete, Approve };
        }

        /// <summary>
        /// check status on allowance to be pushed from capi
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static bool IsStatusAllowCapiSync(SurveyStatus status)
        {
            return status.PublicId == SurveyStatus.Complete.PublicId || status.PublicId == SurveyStatus.Error.PublicId;
        }

        #endregion
    }
}
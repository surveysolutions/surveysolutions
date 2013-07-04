using System;
using Main.Core.Utility;

namespace Core.CAPI.Views.Grouped
{
    /// <summary>
    /// The cq grouped browse input model.
    /// </summary>
    public class CQGroupedBrowseInputModel
    {
        #region Fields

        /// <summary>
        /// The _page.
        /// </summary>
        private int _page = 1;

        /// <summary>
        /// The _page size.
        /// </summary>
        private int _pageSize = 20;

        /// <summary>
        /// The _template questionanire id.
        /// </summary>
        private string _templateQuestionanireId;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the interviewer id.
        /// </summary>
        public Guid? InterviewerId { get; set; }

        /// <summary>
        /// Gets or sets the page.
        /// </summary>
        public int Page
        {
            get
            {
                return this._page;
            }

            set
            {
                this._page = value;
            }
        }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize
        {
            get
            {
                return this._pageSize;
            }

            set
            {
                this._pageSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the template questionanire id.
        /// </summary>
        public string TemplateQuestionanireId
        {
            get
            {
                return this._templateQuestionanireId;
            }

            set
            {
                this._templateQuestionanireId = value;
            }
        }

        #endregion
    }
}
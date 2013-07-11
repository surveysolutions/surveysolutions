// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.Entities;

namespace Core.Supervisor.Views.Survey
{
    public class SurveysViewItem
    {
        #region Constructors and Destructors

        public SurveysViewItem()
        {
        }

        public SurveysViewItem(
            TemplateLight template,
            int total,
            int initial,
            int error,
            int completed,
            int approve,
            int redo,
            int unassigned)
            : this()
        {
            this.Template = template;
            this.Total = total;
            this.Initial = initial;
            this.Error = error;
            this.Completed = completed;
            this.Approved = approve;
            this.Redo = redo;
            this.Unassigned = unassigned;
        }

        #endregion

        #region Public Properties

        public int Unassigned { get; set; }
        /// <summary>
        /// Gets or sets Approve.
        /// </summary>
        public int Approved { get; set; }

        /// <summary>
        /// Gets or sets the complete.
        /// </summary>
        public int Completed { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        public int Error { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public TemplateLight Template { get; set; }

        /// <summary>
        /// Gets or sets the initial.
        /// </summary>
        public int Initial { get; set; }

        /// <summary>
        /// Gets or sets Redo.
        /// </summary>
        public int Redo { get; set; }

        /// <summary>
        /// Gets or sets the total.
        /// </summary>
        public int Total { get; set; }

        #endregion
    }
}
namespace Core.Supervisor.Views.Index
{
    using System;

    /// <summary>
    /// The survey browse item.
    /// </summary>
    public class IndexViewItem
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexViewItem"/> class.
        /// </summary>
        public IndexViewItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexViewItem"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="unassigment">
        /// The un assigment.
        /// </param>
        /// <param name="total">
        /// The total.
        /// </param>
        /// <param name="initial">
        /// The initial.
        /// </param>
        /// <param name="error">
        /// The error.
        /// </param>
        /// <param name="completed">
        /// The completed.
        /// </param>
        /// <param name="approve">
        /// The approve.
        /// </param>
        public IndexViewItem(
            Guid id, 
            string title, 
            int unassigment, 
            int total, 
            int initial, 
            int error, 
            int completed, 
            int approve,
            int redo)
            : this()
        {
            this.Id = id;
            this.Title = title;
            this.Unassigned = unassigment;
            this.Total = total;
            this.Initial = initial;
            this.Error = error;
            this.Completed = completed;
            this.Approved = approve;
            this.Redo = redo;
        }

        #endregion

        #region Public Properties

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
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the initial.
        /// </summary>
        public int Initial { get; set; }

        /// <summary>
        /// Gets or sets Redo.
        /// </summary>
        public int Redo { get; set; }
        
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the total.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Gets or sets the unassigned.
        /// </summary>
        public int Unassigned { get; set; }

        #endregion
    }
}